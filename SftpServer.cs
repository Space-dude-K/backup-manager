using backup_manager.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using Tftp.Net;

namespace backup_manager
{
    internal class SftpServer : ISftpServer
    {
        private string serverDir;
        private readonly ILogger<SftpServer> logger;

        public SftpServer(ILogger<SftpServer> logger)
        {
            this.logger = logger;
        }
        public void RunSftpServer(string sftpTempPath)
        {
            serverDir = sftpTempPath;
            logger.LogInformation("Running TFTP server. Temp directory: " + serverDir);

            using (var server = new TftpServer())
            {
                server.OnReadRequest += new TftpServerEventHandler(Server_OnReadRequest);
                server.OnWriteRequest += new TftpServerEventHandler(Server_OnWriteRequest);
                server.Start();
                //Console.Read();
            }
        }
        private void Server_OnWriteRequest(ITftpTransfer transfer, EndPoint client)
        {
            string backupSubFolder = "";
            String file = Path.Combine(serverDir, backupSubFolder, transfer.Filename);

            if (File.Exists(file))
            {
                CancelTransfer(transfer, TftpErrorPacket.FileAlreadyExists);
            }
            else
            {
                OutputTransferStatus(transfer, "Accepting write request from " + client);
                StartTransfer(transfer, new FileStream(file, FileMode.CreateNew));
            }
        }
        private void Server_OnReadRequest(ITftpTransfer transfer, EndPoint client)
        {
            String path = Path.Combine(serverDir, transfer.Filename);
            FileInfo file = new(path);

            //Is the file within the server directory?
            if (!file.FullName.StartsWith(serverDir, StringComparison.InvariantCultureIgnoreCase))
            {
                CancelTransfer(transfer, TftpErrorPacket.AccessViolation);
            }
            else if (!file.Exists)
            {
                CancelTransfer(transfer, TftpErrorPacket.FileNotFound);
            }
            else
            {
                OutputTransferStatus(transfer, "Accepting request from " + client);
                StartTransfer(transfer, new FileStream(file.FullName, FileMode.Open, FileAccess.Read));
            }
        }
        private void StartTransfer(ITftpTransfer transfer, Stream stream)
        {
            transfer.OnProgress += new TftpProgressHandler(Transfer_OnProgress);
            transfer.OnError += new TftpErrorHandler(Transfer_OnError);
            transfer.OnFinished += new TftpEventHandler(Transfer_OnFinished);
            transfer.Start(stream);
        }
        private void CancelTransfer(ITftpTransfer transfer, TftpErrorPacket reason)
        {
            OutputTransferStatus(transfer, "Cancelling transfer: " + reason.ErrorMessage);
            transfer.Cancel(reason);
        }
        private void Transfer_OnError(ITftpTransfer transfer, TftpTransferError error)
        {
            OutputTransferStatus(transfer, "Error: " + error);
        }
        private void Transfer_OnFinished(ITftpTransfer transfer)
        {
            OutputTransferStatus(transfer, "Finished");
        }
        private void Transfer_OnProgress(ITftpTransfer transfer, TftpTransferProgress progress)
        {
            OutputTransferStatus(transfer, "Progress " + progress);
        }
        private void OutputTransferStatus(ITftpTransfer transfer, string message)
        {
            logger.LogInformation("[" + transfer.Filename + "] " + message);
        }
    }
}