using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface ISftpServer
    {
        bool RunSftpServer(string tempDir, string backupServerAddress, string backupCmd);
        Task<bool> RunSftpServerAsync(string tempDir, string backupServerAddress, List<Task> tasks);
    }
}