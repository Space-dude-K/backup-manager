﻿using Renci.SshNet.Common;
using Renci.SshNet;
using Microsoft.Extensions.Logging;
using backup_manager.Model;
using backup_manager.Interfaces;

namespace backup_manager.Workers
{
    internal class SshShellWorker : ISshShellWorker
    {
        SshClient sshClient;
        ShellStream shell;

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

            using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
            {
                client.Connect();

                logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                    + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");
                logger.LogInformation($"Run cmd -> {cmd}");

                var terminalMode = new Dictionary<TerminalModes, uint>();
                terminalMode.Add(TerminalModes.ECHO, 53);

                shell = client.CreateShellStream("", 0, 0, 0, 0, 5192);

                try
                {
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

                            var res = await asyncExternalResultForConfig.AsyncWaitHandle.WaitOneAsync(2000);
                            var result = shell.EndExpect(asyncExternalResultForConfig);
                        }
                        else
                        {
                            shell.WriteLine(cmd);
                        }
                    }));

                    bool res;

                    // TODO. J9584A получает сигнал true сразу же после отправки команды isConfigModeEnabled
                    if(backupCmdType != Enums.BackupCmdTypes.J9584A)
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
                }
                catch (Exception ex)
                {
                    logger.LogError("Exception - " + ex.Message);
                    throw;
                }

                client.Disconnect();
            }
        }
        public async Task ConnectAndExecuteForMikrotikAsync(Device device, string cmd)
        {
            var connectionInfo =
                new ConnectionInfo(
                    device.Ip,
                    22,
                    device.Login.AdmLogin,
                    new PasswordAuthenticationMethod(device.Login.AdmLogin, device.Login.AdminPass));

            using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
            {
                client.Connect();

                logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                    + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");
                logger.LogInformation($"Run cmd -> {cmd}");

                var terminalMode = new Dictionary<TerminalModes, uint>();
                terminalMode.Add(TerminalModes.ECHO, 53);

                shell = client.CreateShellStream("", 0, 0, 0, 0, 5192);

                try
                {
                    AsyncCallback onWorkDone = (ar) =>
                    {
                        logger.LogInformation("External work done!");
                    };

                    shell.WriteLine("\n");

                    var asyncExternalResult = shell.BeginExpect(onWorkDone, new ExpectAction(">", (_) =>
                    {
                        shell.WriteLine(cmd);
                    }));
                    bool res;
                    res = await asyncExternalResult.AsyncWaitHandle.WaitOneAsync(10000);

                    var asyncExternalResultAfterBackup = shell.BeginExpect(onWorkDone, new ExpectAction(">", (_) =>
                    {
                        shell.WriteLine(cmd);
                    }));
                    bool resAfterBackup;
                    resAfterBackup = await asyncExternalResultAfterBackup.AsyncWaitHandle.WaitOneAsync(10000);

                    var result = shell.EndExpect(asyncExternalResult);
                }
                catch (Exception ex)
                {
                    logger.LogError("Exception - " + ex.Message);
                    throw;
                }

                client.Disconnect();
            }
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