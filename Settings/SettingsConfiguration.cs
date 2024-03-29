using backup_manager.Settings.CheckObject;
using backup_manager.Settings.Email;
using System.Configuration;

namespace backup_manager.Settings
{
    class SettingsConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("objectsToCheck", IsDefaultCollection = false)]
        public CheckObjectElementCollection CheckObjects
        {
            get { return ((CheckObjectElementCollection)(base["objectsToCheck"])); }
        }
        [ConfigurationProperty("emails", IsDefaultCollection = false)]
        public LoginElementCollection Emails
        {
            get { return ((LoginElementCollection)(base["emails"])); }
        }
        [ConfigurationProperty("adminLogin", IsDefaultCollection = false)]
        public string AdminLogin
        {
            get { return (string)this["adminLogin"]; }
            set { this["adminLogin"] = value; }
        }
        [ConfigurationProperty("loginSalt", IsDefaultCollection = false)]
        public string LoginSalt
        {
            get { return (string)this["loginSalt"]; }
            set { this["loginSalt"] = value; }
        }
        [ConfigurationProperty("adminPass", IsDefaultCollection = false)]
        public string AdminPass
        {
            get { return (string)this["adminPass"]; }
            set { this["adminPass"] = value; }
        }
        [ConfigurationProperty("passSalt", IsDefaultCollection = false)]
        public string PassSalt
        {
            get { return (string)this["passSalt"]; }
            set { this["passSalt"] = value; }
        }
    }
}