namespace LLMLab.Server.Data;

public interface IEncryptionProvider
{
    string Encrypt(string dataToEncrypt);
    string Decrypt(string dataToDecrypt);
}