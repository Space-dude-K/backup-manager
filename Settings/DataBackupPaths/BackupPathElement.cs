using System.Configuration;

namespace backup_manager.Settings.BackupPaths
{
    class BackupPathElement : ConfigurationElement
    {
        [ConfigurationProperty("path", DefaultValue = "", IsRequired = true)]
        public string Path
        {
            get { return (string)this["path"]; }
            set { this["path"] = value; }
        }
        public BackupPathElement()
        {
        }
    }
}