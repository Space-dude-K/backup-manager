namespace backup_manager.Model
{
    internal class Db
    {
        public string DbName {  get; set; }
        public string BackupPath { get; set; }
        public Enums.BackupDbTypes BackupType { get; set; }
        public TimeSpan BackupPeriod { get; set; }
        public string Description { get; set; }
        public string BackupName { get; set; }
    }
}