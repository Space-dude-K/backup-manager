using backup_manager.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using Tftp.Net;
using backup_manager.Model;
using System.IO.Compression;

namespace backup_manager.Servers
{
    internal class TftpServer : ITftpServer
    {
        private string serverDir;
        private readonly ILogger<TftpServer> logger;
        private readonly ISshWorker sshWorker;

        public TftpServer(ILogger<TftpServer> logger, ISshWorker sshWorker)
        {
            this.logger = logger;
            this.sshWorker = sshWorker;
        }
        public async Task<bool> RunTftpServerAsync(string tempDir, string backupServerAddress, int serverDlTimeRangeInMs = 30000)
        {
            InitServerDir(tempDir);

            using (var server = new Tftp.Net.TftpServer())
            {
                server.OnReadRequest += new TftpServerEventHandler(Server_OnReadRequest);
                server.OnWriteRequest += new TftpServerEventHandler(Server_OnWriteRequest);

                server.Start();

                while (true)
                {
                    logger.LogInformation("Waiting for connection ...");

                    await Task.Delay(serverDlTimeRangeInMs);
                    break;
                }

                logger.LogInformation("Tftp server completed dl requests.");
            }

            return true;
        }
        private void InitServerDir(string dir)
        {
            serverDir = dir;

            if (!Directory.Exists(serverDir))
                Directory.CreateDirectory(serverDir);

            logger.LogInformation("Init TFTP server. Temp directory: " + serverDir);
        }
        private void Server_OnWriteRequest(ITftpTransfer transfer, EndPoint client)
        {
            string file = Path.Combine(serverDir, transfer.Filename);

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
            string path = Path.Combine(serverDir, transfer.Filename);

            logger.LogInformation($"Server dl: {path}");

            FileInfo file = new(path);

            // Is the file within the server directory?
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
        // TODO. Archive
        private void Transfer_OnFinished(ITftpTransfer transfer)
        {
            OutputTransferStatus(transfer, "Finished: " + transfer.Filename);

            /*var fileName = Path.GetFileNameWithoutExtension(transfer.Filename);
            var fileNameFullPath = Path.Combine(serverDir, transfer.Filename);
            var zipFile = fileName + ".zip";
            var zipDir = Path.Combine(serverDir, "Archive");

            if(!Directory.Exists(zipDir))
                Directory.CreateDirectory(zipDir);

            var zipFileFullPath = Path.Combine(zipDir, zipFile);

            logger.LogInformation($"New archive -> {zipFileFullPath}");

            //Utils.GetWriteStream
            //Utils.SafelyCreateZipFromDirectory(fileNameFullPath, zipFileFullPath);

            Task.Run(() => Utils.CreateArchive(zipFileFullPath, zipFileFullPath, zipFile));*/
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