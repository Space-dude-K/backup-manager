using System.Security;

namespace backup_manager.Cypher
{
    public class RequisiteInformation
    {
        private SecureString user;

        public SecureString User
        {
            get { return user; }
            set { user = value; }
        }
        private SecureString password;

        public SecureString Password
        {
            get { return password; }
            set { password = value; }
        }
        private string uSalt;

        public string USalt
        {
            get { return uSalt; }
            set { uSalt = value; }
        }
        private string pSalt;

        public string PSalt
        {
            get { return pSalt; }
            set { pSalt = value; }
        }
        public RequisiteInformation(
            SecureString user, string uSalt, SecureString password, string pSalt)
        {
            User = user;
            USalt = uSalt;
            Password = password;
            PSalt = pSalt;
        }
    }
}