using backup_manager.Cypher;
using System.Security;

namespace backup_manager.Interfaces
{
    internal interface ICypher
    {
        RequisiteInformation Decrypt(SecureString userEncrypted, string uSalt, SecureString passEncrypted, string pSalt);
        SecureString DecryptSecureString(SecureString encryptedData, string salt);
        SecureString DecryptString(string encryptedData, string salt);
        RequisiteInformation Encrypt(SecureString user, SecureString pass);
        RequisiteInformation Encrypt(string user, string pass);
        string ToInsecureString(SecureString input);
        SecureString ToSecureString(string input);
    }
}