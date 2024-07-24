using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface IBackupManager
    {
        Task Init(List<Device> devices, List<Db> dbs, List<string> backupLocations, string backupSftpFolder, string dbTempPath);
    }
}