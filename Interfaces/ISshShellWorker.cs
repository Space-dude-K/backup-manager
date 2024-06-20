using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface ISshShellWorker
    {
        void ConnectAndExecute(Device device, string cmd);
        Task ConnectAndExecuteAsync(Device device, string backupServerAddress);
    }
}