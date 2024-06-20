using backup_manager.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using Tftp.Net;
using backup_manager.Model;

namespace backup_manager
{
    internal class SftpServer : ISftpServer
    {
        private bool isFinished;
        private string serverDir;
        private readonly ILogger<SftpServer> logger;
        private readonly ISshWorker sshWorker;

        public SftpServer(ILogger<SftpServer> logger, ISshWorker sshWorker)
        {
            this.logger = logger;
            this.sshWorker = sshWorker;
        }
        public bool RunSftpServer(string sftpTempPath, string backupServerAddress, string backupCmd)
        {
            serverDir = sftpTempPath;

            if (!Directory.Exists(serverDir))
                Directory.CreateDirectory(serverDir);

            logger.LogInformation("Running TFTP server. Temp directory: " + serverDir);

            using (var server = new TftpServer())
            {
                server.OnReadRequest += new TftpServerEventHandler(Server_OnReadRequest);
                server.OnWriteRequest += new TftpServerEventHandler(Server_OnWriteRequest);

                server.Start();

                while (!isFinished)
                {
                }

                logger.LogInformation("Tftp server completed dl request.");
            }

            return isFinished;
        }
        public async Task<bool> RunSftpServerAsync(string tempDir, string backupServerAddress, int serverDlTimeRangeInMs = 30000)
        {
            InitServerDir(tempDir);

            using (var server = new TftpServer())
            {
                server.OnReadRequest += new TftpServerEventHandler(Server_OnReadRequest);
                server.OnWriteRequest += new TftpServerEventHandler(Server_OnWriteRequest);

                server.Start();

                while (!isFinished)
                {
                    logger.LogInformation("Waiting for connection ...");

                    await Task.Delay(serverDlTimeRangeInMs);
                }

                logger.LogInformation("Tftp server completed dl requests.");
            }

            return isFinished;
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
            isFinished = true;
        }
        private void Transfer_OnError(ITftpTransfer transfer, TftpTransferError error)
        {
            OutputTransferStatus(transfer, "Error: " + error);
            isFinished = true;
        }
        private void Transfer_OnFinished(ITftpTransfer transfer)
        {
            OutputTransferStatus(transfer, "Finished: " + transfer.Filename);
            isFinished = true;
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