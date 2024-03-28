using System.Configuration;

namespace backup_manager.Settings.CheckObject
{
    [ConfigurationCollection(typeof(CheckObjectElement), AddItemName = "device", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    class CheckObjectElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CheckObjectElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("IpDataArgumentNullException");

            return ((CheckObjectElement)element).ObjectIp;
            //return element;
        }
        [ConfigurationProperty("loggerPath", IsDefaultCollection = false)]
        public string Loggerpath
        {
            get { return (string)this["loggerPath"]; }
            set { this["loggerPath"] = value; }
        }
    }
}