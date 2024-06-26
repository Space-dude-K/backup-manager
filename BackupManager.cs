﻿using backup_manager.Interfaces;
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
        private readonly ITftpServer tftpServer;
        private readonly IFtpServer ftpServer;
        private readonly ISshWorker sshWorker;
        private readonly ISshShellWorker sshShellWorker;

        public BackupManager(ILogger<BackupManager> loggerManager, ITftpServer tftpServer, IFtpServer ftpServer,
            ISshWorker sshWorker, ISshShellWorker sshShellWorker)
        {
            this.loggerManager = loggerManager;
            this.tftpServer = tftpServer;
            this.ftpServer = ftpServer;
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
                //serverTasks.Add(tftpServer.RunSftpServerAsync(backupSftpFolder, Utils.GetLocalIPAddress(), 60000));
                serverTasks.Add(ftpServer.RunFtpServerAsync(backupSftpFolder, 60000));

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

                    /*switch (device.BackupCmdType)
                    {
                        case BackupCmdTypes.Default:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.Mikrotik:
                            //tasks.Add(sshShellWorker.ConnectAndExecuteForMikrotikAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.HP:
                            tasks.Add(sshWorker.ConnectAndDownloadAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.QSFP28:
                            tasks.Add(sshWorker.ConnectAndDownloadAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.JL256A:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.JL072A:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.HP_shell:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.J9298A:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.J9774A:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.J9146A:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.J9145A:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.J9779A:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.J9148A:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.J9147A:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.J9773A:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd));
                            break;
                        case BackupCmdTypes.J9584A:
                            tasks.Add(sshShellWorker.ConnectAndExecuteAsync(device, backupCmd, BackupCmdTypes.J9584A));
                            break;
                    }*/
                }

                await Task.WhenAll(tasks);
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
