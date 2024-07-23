using System.Configuration;

namespace backup_manager.Settings.DbObject
{
    class DbObjectElement : ConfigurationElement
    {
        #region Configuration Properties
        [ConfigurationProperty("dbName", IsRequired = true)]
        public string DbName
        {
            get { return (string)this["dbName"]; }
            set { this["dbName"] = value; }
        }
        [ConfigurationProperty("backupPath", IsRequired = false)]
        public string BackupPath
        {
            get { return (string)this["backupPath"]; }
            set { this["backupPath"] = value; }
        }
        [ConfigurationProperty("backupType", IsRequired = false)]
        public string BackupType
        {
            get { return (string)this["backupType"]; }
            set { this["backupType"] = value; }
        }
        [ConfigurationProperty("backupPeriod", IsRequired = false)]
        public string BackupPeriod
        {
            get { return (string)this["backupPeriod"]; }
            set { this["backupPeriod"] = value; }
        }
        [ConfigurationProperty("backupName", IsRequired = true)]
        public string BackupName
        {
            get { return (string)this["backupName"]; }
            set { this["backupName"] = value; }
        }
        [ConfigurationProperty("backupDescription", IsRequired = true)]
        public string BackupDescription
        {
            get { return (string)this["backupDescription"]; }
            set { this["backupDescription"] = value; }
        }
        private string CheckObject(string rawStr)
        {
            if (rawStr.Contains("="))
            {
                if (rawStr.Remove(0, 2).StartsWith(@"\"))
                {
                    return rawStr;
                }
                else
                {
                    return rawStr.Insert(2, @"\");
                }
            }
            else
            {
                return rawStr;
            }
        }
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
        #region Constructors
        public DbObjectElement()
        {
        }
        #endregion
    }
}