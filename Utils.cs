using System.Net.Sockets;
using System.Net;
using System.Globalization;
using backup_manager.Model;

namespace backup_manager
{
    internal static class Utils
    {
        internal static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        internal static string GetFolderNamePartForBackupParent(Enums.BackupCmdTypes type)
        {
            switch (type)
            {
                case Enums.BackupCmdTypes.HP:
                    return "NetworkHardware";
                case Enums.BackupCmdTypes.Cisco:
                    return "NetworkHardware";
                case Enums.BackupCmdTypes.Default:
                default:
                    return "";
            }
        }
        internal static string RemoveInvalidChars(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }
        internal static string GetBackuFilePath(string ip, string deviceName)
        {
            string dtStr = DateTime.Now.ToString("ddMMyyyy-HHmmss.fff", CultureInfo.InvariantCulture);
            string dir = Path.Combine(Path.GetFileNameWithoutExtension(deviceName));
            string fileName = ip + "_" + dtStr + "_" + deviceName;

            return Path.Combine(dir, fileName);
        }
    }
}