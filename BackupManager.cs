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
using backup_manager.Workers;
using backup_manager.BackupWorkers;

namespace backup_manager
{
    class BackupManager : IBackupManager
    {
        private readonly IServiceProvider serviceCollection;

        private readonly ILogger<BackupManager> loggerManager;
        private readonly ITftpServer tftpServer;
        private readonly ISftpServer sftpServer;
        private readonly ISshWorker sshWorker;

        //private readonly ISshShellWorker sshShellWorker;
        private readonly ISshShellWorker sshShellWorker;

        public BackupManager(IServiceProvider serviceCollection, ILogger<BackupManager> loggerManager, ITftpServer tftpServer, ISftpServer sftpServer,
            ISshWorker sshWorker, ISshShellWorker sshShellWorker)
        {
            this.serviceCollection = serviceCollection;
            this.loggerManager = loggerManager;
            this.tftpServer = tftpServer;
            this.sftpServer = sftpServer;
            this.sshWorker = sshWorker;
            this.sshShellWorker = sshShellWorker;
        }
        public async Task Init(List<Device> devices, List<string> backupLocations, string backupSftpFolder)
        {
            List<Task> serverTasks = null;

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
                serverTasks = new List<Task>();
                serverTasks.Add(tftpServer.RunTftpServerAsync(backupSftpFolder, Utils.GetLocalIPAddress(), 120000));
                serverTasks.Add(sftpServer.RunSftpServerAsync(backupSftpFolder, 120000));

                var sshLogger = serviceCollection.GetRequiredService<ILogger<SshWorker>>();
                var sshShelllogger = serviceCollection.GetRequiredService<ILogger<SshShellWorker>>();

                foreach (var device in devices)
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
                        .Replace("%configName%", device.ConfigName)
                        .Replace("%addr%", backupServerAddress)
                        .Replace("%file%", fileName);

                    //loggerManager.LogInformation($"Backup cmd {backupCmd}");

                    switch (device.BackupCmdType)
                    {
                        case BackupCmdTypes.Default:
                            tasks.Add(Task.Run(() => new SshShellWorker(sshShelllogger).ConnectAndExecuteAsync(device, backupCmd)));
                            break;
                        case BackupCmdTypes.HP:
                        case BackupCmdTypes.QSFP28:
                            tasks.Add(Task.Run(() => new SshWorker(sshLogger).ConnectAndDownloadAsync(device, backupCmd)));
                            break;
                        case BackupCmdTypes.HP_shell:
                        case BackupCmdTypes.JL256A:
                        case BackupCmdTypes.JL072A:
                        case BackupCmdTypes.J9298A:
                        case BackupCmdTypes.J9774A:
                        case BackupCmdTypes.J9146A:
                        case BackupCmdTypes.J9145A:
                        case BackupCmdTypes.J9779A:
                        case BackupCmdTypes.J9148A:
                        case BackupCmdTypes.J9147A:
                        case BackupCmdTypes.J9773A:
                        case BackupCmdTypes.J9584A:
                        case BackupCmdTypes.Fortigate:
                        case BackupCmdTypes.AP_HP:
                            tasks.Add(Task.Run(() => new SshShellWorker(sshShelllogger).ConnectAndExecuteAsync(device, backupCmd, BackupCmdTypes.J9584A)));
                            break;
                        case BackupCmdTypes.Mikrotik:
                            var downloadCmd = "/tool fetch " +
                                "upload=yes " +
                                $"url=\"sftp://{backupServerAddress}/{fileName}\" " +
                                "user=admin password=admin " +
                                $"src-path={fileName + ".backup"} " +
                                $"src-address={device.Ip} " +
                                "port=32";
                            var deleteCmd = $"/file remove \"{fileName + ".backup"}\"";

                            tasks.Add(Task.Run(() => 
                            new SshWorker(sshLogger).ConnectAndDownloadMikrotikCfgAsync(device, backupCmd, downloadCmd, deleteCmd)));
                            break;
                    }
                }

                await Task.WhenAll(tasks);

                //await Task.Delay(10000);

                loggerManager.LogInformation($"Tasks comleted.");
            }

            await Task.WhenAll(serverTasks);
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
