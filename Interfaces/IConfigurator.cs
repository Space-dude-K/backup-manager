﻿using backup_manager.Model;

namespace backup_manager.Interfaces
{
    internal interface IConfigurator
    {
        List<Db> LoadDbSettings(List<Login> logins = null);
        List<Device> LoadDeviceSettings(List<Login> logins = null);
        List<Login> LoadLoginSettings(bool loadAsPlainText = true);
        List<string> LoadPathSettings();
        string LoadSftpTempFolderPath();
        void SaveLoginSettings(Login login);
        void SaveSmtpReqSettings(string mailLogin, string mailLoginSalt, string mailPassword, string mailPasswordSalt);
    }
}