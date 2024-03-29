using System.Configuration;

namespace backup_manager.Settings.Login
{
    [ConfigurationCollection(typeof(LoginElement), AddItemName = "login", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    class LoginElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new LoginElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("LoginData");

            return ((LoginElement)element).LoginData;
        }
    }
}