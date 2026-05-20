using System.Security.Cryptography;

namespace BudgetManagement.FileManagement;

public sealed class EncryptedFileSession
{
    private readonly byte[] _key;
    private readonly byte[] _salt;

    private EncryptedFileSession(byte[] key, byte[] salt)
    {
        _key = key;
        _salt = salt;
    }

    public static EncryptedFileSession Create(string username, string password, string filePath)
    {
        byte[] salt;
        if (File.Exists(filePath))
        {
            var existing = File.ReadAllBytes(filePath);
            if (FileCrypto.TryReadSalt(existing, out var fileSalt))
            {
                salt = fileSalt;
            }
            else
            {
                salt = FileCrypto.CreateSalt();
            }
        }
        else
        {
            salt = FileCrypto.CreateSalt();
        }

        var key = FileCrypto.DeriveKey(password, username, salt);
        return new EncryptedFileSession(key, salt);
    }

    public byte[] Encrypt(byte[] plaintext) => FileCrypto.PackEncrypted(_salt, plaintext, _key);

    public byte[] Decrypt(byte[] fileBytes) => FileCrypto.DecryptPacked(fileBytes, _key);
}
