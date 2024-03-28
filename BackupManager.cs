using backup_manager.Interfaces;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System;

namespace backup_manager
{
    class BackupManager
    {
        private readonly ILogger<BackupManager> loggerManager;
        private readonly ISftpServer sftpServer;

        public BackupManager(ILogger<BackupManager> loggerManager, ISftpServer sftpServer)
        {
            this.loggerManager = loggerManager;
            this.sftpServer = sftpServer;
        }
        public void Init()
        {
            loggerManager.LogInformation("Test!");
        }
        string ConnectAndDownload(string sshClientAddress, string backupCmd)
        {
            

            using (var client = new SshClient(sshClientAddress, "admin", "VMGPa$$w0rd"))
            {
                client.Connect();

                Console.WriteLine($"Conn info: {client.ConnectionInfo.Host + " "
                    + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");
                Console.WriteLine($"Run cmd -> {backupCmd}");

                var cmd = client.RunCommand(backupCmd);
                cmd.CommandTimeout = new TimeSpan(0, 0, 0, 50);
                var execRes = cmd.Execute();

                if (!string.IsNullOrEmpty(cmd.Error))
                    Console.WriteLine($"Error: {cmd.Error}");

                Console.WriteLine($"Exec results: {execRes}");

                client.Disconnect();

                return execRes;
            }
        }
    }
}
