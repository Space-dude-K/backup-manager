using backup_manager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backup_manager.Interfaces
{
    internal interface IBackupManager
    {
        void Init(List<Device> devices, List<string> backupLocations);
    }
}
