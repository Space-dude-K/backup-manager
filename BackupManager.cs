using backup_manager.Interfaces;
using backup_manager.Model;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System;
using System.IO.Pipes;
using static backup_manager.Model.Enums;

namespace backup_manager
{
    class BackupManager : IBackupManager
    {
        private readonly ILogger<BackupManager> loggerManager;
        private readonly ISftpServer sftpServer;

        public BackupManager(ILogger<BackupManager> loggerManager, ISftpServer sftpServer)
        {
            this.loggerManager = loggerManager;
            this.sftpServer = sftpServer;
        }
        public void Init(List<Device> devices, List<string> backupLocations)
        {
            loggerManager.LogInformation($"Backup manager init for {devices.Count} and {backupLocations.Count} paths.");

            if(backupLocations.Count == 0)
            {
                loggerManager.LogInformation($"No copy paths was found.");
            }
            else if(devices.Count == 0)
            {
                loggerManager.LogInformation($"No devices was found.");
            }
            else
            {
                loggerManager.LogInformation($"Init backup process ...");

                foreach(var device in devices)
                {
                    switch(device.BackupCmdType)
                    {
                        case BackupCmdTypes.HP:

                            break;
                    }    
                }
            }
        }
        private void GetBackupType()
        {

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
