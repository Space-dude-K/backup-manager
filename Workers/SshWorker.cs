using backup_manager.Interfaces;
using backup_manager.Model;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using static backup_manager.Model.Enums;

namespace backup_manager.BackupWorkers
{
    internal class SshWorker : ISshWorker
    {
        private readonly ILogger<SshWorker> logger;

        public SshWorker(ILogger<SshWorker> logger)
        {
            this.logger = logger;
        }
        public string ConnectAndDownload(Device device, string backupServerAddress, string backupCmd)
        {
            using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
            {
                client.Connect();

                logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                    + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");

                logger.LogInformation($"Run cmd -> {backupCmd}");

                SshCommand cmd = client.CreateCommand(backupCmd);
                cmd.Execute();
                string execRes = cmd.Result;

                /*var cmd = client.RunCommand(backupCmd);
                cmd.CommandTimeout = new TimeSpan(0, 0, 0, 50);
                var execRes = cmd.Execute();*/

                if (!string.IsNullOrEmpty(cmd.Error))
                    logger.LogError($"Error: {cmd.Error}");

                logger.LogInformation($"Exec results: {execRes}");

                client.Disconnect();

                return execRes;
            }
        }
        public async Task<string> ConnectAndDownloadMikrotikCfgAsync(Device device, string backupCmd, string downloadCmd, string deleteCmd)
        {
            var connectionInfo =
                new ConnectionInfo(
                    device.Ip,
                    22,
                    device.Login.AdmLogin,
                    new PasswordAuthenticationMethod(device.Login.AdmLogin, device.Login.AdminPass));
            connectionInfo.Timeout = new TimeSpan(0, 10, 0);
            connectionInfo.ChannelCloseTimeout = new TimeSpan(0, 0, 20);
            connectionInfo.MaxSessions = 200;

            using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
            {
                client.Connect();

                logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                    + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");

                logger.LogInformation($"Run backup cmd -> {backupCmd}");

                SshCommand sshBackup = client.CreateCommand(backupCmd);
                var cmdExec = sshBackup.BeginExecute();
                while(!cmdExec.IsCompleted)
                {
                    await Task.Delay(2000);
                }
                var cmdExecResult = sshBackup.EndExecute(cmdExec);

                // TODO. Find a way to track completion.
                await Task.Delay(20000);

                logger.LogInformation($"Run download cmd -> {downloadCmd}");

                SshCommand sshDownload = client.CreateCommand(downloadCmd);
                var cmdExecDownload = sshDownload.BeginExecute();

                while (!cmdExecDownload.IsCompleted)
                {
                    await Task.Delay(1000);
                }
                var cmdExecResultDownload = sshDownload.EndExecute(cmdExecDownload);

                await Task.Delay(15000);

                logger.LogInformation($"Run delete cmd -> {downloadCmd}");

                SshCommand sshDelete = client.CreateCommand(deleteCmd);
                var cmdExecDelete = sshDelete.BeginExecute();

                while (!cmdExecDelete.IsCompleted)
                {
                    await Task.Delay(1000);
                }
                var cmdExecResultDelete = sshDelete.EndExecute(cmdExecDelete);

                client.Disconnect();

                return cmdExecResultDelete;
            }
        }
        public async Task<string> ConnectAndDownloadAsync(Device device, string backupCmd, 
            int timeOutInMs = 20000)
        {
            string? execEndRes = string.Empty;

            try
            {
                using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
                {
                    CancellationTokenSource cts = new(timeOutInMs);

                    await client.ConnectAsync(cts.Token);

                    logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                        + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");
                    logger.LogInformation($"Run cmd -> {backupCmd}");

                    var cmd = client.RunCommand(backupCmd);
                    var execRes = cmd.BeginExecute();
                    bool signaled = await execRes.AsyncWaitHandle.WaitOneAsync(10000);

                    if (signaled)
                    {
                        logger.LogInformation("Signal of completion received");
                    }
                    else
                    {
                        logger.LogInformation("Waiting signal timed out");
                    }

                    execEndRes = cmd.EndExecute(execRes);

                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception {ex.Message}");
            }

            return execEndRes;
        }
    }
}