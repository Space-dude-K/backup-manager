using backup_manager.Cypher;
using backup_manager.Interfaces;
using backup_manager.Settings.CheckObject;
using backup_manager.Settings.Email;
using backup_manager.Settings.Login;
using System.Configuration;

namespace backup_manager.Settings
{
    class Configurator : IConfigurator
    {
        enum SectionTypes
        {
            Device,
            Mail
        }
        public Configurator()
        {
        }
        #region User Helpers Devices, Emails
        private Configuration LoadConfig()
        {
#if DEBUG
            string applicationName =
                Environment.GetCommandLineArgs()[0];
#else
                string applicationName =
                    Environment.GetCommandLineArgs()[0];
                    //Environment.GetCommandLineArgs()[0]+ ".exe";
#endif

            string exePath = System.IO.Path.Combine(Environment.CurrentDirectory, applicationName);

            Console.WriteLine("Opening conf -> " + exePath);

            // Get the configuration file. The file name has
            // this format appname.exe.config.
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);

            return config;
        }
        public List<(string, string, string, string)> LoadLoginSettings()
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            List<(string, string, string, string)> reqs = [];

            foreach (LoginElement confReq in myConfig.Logins)
            {
                reqs.Add((confReq.LoginData, confReq.LoginSalt, confReq.PassData, confReq.PassSalt));
            }

            return reqs;
        }
        public void SaveAdminSettings((string admLogin, string loginSalt, string admPass, string passSalt) req, int loginId)
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            bool isAlrdyInConfig = false;

            foreach (LoginElement confReq in myConfig.Logins)
            {
                if (int.Parse(confReq.Id) == loginId)
                {
                    confReq.LoginData = req.admLogin;
                    confReq.LoginSalt = req.loginSalt;
                    confReq.PassData = req.admPass;
                    confReq.PassSalt = req.passSalt;

                    isAlrdyInConfig = true;
                }
            }

            myConfig.Logins.Add(new LoginElement(loginId.ToString(), 
                req.admLogin, req.loginSalt, req.admPass, req.passSalt));

            myConfig.CurrentConfiguration.Save();
        }
        public List<Device> LoadDeviceSettings()
        {
            List<Device> devices = [];

            SettingsConfiguration myConfig = (SettingsConfiguration)ConfigurationManager.GetSection("settings");

            foreach (CheckObjectElement deviceSetting in myConfig.CheckObjects)
            {
                Device device = new();
                device.Ip = deviceSetting.ObjectIp;

                if (deviceSetting.DeviceName != string.Empty)
                {
                    device.Name = deviceSetting.DeviceName;
                }
                else
                {
                    throw new Exception("Null DeviceName setting exception.");
                }

                devices.Add(device);
            }

            return devices;
        }
        public void SaveSmtpReqSettings(string mailLogin, string mailLoginSalt, string mailPassword, string mailPasswordSalt)
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            myConfig.CurrentConfiguration.Save();
        }
        public (bool sendEmail, string smtpServer, string mailFrom) LoadSmtpSettings()
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            bool sendEmail = false;
            bool.TryParse(myConfig.Emails.SendEmail, out sendEmail);

            return (sendEmail, myConfig.Emails.SmtpServer, myConfig.Emails.MailFrom);
        }
        public List<Mail> LoadMailSettings()
        {
            List<Mail> emails = new List<Mail>();

            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            foreach (EmailElement mailSetting in myConfig.Emails)
            {
                Mail mail = new Mail();
                mail.Email = mailSetting.Email;
                mail.Subject = mailSetting.Subject;

                emails.Add(mail);
            }

            return emails;
        }
        public string GetLoggerPath()
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            string cfgPath;

            if (!string.IsNullOrEmpty(myConfig.CheckObjects.Loggerpath) && Directory.Exists(myConfig.CheckObjects.Loggerpath))
                cfgPath = myConfig.CheckObjects.Loggerpath;
            else
            {
                cfgPath = Environment.CurrentDirectory;
            }

            return cfgPath;
        }
        #endregion
    }
}