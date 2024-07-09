using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backup_manager.Interfaces
{
    internal interface ISftpServer
    {
        Task<bool> RunSftpServerAsync(string tempDir, int serverDlTimeRangeInMs = 30000);
    }
}
