using backup_manager.Interfaces;
using LumiSoft.Net.FTP.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tftp.Net;

namespace backup_manager.Servers
{
    internal class FtpServer : IFtpServer
    {
        private readonly ILogger<TftpServer> logger;

        private string serverDir;

        public FtpServer(ILogger<TftpServer> logger)
        {
            this.logger = logger;
        }
        public async Task<bool> RunFtpServerAsync(string tempDir, string backupServerAddress, int serverDlTimeRangeInMs = 30000)
        {
            InitServerDir(tempDir);

            using (var ftpServer = new FTP_Server())
            {
                ftpServer.Start();

                ftpServer.Error += FtpServer_Error;
                ftpServer.Started += FtpServer_Started;
                ftpServer.SessionCreated += FtpServer_SessionCreated;

                while (true)
                {
                    logger.LogInformation("Waiting for connection ...");

                    await Task.Delay(serverDlTimeRangeInMs);
                    break;
                }

                logger.LogInformation("Ftp server completed dl requests.");
            }

            return true;
        }
        public void RunFtpServer(string tempDir)
        {
            InitServerDir(tempDir);

            FTP_Server ftpServer = new();
            ftpServer.Start();

            ftpServer.Error += FtpServer_Error;
            ftpServer.Started += FtpServer_Started;
            ftpServer.SessionCreated += FtpServer_SessionCreated;
        }
        private void InitServerDir(string dir)
        {
            serverDir = dir;

            if (!Directory.Exists(serverDir))
                Directory.CreateDirectory(serverDir);

            logger.LogInformation("Init TFTP server. Temp directory: " + serverDir);
        }
        public async Task<bool> RunFtpServerAsync(string tempDir, int serverDlTimeRangeInMs = 30000)
        {
            InitServerDir(tempDir);

            FTP_Server ftpServer = new();
            ftpServer.Start();

            logger.LogInformation($"Is ftp server running? {ftpServer.IsRunning}");

            ftpServer.Error += FtpServer_Error;
            ftpServer.Started += FtpServer_Started;
            ftpServer.SessionCreated += FtpServer_SessionCreated;

            while (true)
            {
                logger.LogInformation("FTP server waiting for connection ...");

                await Task.Delay(serverDlTimeRangeInMs);
                break;
            }

            return true;
        }
        private void FtpServer_Error(object sender, LumiSoft.Net.Error_EventArgs e)
        {
            logger.LogError($"Ftp server error {e.Exception}");
        }
        private void FtpServer_Started(object? sender, EventArgs e)
        {
            logger.LogInformation($"Ftp server started.");
        }

        private void FtpServer_SessionCreated(object? sender, LumiSoft.Net.TCP.TCP_ServerSessionEventArgs<FTP_Session> e)
        {
            logger.LogInformation($"Session {e.Session.ID} created.");

            e.Session.CurrentDir = serverDir;
        }
    }
}