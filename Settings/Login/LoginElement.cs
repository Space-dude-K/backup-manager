using System.Configuration;

namespace backup_manager.Settings.Login
{
    class LoginElement : ConfigurationElement
    {
        [ConfigurationProperty("id", DefaultValue = "", IsRequired = true)]
        public string Id
        {
            get { return (string)this["id"]; }
            set { this["id"] = value; }
        }
        [ConfigurationProperty("loginData", DefaultValue = "", IsRequired = true)]
        public string LoginData
        {
            get { return (string)this["loginData"]; }
            set { this["loginData"] = value; }
        }
        [ConfigurationProperty("loginSalt", DefaultValue = "", IsRequired = true)]
        public string LoginSalt
        {
            get { return (string)this["loginSalt"]; }
            set { this["loginSalt"] = value; }
        }
        [ConfigurationProperty("passData", DefaultValue = "", IsRequired = true)]
        public string PassData
        {
            get { return (string)this["passData"]; }
            set { this["passData"] = value; }
        }
        [ConfigurationProperty("passSalt", DefaultValue = "", IsRequired = true)]
        public string PassSalt
        {
            get { return (string)this["passSalt"]; }
            set { this["passSalt"] = value; }
        }
        public LoginElement()
        {
        }
    }
}