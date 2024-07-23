using backup_manager.Settings.CheckObject;
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
    }
}