using System.Configuration;

namespace backup_manager.Settings.CheckObject
{
    class CheckObjectElement : ConfigurationElement
    {
        #region Configuration Properties
        [ConfigurationProperty("ip", IsRequired = true)]
        public string ObjectIp
        {
            get { return (string)this["ip"]; }
            set { this["ip"] = value; }
        }
        [ConfigurationProperty("backupCmdType", IsRequired = false)]
        public string BackupCmdType
        {
            get { return CheckObject((string)this["backupCmdType"]); }
            set { this["backupCmdType"] = value; }
        }
        [ConfigurationProperty("name", IsRequired = true)]
        public string DeviceName
        {
            get { return CheckObject((string)this["name"]); }
            set { this["name"] = value; }
        }
        [ConfigurationProperty("loginId", IsRequired = false)]
        public string LoginId
        {
            get { return CheckObject((string)this["loginId"]); }
            set { this["loginId"] = value; }
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
        public CheckObjectElement()
        {
        }
        #endregion
    }
}