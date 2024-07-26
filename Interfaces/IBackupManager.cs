using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface IBackupManager
    {
        Task Init(List<Device> devices, Db testDb, 
            List<Db> dbs, List<string> backupLocations, string backupSftpFolder, 
            string dbTempPath, string dbRestoreDataFolder);
    }
}