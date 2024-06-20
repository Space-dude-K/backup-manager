using backup_manager.Interfaces;
using backup_manager.Model;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Net;
using static backup_manager.Model.Enums;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace backup_manager
{
    class BackupManager : IBackupManager
    {
        private readonly ILogger<BackupManager> loggerManager;
        private readonly ISftpServer sftpServer;
        private readonly ISshWorker sshWorker;
        private readonly ISshShellWorker sshShellWorker;

        public BackupManager(ILogger<BackupManager> loggerManager, ISftpServer sftpServer, ISshWorker sshWorker, ISshShellWorker sshShellWorker)
        {
            this.loggerManager = loggerManager;
            this.sftpServer = sftpServer;
            this.sshWorker = sshWorker;
            this.sshShellWorker = sshShellWorker;
        }
        public async Task Init(List<Device> devices, List<string> backupLocations, string backupSftpFolder)
        {
            Task managerTask = null;

            loggerManager.LogInformation($"Backup manager init for {devices.Count} and {backupLocations.Count} paths.");

            List<Task> tasks = [];

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

                // TODO: Add parallel execution
                managerTask = sftpServer.RunSftpServerAsync(backupSftpFolder, Utils.GetLocalIPAddress());

                foreach(var device in devices)
                {
                    var dtStr = DateTime.Now.ToString("ddMMyyyy.fff", CultureInfo.InvariantCulture);
                    var deviceNameAndSn = Utils.RemoveInvalidChars(device.Name + "_" + device.SerialNumber);
                    var fileName = (deviceNameAndSn + "_" + dtStr + ".cfg")
                        .GetCleanFileName();
                    var backupServerAddress = Utils.GetLocalIPAddress();
                    var l = Path.Combine(backupSftpFolder, Utils.GetFolderNamePartForBackupParent(device.BackupCmdType),
                        deviceNameAndSn);
                    var backupCmd =
                        device.BackupCmdType.GetDisplayAttributeFrom(typeof(BackupCmdTypes))
                        .Replace("%addr%", backupServerAddress)
                        .Replace("%file%", fileName);

                    switch (device.BackupCmdType)
                    {
                        case BackupCmdTypes.HP:
                            break;
                        case BackupCmdTypes.HP_shell:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            //loggerManager.LogInformation("Add task.");
                            //tasks.Add(Task.Run(() => sshWorker.ConnectAndExecute(device, backupCmd)));
                            //sshWorker.ConnectAndExecute(device, backupCmd);
                            break;
                    }  
                }

                await Task.WhenAll(tasks);
            }

            await managerTask;
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
