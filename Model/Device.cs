namespace backup_manager.Model
{
    internal class Device
    {
        public string Ip { get; set; }
        public string Name { get; set; }
        public int BackupCmdType { get; set; }
        public Login Login {  get; set; }
    }
}