using backup_manager.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backup_manager.Servers
{
    internal class FtpServer : IFtpServer
    {
        private readonly ILogger<TftpServer> logger;

        public FtpServer(ILogger<TftpServer> logger)
        {
            this.logger = logger;
        }
    }
}