using Renci.SshNet.Common;
using Renci.SshNet;
using Microsoft.Extensions.Logging;
using backup_manager.Model;
using backup_manager.Interfaces;

namespace backup_manager.Workers
{
    internal class SshShellWorker : ISshShellWorker
    {
        private readonly ILogger<SshShellWorker> logger;

        public SshShellWorker(ILogger<SshShellWorker> logger)
        {
            this.logger = logger;
        }
        // TODO. Async ssh shell calls with completion result.
        public async Task ConnectAndExecuteAsync(Device device, string cmd, Enums.BackupCmdTypes backupCmdType = Enums.BackupCmdTypes.Default, 
            bool isConfigModeEnabled = false)
        {
            var connectionInfo =
                new ConnectionInfo(
                    device.Ip,
                    22,
                    device.Login.AdmLogin,
                    new PasswordAuthenticationMethod(device.Login.AdmLogin, device.Login.AdminPass));

            try
            {
                using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
                {
                    ShellStream shell;

                    client.ConnectionInfo.Timeout = new TimeSpan(0, 1, 0);
                    client.Connect();

                    logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                        + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");
                    logger.LogInformation($"Run cmd -> {cmd}");

                    var terminalMode = new Dictionary<TerminalModes, uint>();
                    terminalMode.Add(TerminalModes.ECHO, 53);

                    shell = client.CreateShellStream("", 0, 0, 0, 0, 5192);

                    AsyncCallback onWorkDone = (ar) =>
                    {
                        logger.LogInformation("External work done!");
                    };

                    shell.WriteLine("\n");

                    var asyncExternalResult = shell.BeginExpect(onWorkDone, new ExpectAction("#", async (_) =>
                    {
                        if (isConfigModeEnabled)
                        {
                            shell.WriteLine("config");

                            logger.LogInformation($"Cnfg result: {shell.ReadLine()}");

                            var asyncExternalResultForConfig = shell.BeginExpect(onWorkDone, new ExpectAction("#", (_) =>
                            {
                                shell.WriteLine(cmd);
                            }));

                            var res = await asyncExternalResultForConfig.AsyncWaitHandle.WaitOneAsync(8000);
                            var result = shell.EndExpect(asyncExternalResultForConfig);
                        }
                        else
                        {
                            shell.WriteLine(cmd);
                        }
                    }));

                    bool res;

                    // TODO. J9584A получает сигнал true сразу же после отправки команды isConfigModeEnabled
                    if (backupCmdType != Enums.BackupCmdTypes.J9584A)
                    {
                        res = await asyncExternalResult.AsyncWaitHandle.WaitOneAsync(10000);
                    }
                    else
                    {
                        await Task.Delay(3000);
                        res = await asyncExternalResult.AsyncWaitHandle.WaitOneAsync(1);
                        //res = asyncExternalResult.CompletedSynchronously;
                    }

                    //var res = asyncExternalResult.AsyncWaitHandle.WaitOne();
                    var result = shell.EndExpect(asyncExternalResult);

                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Exception - " + ex.Message + $" for device: {device.Ip}");
                throw;
            }
        }
        public void ConnectAndExecuteForMikrotik(Device device, string backupCmd, string downloadCmd, string deleteCmd)
        {
            var connectionInfo =
                new ConnectionInfo(
                    device.Ip,
                    22,
                    device.Login.AdmLogin,
                    new PasswordAuthenticationMethod(device.Login.AdmLogin, device.Login.AdminPass));
            connectionInfo.Timeout = new TimeSpan(0, 10, 0);

            using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
            {
                ShellStream shell;
                client.Connect();

                logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                    + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");
                logger.LogInformation($"Run cmd -> {backupCmd}, {downloadCmd}");

                var terminalMode = new Dictionary<TerminalModes, uint>();
                terminalMode.Add(TerminalModes.ECHO, 53);

                shell = client.CreateShellStream("", 0, 0, 0, 0, 5192);

                try
                {
                    shell.WriteLine("\n");

                    shell.Expect(new ExpectAction(">", (_) =>
                    {
                        logger.LogInformation($"> received, executing backup sequence: {backupCmd} ...");
                        shell.WriteLine(backupCmd);
                    }));
                    shell.Expect(new ExpectAction("Configuration backup saved", (_) =>
                    {
                        logger.LogInformation($"Backup completed, executing download sequence: {downloadCmd} ...");
                        shell.WriteLine(downloadCmd);
                    }));

                    Thread.Sleep(10000);

                    shell.Expect(new ExpectAction("duration:", (_) =>
                    {
                        logger.LogInformation($"Download completed, executing delete sequence: {deleteCmd}  ...");
                        shell.WriteLine(deleteCmd);
                    }));

                    shell.Dispose();
                }
                catch (Exception ex)
                {
                    logger.LogError("Exception - " + ex.Message);
                    throw;
                }

                client.Disconnect();
            }
        }
        // TODO. Wait for completion.
        public async Task ConnectAndExecuteForMikrotikAsync(Device device, string backupCmd, string downloadCmd, string deleteCmd)
        {
            var connectionInfo =
                new ConnectionInfo(
                    device.Ip,
                    22,
                    device.Login.AdmLogin,
                    new PasswordAuthenticationMethod(device.Login.AdmLogin, device.Login.AdminPass));
            connectionInfo.Timeout = new TimeSpan(0, 10, 0);

            using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
            {
                ShellStream shell;

                client.Connect();

                logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                    + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");
                logger.LogInformation($"Run cmd -> {backupCmd}, {downloadCmd}");

                var terminalMode = new Dictionary<TerminalModes, uint>();
                terminalMode.Add(TerminalModes.ECHO, 53);

                shell = client.CreateShellStream("", 0, 0, 0, 0, 5192);

                try
                {
                    AsyncCallback onWorkDone = (ar) =>
                    {
                        logger.LogInformation($"External work done!");
                    };

                    shell.WriteLine("\n");

                    var asyncExternalResult = shell.BeginExpect(onWorkDone, new ExpectAction(">", (_) =>
                    {
                        logger.LogInformation($"> received, executing backup sequence: {backupCmd} ...");
                        shell.WriteLine(backupCmd);
                    }));
                    bool res;
                    res = await asyncExternalResult.AsyncWaitHandle.WaitOneAsync(50000);

                    var asyncExternalResultAfterBackup = shell.BeginExpect(onWorkDone, new ExpectAction("Configuration backup saved", (_) =>
                    {
                        logger.LogInformation($"Backup completed, executing download sequence: {downloadCmd} ...");
                        shell.WriteLine(downloadCmd);
                    }));
                    bool resAfterBackup;
                    resAfterBackup = await asyncExternalResultAfterBackup.AsyncWaitHandle.WaitOneAsync(100000);

                    await Task.Delay(200);

                    var asyncExternalResultAfterDownload = shell.BeginExpect(onWorkDone, new ExpectAction("duration:", (_) =>
                    {
                        logger.LogInformation($"Download completed, executing delete sequence: {deleteCmd}  ...");
                        shell.WriteLine(deleteCmd);
                    }));
                    bool resAfterDownload;
                    resAfterDownload = await asyncExternalResultAfterDownload.AsyncWaitHandle.WaitOneAsync(80000);

                    var asyncExternalResultAfterDelete = shell.BeginExpect(onWorkDone, new ExpectAction(">", (_) =>
                    {
                        logger.LogInformation($"Delete completed.");
                    }));
                    bool resAfterDelete;
                    resAfterDelete = await asyncExternalResultAfterDelete.AsyncWaitHandle.WaitOneAsync(20000);

                    var result = shell.EndExpect(asyncExternalResult);
                    var resultB = shell.EndExpect(asyncExternalResultAfterBackup);
                    var resultDown = shell.EndExpect(asyncExternalResultAfterDownload);
                    var resultDel = shell.EndExpect(asyncExternalResultAfterDelete);
                }
                catch (Exception ex)
                {
                    logger.LogError("Exception - " + ex.Message);
                }
                finally
                {
                    shell.WriteLine(deleteCmd);
                    await shell.DisposeAsync();
                }

                client.Disconnect();
            }
        }
        // TODO. Refactoring.
        public async Task<string> ConnectAndDownloadCiscoCfgAsync(Device device, string backupCmd)
        {
            List<string> cmdResults = new();

            var connectionInfo =
                new ConnectionInfo(
                    device.Ip,
                    22,
                    device.Login.AdmLogin,
                    new PasswordAuthenticationMethod(device.Login.AdmLogin, device.Login.AdminPass));
            connectionInfo.Timeout = new TimeSpan(0, 0, 30);
            connectionInfo.ChannelCloseTimeout = new TimeSpan(0, 0, 50);
            connectionInfo.MaxSessions = 200;

            try
            {
                using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
                {
                    ShellStream shell;

                    client.Connect();

                    logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                        + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");

                    var cmds = backupCmd.Split(',')
                        .Select(s => s.TrimStart()).ToList();

                    var terminalMode = new Dictionary<TerminalModes, uint>();
                    terminalMode.Add(TerminalModes.ECHO, 53);

                    shell = client.CreateShellStream("", 0, 0, 0, 0, 5192);

                    try
                    {
                        logger.LogInformation($"Run cmds -> {cmds.Count}");

                        string cmdUploadMode = cmds[0];
                        string cmdUploadDatatype = cmds[1];
                        string cmdUploadFilename = cmds[2];
                        string cmdUploadPath = cmds[3];
                        string cmdUploadServerip = cmds[4];
                        string cmdUploadStart = cmds[5];

                        shell.Write("\n");

                        shell.Write(cmdUploadMode);
                        await Task.Delay(1000);
                        shell.Write("\n");

                        shell.Write(cmdUploadDatatype);
                        await Task.Delay(1000);
                        shell.Write("\n");

                        shell.Write(cmdUploadFilename);
                        await Task.Delay(1000);
                        shell.Write("\n");

                        shell.Write(cmdUploadPath);
                        await Task.Delay(1000);
                        shell.Write("\n");

                        shell.Write(cmdUploadServerip);
                        await Task.Delay(1000);
                        shell.Write("\n");

                        shell.Write(cmdUploadStart);
                        await Task.Delay(1000);
                        shell.Write("\n");

                        shell.Write("y");
                        await Task.Delay(1000);
                        shell.Write("\n");

                        /*foreach (var cmd in cmds)
                        {
                            string expectedSymbol = !cmd.Contains("start") ? ">" : "Are you sure you want to start? (y/N)";

                            cmdResults.Add(await CmdRunner(shell, cmd, expectedSymbol));
                            await Task.Delay(1000);
                        }*/

                        logger.LogInformation($"Output: {shell.Read()}");

                        await Task.Delay(5000);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Exception - " + ex.Message);
                    }
                    finally
                    {
                        await shell.DisposeAsync();
                    }

                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{ex.Message}");
                throw;
            }
            finally
            {

            }

            return string.Concat(cmdResults);
        }
        private async Task<string> CmdRunner(ShellStream shell, string cmd, string expectedSymbol)
        {
            logger.LogInformation($"Run -> {cmd}");

            AsyncCallback onWorkDone = (ar) =>
            {
                logger.LogInformation($"External work done!");
            };

            shell.WriteLine(cmd);
            await Task.Delay(1000);
            shell.WriteLine("\n");
            await Task.Delay(200);

            var asyncExternalResult = shell.BeginExpect(onWorkDone, new ExpectAction(expectedSymbol, async (_) =>
            {
                logger.LogInformation($"{expectedSymbol} received.");

                if(expectedSymbol == "Are you sure you want to start? (y/N)")
                {
                    await Task.Delay(10000);
                    shell.WriteLine("y");
                    await Task.Delay(2000);
                    shell.WriteLine("\n");
                }
            }));

            bool res;
            res = await asyncExternalResult.AsyncWaitHandle.WaitOneAsync(30000);

            var result = shell.EndExpect(asyncExternalResult);

            return result;
        }
        public void ConnectAndExecute(Device device, string cmd)
        {
            var connectionInfo =
                new ConnectionInfo(
                    device.Ip,
                    22,
                    device.Login.AdmLogin,
                    new PasswordAuthenticationMethod(device.Login.AdmLogin, device.Login.AdminPass));

            using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
            {
                ShellStream shell;

                client.Connect();

                logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                    + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");

                var terminalMode = new Dictionary<TerminalModes, uint>();
                terminalMode.Add(TerminalModes.ECHO, 53);

                shell = client.CreateShellStream("", 0, 0, 0, 0, 4096, terminalMode);

                try
                {
                    shell.Expect(new ExpectAction("Press any key to continue", (_) => 
                    {
                        shell.WriteLine("\n");

                        shell.Expect(new ExpectAction("#", (_) =>
                        {
                            shell.WriteLine(cmd);
                        }));

                    }));

                    logger.LogInformation($"Read: {shell.Read()}");
                }
                catch (Exception ex)
                {
                    logger.LogError("Exception - " + ex.Message);
                    throw;
                }

                logger.LogInformation($"Run cmd -> {cmd}");

                client.Disconnect();
            }
        }
    }
}