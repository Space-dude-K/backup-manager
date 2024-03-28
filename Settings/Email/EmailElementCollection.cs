using System.Configuration;

namespace backup_manager.Settings.Email
{
    [ConfigurationCollection(typeof(EmailElement), AddItemName = "email", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    class EmailElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new EmailElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("EmailData");

            return ((EmailElement)element).Email;
        }
        [ConfigurationProperty("sendEmail", IsDefaultCollection = false)]
        public string SendEmail
        {
            get { return (string)this["sendEmail"]; }
            set { this["sendEmail"] = value; }
        }
        [ConfigurationProperty("smtpServer", IsDefaultCollection = false)]
        public string SmtpServer
        {
            get { return (string)this["smtpServer"]; }
            set { this["smtpServer"] = value; }
        }
        [ConfigurationProperty("mailFrom", IsDefaultCollection = false)]
        public string MailFrom
        {
            get { return (string)this["mailFrom"]; }
            set { this["mailFrom"] = value; }
        }
        [ConfigurationProperty("mailLogin", IsDefaultCollection = false)]
        public string MailLogin
        {
            get { return (string)this["mailLogin"]; }
            set { this["mailLogin"] = value; }
        }
        [ConfigurationProperty("mailLoginSalt", IsDefaultCollection = false)]
        public string MailLoginSalt
        {
            get { return (string)this["mailLoginSalt"]; }
            set { this["mailLoginSalt"] = value; }
        }
        [ConfigurationProperty("mailPassword", IsDefaultCollection = false)]
        public string MailPassword
        {
            get { return (string)this["mailPassword"]; }
            set { this["mailPassword"] = value; }
        }
        [ConfigurationProperty("mailPasswordSalt", IsDefaultCollection = false)]
        public string MailPasswordSalt
        {
            get { return (string)this["mailPasswordSalt"]; }
            set { this["mailPasswordSalt"] = value; }
        }
    }
}