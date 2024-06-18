using backup_manager.Interfaces;
using backup_manager.Model;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System.IO;
using System.Runtime.Intrinsics.X86;
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
        public string ConnectAndDownloadViaShellChannel(Device device, string backupServerAddress, string backupCmd)
        {
            using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
            {
                client.Connect();

                logger.LogInformation($"Run cmd -> {backupCmd}");

                var cmd = client.CreateCommand(backupCmd);
                var asyncExecute = cmd.BeginExecute();

                cmd.OutputStream.CopyTo(Console.OpenStandardOutput());
                cmd.EndExecute(asyncExecute);

                if (!string.IsNullOrEmpty(cmd.Error))
                    logger.LogError($"Error: {cmd.Error}");

                string execRes = cmd.Result;

                logger.LogInformation($"Exec results: {execRes}");

                client.Disconnect();

                return execRes;
            }
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
        public async Task<string> ConnectAndDownloadAsync(Device device, string backupServerAddress, string backupCmd, 
            int timeOutInMs = 20000)
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

                var execEndRes = cmd.EndExecute(execRes);

                logger.LogInformation($"Exec results: {execEndRes}");

                client.Disconnect();

                return execEndRes;
            }
        }
        async Task<string> ConnectAndDownloadAsync1(string address, string login, string password, string backupCmd)
        {
            string expectedFingerPrint = "LKOy5LvmtEe17S4lyxVXqvs7uPMy+yF79MQpHeCs/Qo";

            using (var client = new SshClient(address, "admin", "VMGPa$$w0rd"))
            {
                /*client.HostKeyReceived += (sender, e) =>
                {
                    e.CanTrust = expectedFingerPrint.Equals(e.FingerPrintSHA256);
                };*/

                //client.Connect();

                /*Task<string> task = Task.Factory.FromAsync<string, string>((p, c, st) => cmd.BeginExecute(p, c, st),
                cmd.EndExecute, cmdText.ToString(), null);*/

                CancellationTokenSource cts = new(20000);

                await client.ConnectAsync(cts.Token);

                Console.WriteLine($"Conn info: {client.ConnectionInfo.Host + " "
                    + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");
                Console.WriteLine($"Run cmd -> {backupCmd}");

                var cmd = client.RunCommand(backupCmd);
                var execRes = cmd.BeginExecute();
                bool signaled = await execRes.AsyncWaitHandle.WaitOneAsync(10000);

                if (signaled)
                {
                    Console.WriteLine("Signal of completion received");
                }
                else
                {
                    Console.WriteLine("Waiting signal timed out");
                }

                var execEndRes = cmd.EndExecute(execRes);

                Console.WriteLine($"Exec results: {execEndRes}");

                client.Disconnect();

                return execEndRes;
            }
        }
    }
}
