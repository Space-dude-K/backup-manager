using backup_manager.Interfaces;
using Microsoft.Extensions.Logging;
using Nuane.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tftp.Net;

namespace backup_manager.Servers
{
    internal class SftpServer : ISftpServer
    {
        private string serverDir;
        private readonly ILogger<SftpServer> logger;
        public SftpServer(ILogger<SftpServer> logger)
        {
            this.logger = logger;
        }
        public void RunSftpServer()
        {
            SshKey rsaKey = SshKey.Generate(SshKeyAlgorithm.RSA, 1024);
            SshKey dssKey = SshKey.Generate(SshKeyAlgorithm.DSS, 1024);


            // add keys, bindings and users
            Nuane.Net.SftpServer server = new();

            server.Log = Console.Out;

            server.Keys.Add(rsaKey);

            server.Keys.Add(dssKey);

            server.Bindings.Add(IPAddress.Any, 22);

            server.Users.Add(new SshUser("user01", "some-password", @"c:\data\user01"));

            server.Users.Add(new SshUser("user02", "another-password", @"c:\data\user02"));

            // start the server                                                    
            server.Start();

            Console.WriteLine("Press any key to stop the server.");

            Console.ReadKey();


            // stop the server                                                                        

            server.Stop();
        }
        public async Task<bool> RunSftpServerAsync(string tempDir, int serverDlTimeRangeInMs = 30000)
        {
            InitServerDir(tempDir);

            SshKey rsaKey = SshKey.Generate(SshKeyAlgorithm.RSA, 1024);
            SshKey dssKey = SshKey.Generate(SshKeyAlgorithm.DSS, 1024);

            // add keys, bindings and users
            Nuane.Net.SftpServer server = new();

            server.Log = Console.Out;

            server.Keys.Add(rsaKey);
            server.Keys.Add(dssKey);

            server.Bindings.Add(IPAddress.Any, 22);

            server.Users.Add(new SshUser("admin", "admin", tempDir));

            // start the server                                                    
            server.Start();

            while (true)
            {
                logger.LogInformation("Waiting for connection ...");

                await Task.Delay(serverDlTimeRangeInMs);
                break;
            }

            logger.LogInformation("Sftp server completed dl requests.");
            server.Stop();

            return true;
        }
        private void InitServerDir(string dir)
        {
            serverDir = dir;

            if (!Directory.Exists(serverDir))
                Directory.CreateDirectory(serverDir);

            logger.LogInformation("Init SFTP server. Temp directory: " + serverDir);
        }
    }
}
