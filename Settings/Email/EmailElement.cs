using System.Configuration;

namespace backup_manager.Settings.Email
{
    class EmailElement : ConfigurationElement
    {
        [ConfigurationProperty("mail", DefaultValue = "", IsRequired = true)]
        public string Email
        {
            get { return (string)this["mail"]; }
            set { this["mail"] = value; }
        }
        [ConfigurationProperty("subject", DefaultValue = "", IsRequired = false)]
        public string Subject
        {
            get { return (string)this["subject"]; }
            set { this["subject"] = value; }
        }
        public EmailElement()
        {
        }
    }
}