namespace backup_manager.Interfaces
{
    internal interface ITftpServer
    {
        Task<bool> RunSftpServerAsync(string tempDir, string backupServerAddress, int serverDlTimeRangeInMs = 30000);
    }
}