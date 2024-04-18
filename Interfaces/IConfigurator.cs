using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface IConfigurator
    {
        void SaveAdminSettings(Login login);
        void SaveSmtpReqSettings(string mailLogin, string mailLoginSalt, string mailPassword, string mailPasswordSalt);
    }
}