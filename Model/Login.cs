namespace backup_manager.Model
{
    public class Login
    {
        public Login(int loginId, string admLogin, string loginSalt, string adminPass, string passSalt)
        {
            LoginId = loginId;
            AdmLogin = admLogin;
            LoginSalt = loginSalt;
            AdminPass = adminPass;
            PassSalt = passSalt;
        }

        public int LoginId { set; get; }
        public string AdmLogin {  set; get; }
        public string LoginSalt {  set; get; }
        public string AdminPass {  set; get; }
        public string PassSalt { get; set; }

    }
}