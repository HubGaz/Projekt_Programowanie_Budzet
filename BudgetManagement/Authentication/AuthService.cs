namespace BudgetManagement.Authentication;

public static class AuthService
{
    public static bool Register(string username, string password, string confirmPassword, out string message)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            message = "Username cannot be empty.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            message = "Password cannot be empty.";
            return false;
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            message = "Passwords do not match.";
            return false;
        }

        if (UserStore.IsUsernameTaken(username))
        {
            message = "A user with this name already exists.";
            return false;
        }

        var salt = PasswordHasher.CreateSalt();
        var account = new UserAccount
        {
            Username = username.Trim(),
            Salt = salt,
            PasswordHash = PasswordHasher.Hash(password, salt)
        };

        return UserStore.TryCreateAccount(account, password, out message);
    }

    public static bool Login(string username, string password, out string message, out string? loggedInUsername)
    {
        loggedInUsername = null;

        if (UserStore.TryLogin(username, password, out var account, out message))
        {
            loggedInUsername = account!.Username;
            return true;
        }

        return false;
    }
}
