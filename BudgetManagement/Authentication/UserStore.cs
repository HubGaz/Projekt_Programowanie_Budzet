using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BudgetManagement.FileManagement;

namespace BudgetManagement.Authentication;

public static class UserStore
{
    private const string LegacyUsersFilePath = "users.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static bool AccountExists(string username) =>
        File.Exists(UserFilePaths.GetAccountFilePath(username));

    public static bool IsUsernameTaken(string username)
    {
        if (AccountExists(username))
        {
            return true;
        }

        return LoadLegacyUsers()
            .Any(u => string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static bool TryCreateAccount(UserAccount account, string password, out string message)
    {
        var path = UserFilePaths.GetAccountFilePath(account.Username);
        if (File.Exists(path))
        {
            message = "A user with this name already exists.";
            return false;
        }

        try
        {
            SaveEncryptedAccount(path, account.Username, password, account);
            message = "Account has been created.";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Could not create account file: {ex.Message}";
            return false;
        }
    }

    public static bool TryLogin(string username, string password, out UserAccount? account, out string message)
    {
        account = null;
        var trimmedUsername = username.Trim();
        var accountPath = UserFilePaths.GetAccountFilePath(trimmedUsername);

        if (!File.Exists(accountPath))
        {
            if (!TryLoginLegacy(trimmedUsername, password, out account, out message))
            {
                message = "User was not found.";
                return false;
            }

            try
            {
                MigrateLegacyAccount(account!, password);
            }
            catch (Exception ex)
            {
                message = $"Login succeeded but migration failed: {ex.Message}";
                return false;
            }

            message = "Logged in successfully.";
            return true;
        }

        try
        {
            account = LoadEncryptedAccount(trimmedUsername, password, accountPath);
        }
        catch (CryptographicException)
        {
            message = "Incorrect password.";
            return false;
        }
        catch (JsonException)
        {
            message = "Account file is corrupted.";
            return false;
        }

        if (!VerifyPassword(password, account))
        {
            message = "Incorrect password.";
            account = null;
            return false;
        }

        message = "Logged in successfully.";
        return true;
    }

    private static bool TryLoginLegacy(string username, string password, out UserAccount? account, out string message)
    {
        account = LoadLegacyUsers()
            .FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        if (account is null)
        {
            message = "User was not found.";
            return false;
        }

        if (!VerifyPassword(password, account))
        {
            message = "Incorrect password.";
            account = null;
            return false;
        }

        message = "Logged in successfully.";
        return true;
    }

    private static void MigrateLegacyAccount(UserAccount account, string password)
    {
        var path = UserFilePaths.GetAccountFilePath(account.Username);
        if (File.Exists(path))
        {
            return;
        }

        SaveEncryptedAccount(path, account.Username, password, account);
        RemoveFromLegacyStore(account.Username);
    }

    private static void RemoveFromLegacyStore(string username)
    {
        if (!File.Exists(LegacyUsersFilePath))
        {
            return;
        }

        var users = LoadLegacyUsers();
        users.RemoveAll(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        if (users.Count == 0)
        {
            File.Delete(LegacyUsersFilePath);
            return;
        }

        var json = JsonSerializer.Serialize(users, JsonOptions);
        File.WriteAllText(LegacyUsersFilePath, json);
    }

    private static List<UserAccount> LoadLegacyUsers()
    {
        try
        {
            if (!File.Exists(LegacyUsersFilePath))
            {
                return new List<UserAccount>();
            }

            var json = File.ReadAllText(LegacyUsersFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<UserAccount>();
            }

            return JsonSerializer.Deserialize<List<UserAccount>>(json) ?? new List<UserAccount>();
        }
        catch
        {
            return new List<UserAccount>();
        }
    }

    private static void SaveEncryptedAccount(string path, string username, string password, UserAccount account)
    {
        var json = JsonSerializer.Serialize(account, JsonOptions);
        var plaintext = Encoding.UTF8.GetBytes(json);
        var keyId = UserFilePaths.BuildSafeSegment(username);
        var session = EncryptedFileSession.Create(keyId, password, path);
        var encrypted = session.Encrypt(plaintext);
        File.WriteAllBytes(path, encrypted);
    }

    private static UserAccount LoadEncryptedAccount(string username, string password, string path)
    {
        var fileBytes = File.ReadAllBytes(path);
        var keyId = UserFilePaths.BuildSafeSegment(username);
        var session = EncryptedFileSession.Create(keyId, password, path);
        var plaintext = session.Decrypt(fileBytes);
        var json = Encoding.UTF8.GetString(plaintext);

        var account = JsonSerializer.Deserialize<UserAccount>(json)
            ?? throw new JsonException("Account file payload is empty.");

        return account;
    }

    private static bool VerifyPassword(string password, UserAccount account)
    {
        var hash = PasswordHasher.Hash(password, account.Salt);
        return string.Equals(hash, account.PasswordHash, StringComparison.Ordinal);
    }
}
