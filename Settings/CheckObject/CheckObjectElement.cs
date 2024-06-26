﻿using System.Configuration;

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
            get { return (string)this["backupCmdType"]; }
            set { this["backupCmdType"] = value; }
        }
        [ConfigurationProperty("name", IsRequired = true)]
        public string DeviceName
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
        [ConfigurationProperty("sn", IsRequired = true)]
        public string SerialNumber
        {
            get { return (string)this["sn"]; }
            set { this["sn"] = value; }
        }
        [ConfigurationProperty("cfgName", IsRequired = false)]
        public string ConfigName
        {
            get { return (string)this["cfgName"]; }
            set { this["cfgName"] = value; }
        }
        [ConfigurationProperty("loginId", IsRequired = false)]
        public string LoginId
        {
            get { return (string)this["loginId"]; }
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