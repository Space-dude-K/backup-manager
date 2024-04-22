using backup_manager.Interfaces;
using backup_manager.Model;
using backup_manager.Settings.BackupPaths;
using backup_manager.Settings.CheckObject;
using backup_manager.Settings.Email;
using backup_manager.Settings.Login;
using Microsoft.Extensions.Logging;
using System.Configuration;
using static backup_manager.Model.Enums;

namespace backup_manager.Settings
{
    class Configurator : IConfigurator
    {
        private readonly ILogger<Configurator> loggerManager;
        private readonly ICypher cypher;

        enum SectionTypes
        {
            Device,
            Mail
        }
        
        public Configurator(ILogger<Configurator> loggerManager, ICypher cypher)
        {
            this.loggerManager = loggerManager;
            this.cypher = cypher;
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

            string exePath = Path.Combine(Environment.CurrentDirectory, applicationName);

            Console.WriteLine("Opening conf -> " + exePath);

            // Get the configuration file. The file name has
            // this format appname.exe.config.
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);

            return config;
        }
        public List<Model.Login> LoadLoginSettings(bool loadAsPlainText = true)
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            List<Model.Login> logins = [];

            foreach (LoginElement confReq in myConfig.Logins)
            {
                int loginId = 0;
                int.TryParse(confReq.Id, out loginId);

                if(loadAsPlainText)
                {
                    logins.Add(new Model.Login(loginId, 
                        cypher.ToInsecureString(cypher.DecryptString(confReq.LoginData, confReq.LoginSalt)),
                        cypher.ToInsecureString(cypher.DecryptString(confReq.PassData, confReq.PassSalt))));
                }
                else
                {
                    logins.Add(new Model.Login(loginId, confReq.LoginData, confReq.LoginSalt, confReq.PassData, confReq.PassSalt));
                }
            }

            return logins;
        }
        public void SaveLoginSettings(Model.Login login)
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            // TODO. Find latest ID
            var ri = cypher.Encrypt(login.AdmLogin, login.AdminPass);

            foreach (LoginElement confReq in myConfig.Logins)
            {
                if (int.Parse(confReq.Id) == login.LoginId)
                {
                    confReq.LoginData = cypher.ToInsecureString(ri.User);
                    confReq.LoginSalt = ri.USalt;
                    confReq.PassData = cypher.ToInsecureString(ri.Password);
                    confReq.PassSalt = ri.PSalt;
                }
            }

            myConfig.Logins.Add(new LoginElement(login.LoginId.ToString(), 
                cypher.ToInsecureString(ri.User), ri.USalt, cypher.ToInsecureString(ri.Password), ri.PSalt));

            myConfig.CurrentConfiguration.Save(ConfigurationSaveMode.Minimal);
        }
        public List<string> LoadPathSettings()
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            List<string> paths = [];

            foreach (BackupPathElement pathEl in myConfig.BackupPaths)
            {
                try
                {
                    if (!string.IsNullOrEmpty(Path.GetFullPath(pathEl.Path)))
                        paths.Add(pathEl.Path);
                }
                catch
                {
                    string err = $"Invalid path in path setting {pathEl.Path}";
                    loggerManager.LogError($"{err}");
                    throw new Exception(err);
                }
            }

            return paths;
        }
        public string LoadSftpTempFolderPath()
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            var sftpPath = string.IsNullOrWhiteSpace(myConfig.BackupPaths.SftpTempFolder) 
                ? Path.Combine(Environment.CurrentDirectory, "SftpTemp") : myConfig.BackupPaths.SftpTempFolder;

            return sftpPath;
        }
        public List<Device> LoadDeviceSettings(List<Model.Login> logins = null)
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

                device.SerialNumber = deviceSetting.SerialNumber;

                Enum.TryParse(deviceSetting.BackupCmdType, true, out BackupCmdTypes backupType);
                device.BackupCmdType = backupType;

                if(!string.IsNullOrEmpty(deviceSetting.LoginId) && logins.Count > 0)
                {
                    int loginId = 0;
                    int.TryParse(deviceSetting.LoginId, out loginId);

                    try
                    {
                        device.Login = logins.SingleOrDefault(l => l.LoginId == loginId);
                    }
                    catch(InvalidOperationException ex)
                    {
                        loggerManager.LogError("Duplicate login id in config section.", ex);
                        throw;
                    }
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
            List<Mail> emails = new();

            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            foreach (EmailElement mailSetting in myConfig.Emails)
            {
                Mail mail = new();
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