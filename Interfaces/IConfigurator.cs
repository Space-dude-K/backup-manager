using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backup_manager.Interfaces
{
    internal interface IConfigurator
    {
        void SaveAdminSettings((string admLogin, string loginSalt, string admPass, string passSalt) req, int loginId);
        void SaveSmtpReqSettings(string mailLogin, string mailLoginSalt, string mailPassword, string mailPasswordSalt);
    }
}
