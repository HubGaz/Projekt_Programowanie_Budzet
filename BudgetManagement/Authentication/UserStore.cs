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

    public static bool TryChangeUsername(
        string currentUsername,
        string password,
        string newUsername,
        out string message)
    {
        var trimmedCurrent = currentUsername.Trim();
        var trimmedNew = newUsername.Trim();

        if (string.Equals(trimmedCurrent, trimmedNew, StringComparison.OrdinalIgnoreCase))
        {
            message = "The new username is the same as the current one.";
            return false;
        }

        if (!TryLogin(trimmedCurrent, password, out var account, out message) || account is null)
        {
            return false;
        }

        if (IsUsernameTaken(trimmedNew))
        {
            message = "A user with this name already exists.";
            return false;
        }

        var oldPaths = new UserFilePaths(account.Username);
        var newPaths = new UserFilePaths(trimmedNew);
        var oldAccountPath = UserFilePaths.GetAccountFilePath(account.Username);
        var newAccountPath = UserFilePaths.GetAccountFilePath(trimmedNew);

        if (File.Exists(newAccountPath))
        {
            message = "A user with this name already exists.";
            return false;
        }

        var previousUsername = account.Username;
        account.Username = trimmedNew;

        try
        {
            SaveEncryptedAccount(newAccountPath, trimmedNew, password, account);
            FileManagement.Files.MigrateEncryptedDataFile(
                oldPaths.DataFilePath,
                newPaths.DataFilePath,
                previousUsername,
                trimmedNew,
                password);

            if (File.Exists(oldAccountPath))
            {
                File.Delete(oldAccountPath);
            }

            if (File.Exists(oldPaths.DataFilePath)
                && !string.Equals(oldPaths.DataFilePath, newPaths.DataFilePath, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(oldPaths.DataFilePath);
            }

            message = "Username has been changed.";
            return true;
        }
        catch (Exception ex)
        {
            if (File.Exists(newAccountPath))
            {
                try
                {
                    File.Delete(newAccountPath);
                }
                catch
                {
                    // Best-effort rollback.
                }
            }

            message = $"Could not change username: {ex.Message}";
            return false;
        }
    }

    public static bool TryChangePassword(
        string currentUsername,
        string currentPassword,
        string newPassword,
        out string message)
    {
        var trimmedUsername = currentUsername.Trim();

        if (!TryLogin(trimmedUsername, currentPassword, out var account, out message) || account is null)
        {
            return false;
        }

        if (string.Equals(currentPassword, newPassword, StringComparison.Ordinal))
        {
            message = "The new password is the same as the current one.";
            return false;
        }

        var paths = new UserFilePaths(account.Username);
        var accountPath = UserFilePaths.GetAccountFilePath(account.Username);

        account.Salt = PasswordHasher.CreateSalt();
        account.PasswordHash = PasswordHasher.Hash(newPassword, account.Salt);

        try
        {
            SaveEncryptedAccount(accountPath, account.Username, newPassword, account);
            FileManagement.Files.ReencryptWithNewPassword(
                paths.DataFilePath,
                account.Username,
                currentPassword,
                newPassword);

            message = "Password has been changed.";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Could not change password: {ex.Message}";
            return false;
        }
    }

    public static bool TrySetAlias(
        string username,
        string password,
        string? alias,
        out string message)
    {
        if (!TryVerifyCredentials(username, password, out var account, out message) || account is null)
        {
            return false;
        }

        if (account.IsSuspended)
        {
            message = "Cannot change alias while the account is suspended.";
            return false;
        }

        var trimmedAlias = string.IsNullOrWhiteSpace(alias) ? null : alias.Trim();

        if (trimmedAlias is not null && trimmedAlias.Length > 64)
        {
            message = "Alias cannot be longer than 64 characters.";
            return false;
        }

        account.Alias = trimmedAlias;

        try
        {
            SaveEncryptedAccount(
                UserFilePaths.GetAccountFilePath(account.Username),
                account.Username,
                password,
                account);
            message = trimmedAlias is null
                ? "Alias has been removed."
                : "Alias has been updated.";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Could not update alias: {ex.Message}";
            return false;
        }
    }

    public static bool TrySuspendAccount(
        string username,
        string password,
        string? reason,
        out string message)
    {
        if (!TryVerifyCredentials(username, password, out var account, out message) || account is null)
        {
            return false;
        }

        if (account.IsSuspended)
        {
            message = "Account is already suspended.";
            return false;
        }

        account.IsSuspended = true;
        account.SuspendedAt = DateTimeOffset.UtcNow;
        account.SuspensionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        try
        {
            SaveEncryptedAccount(
                UserFilePaths.GetAccountFilePath(account.Username),
                account.Username,
                password,
                account);
            message = "Account has been suspended. Sign in again after reactivation.";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Could not suspend account: {ex.Message}";
            return false;
        }
    }

    public static bool TryUnsuspendAccount(string username, string password, out string message)
    {
        if (!TryVerifyCredentials(username, password, out var account, out message) || account is null)
        {
            return false;
        }

        if (!account.IsSuspended)
        {
            message = "Account is not suspended.";
            return false;
        }

        account.IsSuspended = false;
        account.SuspendedAt = null;
        account.SuspensionReason = null;

        try
        {
            SaveEncryptedAccount(
                UserFilePaths.GetAccountFilePath(account.Username),
                account.Username,
                password,
                account);
            message = "Account has been reactivated. You can sign in now.";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Could not reactivate account: {ex.Message}";
            return false;
        }
    }

    public static bool TryDeleteAccount(string username, string password, out string message)
    {
        if (!TryVerifyCredentials(username, password, out var account, out message) || account is null)
        {
            return false;
        }

        var paths = new UserFilePaths(account.Username);
        var accountPath = UserFilePaths.GetAccountFilePath(account.Username);

        try
        {
            if (File.Exists(accountPath))
            {
                File.Delete(accountPath);
            }

            if (File.Exists(paths.DataFilePath))
            {
                File.Delete(paths.DataFilePath);
            }

            RemoveFromLegacyStore(account.Username);
            message = "Account and associated data have been deleted.";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Could not delete account: {ex.Message}";
            return false;
        }
    }

    public static bool TryLogin(string username, string password, out UserAccount? account, out string message)
    {
        account = null;
        var trimmedUsername = username.Trim();
        var accountPath = UserFilePaths.GetAccountFilePath(trimmedUsername);
        var needsMigration = !File.Exists(accountPath);

        if (!TryVerifyCredentials(username, password, out account, out message) || account is null)
        {
            return false;
        }

        if (needsMigration)
        {
            try
            {
                MigrateLegacyAccount(account, password);
            }
            catch (Exception ex)
            {
                message = $"Login succeeded but migration failed: {ex.Message}";
                account = null;
                return false;
            }
        }

        if (account.IsSuspended)
        {
            var reasonSuffix = string.IsNullOrWhiteSpace(account.SuspensionReason)
                ? string.Empty
                : $" Reason: {account.SuspensionReason.Trim()}";
            message = $"Account is suspended.{reasonSuffix} Use \"Reactivate account\" on the login screen.";
            account = null;
            return false;
        }

        message = "Logged in successfully.";
        return true;
    }

    private static bool TryVerifyCredentials(
        string username,
        string password,
        out UserAccount? account,
        out string message)
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

            if (!VerifyPassword(password, account!))
            {
                message = "Incorrect password.";
                account = null;
                return false;
            }

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

        message = "Credentials verified.";
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
