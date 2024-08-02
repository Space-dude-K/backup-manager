namespace backup_manager.Model
{
    internal class DbResult
    {
        public string ServerAndInstanceName { get; set; }
        public string DbName { get; set; }
        public bool BackupStatus { get; set; }
        public bool VerifyStatus { get; set; }
        public bool RestoreStatus { get; set; }
        public bool DbCheckStatus { get; set; }
        public bool CleanupStatus { get; set; }
        public bool DeleteStatus { get; set; }
        public long CompletionTime { get; set; }
        public string CompletionTimeStr { get; set; }
    }
}