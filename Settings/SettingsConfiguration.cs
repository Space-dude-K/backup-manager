using backup_manager.Settings.BackupPaths;
using backup_manager.Settings.CheckObject;
using backup_manager.Settings.DbObject;
using backup_manager.Settings.Email;
using backup_manager.Settings.Login;
using System.Configuration;

namespace backup_manager.Settings
{
    class SettingsConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("objectsToCheck", IsDefaultCollection = false)]
        public CheckObjectElementCollection CheckObjects
        {
            get { return (CheckObjectElementCollection)base["objectsToCheck"]; }
        }
        [ConfigurationProperty("dbs", IsDefaultCollection = false)]
        public DbObjectElementCollection Dbs
        {
            get { return (DbObjectElementCollection)base["dbs"]; }
        }
        [ConfigurationProperty("backupPaths", IsDefaultCollection = false)]
        public BackupPathElementCollection BackupPaths
        {
            get { return (BackupPathElementCollection)base["backupPaths"]; }
        }
        [ConfigurationProperty("emails", IsDefaultCollection = false)]
        public EmailElementCollection Emails
        {
            get { return (EmailElementCollection)base["emails"]; }
        }
        [ConfigurationProperty("logins", IsDefaultCollection = false)]
        public LoginElementCollection Logins
        {
            get { return (LoginElementCollection)base["logins"]; }
        }
    }
}