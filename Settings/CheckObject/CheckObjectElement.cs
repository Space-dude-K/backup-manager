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
        [ConfigurationProperty("name", IsRequired = false)]
        public string DeviceName
        {
            get { return CheckObject((string)this["name"]); }
            set { this["name"] = value; }
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