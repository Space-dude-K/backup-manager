using System.Diagnostics;
using System.Security.Cryptography;
using System.Security;
using backup_manager.Interfaces;

namespace backup_manager.Cypher
{
    public class Encryptor : ICypher
    {
        private readonly int saltLengthLimit = 32;
        private byte[] GetSalt()
        {
            return GetSalt(saltLengthLimit);
        }
        private byte[] GetSalt(int maximumSaltLength)
        {
            var salt = new byte[maximumSaltLength];

            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }

            return salt;
        }
        /// <summary>
        /// Этот метод шифрует пользовательские данные (логин и пароль).
        /// </summary>
        /// <param name="driver">Драйвер бд.</param>
        /// <param name="host">Адрес бд.</param>
        /// <param name="instance">Инстанс бд.</param>
        /// <param name="bd">Имя бд.</param>
        /// <param name="user">Логин.</param>
        /// <param name="pass">Пароль.</param>
        /// <returns>
        /// Готовый <see cref="RequisiteInformation"/>
        /// </returns>
        public RequisiteInformation Encrypt(SecureString user, SecureString pass)
        {
            return SetRequisites(user, pass);
        }
        /// <summary>
        /// Этот метод производит расшифровку пользовательских данных в простую строку (логин и пароль).
        /// </summary>
        /// <param name="driver">Драйвер бд.</param>
        /// <param name="host">Адрес бд.</param>
        /// <param name="instance">Инстанс бд.</param>
        /// <param name="bd">Имя бд.</param>
        /// <param name="userEncrypted">Зашифрованный логин.</param>
        /// <param name="uSalt">Соль логина.</param>
        /// <param name="passEncrypted">Зашифрованный пароль.</param>
        /// <param name="pSalt">Соль пароля.</param>
        /// <returns>
        /// Готовый <see cref="RequisiteInformation"/>
        /// </returns>
        public RequisiteInformation Decrypt(SecureString userEncrypted, string uSalt, SecureString passEncrypted, string pSalt)
        {
            return new RequisiteInformation(DecryptString(userEncrypted, uSalt), uSalt, DecryptString(passEncrypted, pSalt), pSalt);
        }
        public RequisiteInformation SetRequisites(SecureString user, SecureString pass)
        {
            var lSalt = GetSalt(256);
            var pSalt = GetSalt(256);

            return new RequisiteInformation(
                ToSecureString(EncryptString(user, lSalt)),
                Convert.ToBase64String(lSalt),
                ToSecureString(EncryptString(pass, pSalt)),
                Convert.ToBase64String(pSalt)
                );
        }
        private string EncryptString(System.Security.SecureString input, byte[] salt)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                salt,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedData);
        }
        public SecureString DecryptString(SecureString encryptedData, string salt)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(ToInsecureString(encryptedData)),
                    Convert.FromBase64String(salt),
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);

                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                return new SecureString();
            }
        }
        public SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();

            foreach (char c in input)
            {
                secure.AppendChar(c);
            }

            secure.MakeReadOnly();
            return secure;
        }
        public string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);

            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }

            return returnValue;
        }
    }
}