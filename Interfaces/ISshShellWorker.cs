using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface ISshShellWorker
    {
        Task<string> ConnectAndDownloadCiscoCfgAsync(Device device, string backupCmd);
        void ConnectAndExecute(Device device, string cmd);
        Task ConnectAndExecuteAsync(Device device, string backupServerAddress, Enums.BackupCmdTypes backupCmdType = Enums.BackupCmdTypes.Default, 
            bool isConfigModeEnabled = false);
        void ConnectAndExecuteForMikrotik(Device device, string backupCmd, string downloadCmd, string deleteCmd);
        Task ConnectAndExecuteForMikrotikAsync(Device device, string backupCmd, string downloadCmd, string deleteCmd);
    }
}