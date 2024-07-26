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
using System.Data.SqlClient;
using System.Diagnostics;

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
        private readonly IZipWorker zipWorker;
        private readonly ISqlWorker sqlWorker;

        public BackupManager(IServiceProvider serviceCollection, ILogger<BackupManager> loggerManager, ITftpServer tftpServer, ISftpServer sftpServer,
            ISshWorker sshWorker, ISshShellWorker sshShellWorker, IZipWorker zipWorker, ISqlWorker sqlWorker)
        {
            this.serviceCollection = serviceCollection;
            this.loggerManager = loggerManager;
            this.tftpServer = tftpServer;
            this.sftpServer = sftpServer;
            this.sshWorker = sshWorker;
            this.sshShellWorker = sshShellWorker;
            this.zipWorker = zipWorker;
            this.sqlWorker = sqlWorker;
        }
        public async Task Init(List<Device> devices, Db? testDb, List<Db> dbs, 
            List<string> backupLocations, string backupSftpFolder, string dbTempPath, string dbRestoreDataFolder)
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

                serverTasks = new List<Task>();
                serverTasks.Add(tftpServer.RunTftpServerAsync(backupSftpFolder, Utils.GetLocalIPAddress(), 120000));
                serverTasks.Add(sftpServer.RunSftpServerAsync(backupSftpFolder, 120000));

                /*foreach (var device in devices)
                {
                    var dtStr = DateTime.Now.ToString("ddMMyyyy.fff", CultureInfo.InvariantCulture);
                    var deviceNameAndSn = Utils.RemoveInvalidChars(device.Name + "_" + device.SerialNumber);
                    var fileName = (deviceNameAndSn + "_" + dtStr + ".cfg")
                        .GetCleanFileName();
                    var backupServerAddress = Utils.GetLocalIPAddress();
                    var backupCmd =
                        device.BackupCmdType.GetDisplayAttributeFrom(typeof(BackupCmdTypes))
                        .Replace("%configName%", device.ConfigName)
                        .Replace("%addr%", backupServerAddress)
                        .Replace("%file%", fileName);

                    switch (device.BackupCmdType)
                    {
                        case BackupCmdTypes.Default:
                            tasks.Add(Task.Run(() => sshShellWorker.ConnectAndExecuteAsync(device, backupCmd)));
                            break;
                        case BackupCmdTypes.HP:
                        case BackupCmdTypes.QSFP28:
                            tasks.Add(Task.Run(() => sshWorker.ConnectAndDownloadAsync(device, backupCmd)));
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
                        case BackupCmdTypes.NanoStation:
                            tasks.Add(Task.Run(() => sshShellWorker
                            .ConnectAndExecuteAsync(device, backupCmd, BackupCmdTypes.J9584A)));
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
                            sshWorker.ConnectAndDownloadMikrotikCfgAsync(device, backupCmd, downloadCmd, deleteCmd)));
                            break;
                        case BackupCmdTypes.Cisco_vWLC:
                            tasks.Add(Task.Run(() => sshShellWorker
                            .ConnectAndDownloadCiscoCfgAsync(device, backupCmd)));
                            break;
                    }
                }*/

                if (dbs.Count > 0)
                {
                    var connStrForTestServer = new SqlConnectionStringBuilder()
                    {
                        DataSource = testDb.Server,
                        InitialCatalog = testDb.DbName,
                        IntegratedSecurity = false,
                        UserID = testDb.Login.AdmLogin,
                        Password = testDb.Login.AdminPass
                    }.ConnectionString;

                    foreach (Db db in dbs)
                    {
                        var dtNamePart = db.BackupName;
                        var dtStr = Utils.GetDateStrForBackupFileName();
                        var fileName = (dtNamePart + "_" + dtStr + ".bak").GetCleanFileName();
                        var fileFullPath = Path.Combine(dbTempPath, fileName);

                        var connStr = new SqlConnectionStringBuilder()
                        {
                            DataSource = db.Server,
                            InitialCatalog = db.DbName,
                            IntegratedSecurity = false,
                            UserID = db.Login.AdmLogin,
                            Password = db.Login.AdminPass
                        }.ConnectionString;

                        // 1. Backup DB
                        bool backupResult = false;
                        using (SqlConnection conn = new(connStr))
                        {
                            Stopwatch timer = new();
                            timer.Start();

                            backupResult = await sqlWorker
                                .BackupDatabaseAsync(conn, db.DbName, fileFullPath, db.Description, fileName);

                            timer.Stop();

                            loggerManager.LogInformation($"Backup task {db.Server} {db.DbName} completed: {timer.Elapsed}");
                        }

                        //loggerManager.LogInformation($"{db.DbName} {db.BackupType.ToString()}, {db.BackupPeriod} -> {fileFullPath}");

                        // 2. Verify DB
                        bool verifyResult = false;
                        using (SqlConnection conn = new(connStrForTestServer))
                        {
                            if (backupResult)
                            {
                                Stopwatch timer = new();
                                timer.Start();

                                verifyResult = await sqlWorker
                                    .VerifyDatabaseAsync(conn, fileFullPath, db.DbName);

                                timer.Stop();

                                loggerManager.LogInformation($"Veriyfy task {db.Server} {db.DbName} completed: {timer.Elapsed}");
                            }
                            else
                            {
                                loggerManager.LogInformation($"Verify task {db.Server} {db.DbName} skipped!");
                            }
                        }

                        // 3. Restore DB
                        bool restoreResult = false;
                        using (SqlConnection conn = new(connStrForTestServer))
                        {
                            if (verifyResult)
                            {
                                Stopwatch timer = new();
                                timer.Start();

                                restoreResult = await sqlWorker
                                    .RestoreDatabaseWithMoveAsync(conn, fileFullPath, db.DbName, dbRestoreDataFolder);

                                timer.Stop();

                                loggerManager.LogInformation($"Restore task {db.Server} {db.DbName} completed: {timer.Elapsed}");
                            }
                            else
                            {
                                loggerManager.LogInformation($"Restore task {db.Server} {db.DbName} skipped!");
                            }
                        }

                        // 4. DBCHECK
                        bool dbCheckResult = false;
                        using (SqlConnection conn = new(connStrForTestServer))
                        {
                            if (restoreResult)
                            {
                                Stopwatch timer = new();
                                timer.Start();

                                dbCheckResult = await sqlWorker
                                    .RestoreDatabaseWithMoveAsync(conn, fileFullPath, db.DbName);

                                timer.Stop();

                                loggerManager.LogInformation($"Restore task {db.Server} {db.DbName} completed: {timer.Elapsed}");
                            }
                            else
                            {
                                loggerManager.LogInformation($"Restore task {db.Server} {db.DbName} skipped!");
                            }
                        }

                    }
                }

                await Task.WhenAll(tasks);
                await Task.Delay(10000);

                loggerManager.LogInformation($"Backup tasks comleted.");

                // Zip
                foreach(var file in Directory.GetFiles(backupSftpFolder))
                {
                    zipWorker.SafelyCreateZipFromDirectory(file);
                }

                loggerManager.LogInformation($"Zip tasks comleted.");
            }

            await Task.WhenAll(serverTasks);
        }
    }
}