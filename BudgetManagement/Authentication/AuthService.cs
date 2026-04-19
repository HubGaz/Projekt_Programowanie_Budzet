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

        var users = UserStore.LoadUsers();
        if (users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
        {
            message = "A user with this name already exists.";
            return false;
        }

        var salt = PasswordHasher.CreateSalt();
        users.Add(new UserAccount
        {
            Username = username.Trim(),
            Salt = salt,
            PasswordHash = PasswordHasher.Hash(password, salt)
        });

        UserStore.SaveUsers(users);
        message = "Account has been created.";
        return true;
    }

    public static bool Login(string username, string password, out string message)
    {
        var users = UserStore.LoadUsers();
        var user = users.FirstOrDefault(u =>
            string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            message = "User was not found.";
            return false;
        }

        var hash = PasswordHasher.Hash(password, user.Salt);
        if (!string.Equals(hash, user.PasswordHash, StringComparison.Ordinal))
        {
            message = "Incorrect password.";
            return false;
        }

        message = "Logged in successfully.";
        return true;
    }
}
