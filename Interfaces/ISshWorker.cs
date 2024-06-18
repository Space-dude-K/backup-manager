using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface ISshWorker
    {
        string ConnectAndDownload(Device device, string backupServerAddress, string backupCmd);
        Task<string> ConnectAndDownloadAsync(Device device, string backupServerAddress, string backupCmd, int timeOutInMs = 20000);
        string ConnectAndDownloadViaShellChannel(Device device, string backupServerAddress, string backupCmd);
    }
}