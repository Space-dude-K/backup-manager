using System.Configuration;

namespace backup_manager.Settings.DbObject
{
    [ConfigurationCollection(typeof(DbObjectElement), AddItemName = "db", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    class DbObjectElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DbObjectElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("DbNameDataArgumentNullException");

            return ((DbObjectElement)element).DbName;
        }
        [ConfigurationProperty("dbTempFolder", IsDefaultCollection = false)]
        public string DbTempFolder
        {
            get { return (string)this["dbTempFolder"]; }
            set { this["dbTempFolder"] = value; }
        }
        [ConfigurationProperty("testDbName", IsDefaultCollection = false)]
        public string TestDbName
        {
            get { return (string)this["testDbName"]; }
            set { this["testDbName"] = value; }
        }
        [ConfigurationProperty("testServerAddress", IsDefaultCollection = false)]
        public string TestServerAddress
        {
            get { return (string)this["testServerAddress"]; }
            set { this["testServerAddress"] = value; }
        }
        [ConfigurationProperty("loginId", IsDefaultCollection = false)]
        public string LoginId
        {
            get { return (string)this["loginId"]; }
            set { this["loginId"] = value; }
        }
        [ConfigurationProperty("dbRestoreDataFolder", IsDefaultCollection = false)]
        public string DbRestoreDataFolder
        {
            get { return (string)this["dbRestoreDataFolder"]; }
            set { this["dbRestoreDataFolder"] = value; }
        }
    }
}