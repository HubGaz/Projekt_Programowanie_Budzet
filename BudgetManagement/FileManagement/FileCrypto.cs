using System.Security.Cryptography;
using System.Text;

namespace BudgetManagement.FileManagement;

internal static class FileCrypto
{
    private static readonly byte[] Magic = "BMENC1"u8.ToArray();
    private const int SaltSize = 16;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int Pbkdf2Iterations = 100_000;
    private const int KeySize = 32;

    public static byte[] CreateSalt() => RandomNumberGenerator.GetBytes(SaltSize);

    public static byte[] DeriveKey(string password, string username, byte[] salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var saltWithUser = new byte[salt.Length + Encoding.UTF8.GetByteCount(username)];
        Buffer.BlockCopy(salt, 0, saltWithUser, 0, salt.Length);
        Encoding.UTF8.GetBytes(username, saltWithUser.AsSpan(salt.Length));

        return Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            saltWithUser,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            KeySize);
    }

    public static bool IsEncryptedFile(ReadOnlySpan<byte> data) =>
        data.Length >= Magic.Length && data[..Magic.Length].SequenceEqual(Magic);

    public static bool IsPlaintextJson(ReadOnlySpan<byte> data)
    {
        foreach (var b in data)
        {
            if (b is (byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n')
            {
                continue;
            }

            return b is (byte)'{' or (byte)'[';
        }

        return false;
    }

    public static bool TryReadSalt(ReadOnlySpan<byte> data, out byte[] salt)
    {
        salt = Array.Empty<byte>();
        if (!IsEncryptedFile(data) || data.Length < Magic.Length + SaltSize)
        {
            return false;
        }

        salt = data.Slice(Magic.Length, SaltSize).ToArray();
        return true;
    }

    public static byte[] PackEncrypted(byte[] salt, byte[] plaintext, byte[] key)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var payloadLength = sizeof(int) + ciphertext.Length;
        var packed = new byte[Magic.Length + SaltSize + NonceSize + TagSize + payloadLength];
        var offset = 0;

        Magic.CopyTo(packed, offset);
        offset += Magic.Length;
        salt.CopyTo(packed, offset);
        offset += SaltSize;
        nonce.CopyTo(packed, offset);
        offset += NonceSize;
        tag.CopyTo(packed, offset);
        offset += TagSize;

        BitConverter.TryWriteBytes(packed.AsSpan(offset, sizeof(int)), ciphertext.Length);
        offset += sizeof(int);
        ciphertext.CopyTo(packed, offset);

        return packed;
    }

    public static byte[] DecryptPacked(ReadOnlySpan<byte> data, byte[] key)
    {
        if (!IsEncryptedFile(data))
        {
            throw new CryptographicException("File is not in encrypted format.");
        }

        var minLength = Magic.Length + SaltSize + NonceSize + TagSize + sizeof(int);
        if (data.Length < minLength)
        {
            throw new CryptographicException("Encrypted file is too short.");
        }

        var offset = Magic.Length + SaltSize;
        var nonce = data.Slice(offset, NonceSize);
        offset += NonceSize;
        var tag = data.Slice(offset, TagSize);
        offset += TagSize;

        var cipherLength = BitConverter.ToInt32(data.Slice(offset, sizeof(int)));
        offset += sizeof(int);

        if (cipherLength < 0 || data.Length < offset + cipherLength)
        {
            throw new CryptographicException("Encrypted file has invalid payload length.");
        }

        var ciphertext = data.Slice(offset, cipherLength);
        var plaintext = new byte[cipherLength];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return plaintext;
    }
}
