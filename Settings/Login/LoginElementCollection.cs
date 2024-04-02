using System.Configuration;

namespace backup_manager.Settings.Login
{
    [ConfigurationCollection(typeof(LoginElement), AddItemName = "login", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class LoginElementCollection : ConfigurationElementCollection
    {
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("Login id is null");

            return ((LoginElement)element).Id;
        }
        public void Add(LoginElement element)
        {
            LockItem = false;

            BaseAdd(element);

            LockItem = true;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new LoginElement();
        }
    }
}