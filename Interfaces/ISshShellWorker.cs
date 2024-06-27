using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface ISshShellWorker
    {
        void ConnectAndExecute(Device device, string cmd);
        Task ConnectAndExecuteAsync(Device device, string backupServerAddress, Enums.BackupCmdTypes backupCmdType = Enums.BackupCmdTypes.Default, 
            bool isConfigModeEnabled = false);
        Task ConnectAndExecuteForMikrotikAsync(Device device, string cmd);
    }
}