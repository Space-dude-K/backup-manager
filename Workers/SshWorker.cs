using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backup_manager.BackupWorkers
{
    internal class SshWorker
    {
        private readonly ILogger<SshWorker> logger;

        public SshWorker(ILogger<SshWorker> logger)
        {
            this.logger = logger;
        }

        static string ConnectAndDownload(string address, string login, string password, string backupCmd)
        {
            using (var client = new SshClient(address, login, password))
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
        static async Task<string> ConnectAndDownloadAsync(string address, string login, string password, string backupCmd)
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
