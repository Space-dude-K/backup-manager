
namespace backup_manager.Interfaces
{
    internal interface IFtpServer
    {
        void RunFtpServer(string tempDir);
        Task<bool> RunFtpServerAsync(string tempDir, int serverDlTimeRangeInMs = 30000);
    }
}