using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface ISshWorker
    {
        string ConnectAndDownload(Device device, string backupServerAddress, string backupCmd);
        Task<string> ConnectAndDownloadAsync(Device device, string backupCmd, int timeOutInMs = 20000);
        Task<string> ConnectAndDownloadMikrotikCfgAsync(Device device, string backupCmd, string downloadCmd, string deleteCmd);
    }
}