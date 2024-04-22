using System.Configuration;

namespace backup_manager.Settings.BackupPaths
{
    [ConfigurationCollection(typeof(BackupPathElement), AddItemName = "path", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    class BackupPathElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new BackupPathElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("Path element is null.");

            return ((BackupPathElement)element).Path;
        }
        [ConfigurationProperty("sftpTempFolder", IsDefaultCollection = false)]
        public string SftpTempFolder
        {
            get { return (string)this["sftpTempFolder"]; }
            set { this["sftpTempFolder"] = value; }
        }
    }
}