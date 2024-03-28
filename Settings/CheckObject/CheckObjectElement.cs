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
        [ConfigurationProperty("device", IsRequired = false)]
        public string Device
        {
            get { return CheckObject((string)this["device"]); }
            set { this["device"] = value; }
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