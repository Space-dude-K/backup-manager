using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface ISftpServer
    {
        Task<bool> RunSftpServerAsync(string tempDir, string backupServerAddress, int serverDlTimeRangeInMs = 30000);
    }
}