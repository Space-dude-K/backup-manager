using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface ISftpServer
    {
        bool RunSftpServer(string sftpTempPath, Device device, string backupServerAddress, string backupCmd);
        Task<bool> RunSftpServerAsync(string sftpTempPath, Device device, string backupServerAddress, string backupCmd);
    }
}