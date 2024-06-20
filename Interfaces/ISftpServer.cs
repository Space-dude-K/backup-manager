using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface ISftpServer
    {
<<<<<<< HEAD
        bool RunSftpServer(string tempDir, string backupServerAddress, string backupCmd);
        Task<bool> RunSftpServerAsync(string tempDir, string backupServerAddress, int serverDlTimeRangeInMs = 30000);
=======
        bool RunSftpServer(string sftpTempPath, Device device, string backupServerAddress, string backupCmd);
        Task<bool> RunSftpServerAsync(string sftpTempPath, Device device, string backupServerAddress, string backupCmd);
>>>>>>> main
    }
}