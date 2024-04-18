using backup_manager.Interfaces;
using backup_manager.Model;
using backup_manager.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Renci.SshNet;
using System;
using System.Configuration;
using System.Net;
using System.Xml;
using Tftp.Net;
using static System.Net.Mime.MediaTypeNames;

namespace backup_manager
{
    internal class Program
    {
        static string sshClientAddress = "10.10.102.1";
        static string backupCmd = "backup startup-configuration to 10.10.200.37 test3.cfg";
        static string backupCmd1 = "copy running-configuration 10.10.200.37 run.cfg";
        static string ServerDirectory = Environment.CurrentDirectory;
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                var config = new ConfigurationBuilder()
                   //.SetBasePath(System.IO.Directory.GetCurrentDirectory())
                   //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .Build();

                var servicesProvider = BuildDi(config);

                using (servicesProvider as IDisposable)
                {
                    var backupManager = servicesProvider.GetRequiredService<BackupManager>();
                    backupManager.Init();

                    var conf = servicesProvider.GetRequiredService<IConfigurator>();
                    conf.SaveAdminSettings(new Login (3, "admLogin", "logSalt", "admPass", "passSalt"));

                    Console.WriteLine("Press ANY key to exit");
                    //Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }
        private static IServiceProvider BuildDi(IConfiguration config)
        {
            return new ServiceCollection()
                  // Add DI Classes here
                  .AddSingleton<IConfigurator, Configurator>()
                  .AddTransient<ISftpServer, SftpServer>()
                  .AddTransient<BackupManager>()
                  .AddLogging(loggingBuilder =>
                  {
                      // configure Logging with NLog
                      loggingBuilder.ClearProviders();
                      loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                      loggingBuilder.AddNLog(config);
                  })
                  .BuildServiceProvider();
        }

        static string ConnectAndDownload()
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
        static async Task<string> ConnectAndDownloadAsync()
        {
            string expectedFingerPrint = "LKOy5LvmtEe17S4lyxVXqvs7uPMy+yF79MQpHeCs/Qo";

            using (var client = new SshClient(sshClientAddress, "admin", "VMGPa$$w0rd"))
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
        static void RunSftpServer()
        {
            Console.WriteLine("Running TFTP server for directory: " + ServerDirectory);
            Console.WriteLine();
            Console.WriteLine("Press any key to close the server.");

            using (var server = new TftpServer())
            {
                server.OnReadRequest += new TftpServerEventHandler(server_OnReadRequest);
                server.OnWriteRequest += new TftpServerEventHandler(server_OnWriteRequest);
                server.Start();
                Console.Read();
            }
        }

        static void server_OnWriteRequest(ITftpTransfer transfer, EndPoint client)
        {
            String file = Path.Combine(ServerDirectory, transfer.Filename);

            if (File.Exists(file))
            {
                CancelTransfer(transfer, TftpErrorPacket.FileAlreadyExists);
            }
            else
            {
                OutputTransferStatus(transfer, "Accepting write request from " + client);
                StartTransfer(transfer, new FileStream(file, FileMode.CreateNew));
            }
        }

        static void server_OnReadRequest(ITftpTransfer transfer, EndPoint client)
        {
            String path = Path.Combine(ServerDirectory, transfer.Filename);
            FileInfo file = new FileInfo(path);

            //Is the file within the server directory?
            if (!file.FullName.StartsWith(ServerDirectory, StringComparison.InvariantCultureIgnoreCase))
            {
                CancelTransfer(transfer, TftpErrorPacket.AccessViolation);
            }
            else if (!file.Exists)
            {
                CancelTransfer(transfer, TftpErrorPacket.FileNotFound);
            }
            else
            {
                OutputTransferStatus(transfer, "Accepting request from " + client);
                StartTransfer(transfer, new FileStream(file.FullName, FileMode.Open, FileAccess.Read));
            }
        }

        private static void StartTransfer(ITftpTransfer transfer, Stream stream)
        {
            transfer.OnProgress += new TftpProgressHandler(transfer_OnProgress);
            transfer.OnError += new TftpErrorHandler(transfer_OnError);
            transfer.OnFinished += new TftpEventHandler(transfer_OnFinished);
            transfer.Start(stream);
        }

        private static void CancelTransfer(ITftpTransfer transfer, TftpErrorPacket reason)
        {
            OutputTransferStatus(transfer, "Cancelling transfer: " + reason.ErrorMessage);
            transfer.Cancel(reason);
        }

        static void transfer_OnError(ITftpTransfer transfer, TftpTransferError error)
        {
            OutputTransferStatus(transfer, "Error: " + error);
        }

        static void transfer_OnFinished(ITftpTransfer transfer)
        {
            OutputTransferStatus(transfer, "Finished");
        }

        static void transfer_OnProgress(ITftpTransfer transfer, TftpTransferProgress progress)
        {
            OutputTransferStatus(transfer, "Progress " + progress);
        }

        private static void OutputTransferStatus(ITftpTransfer transfer, string message)
        {
            Console.WriteLine("[" + transfer.Filename + "] " + message);
        }
        
    }
}
