using backup_manager.Cypher;
using System.Security;

namespace backup_manager.Interfaces
{
    internal interface ICypher
    {
        RequisiteInformation Decrypt(SecureString userEncrypted, string uSalt, SecureString passEncrypted, string pSalt);
        SecureString DecryptString(SecureString encryptedData, string salt);
        RequisiteInformation Encrypt(SecureString user, SecureString pass);
        string ToInsecureString(SecureString input);
        SecureString ToSecureString(string input);
    }
}