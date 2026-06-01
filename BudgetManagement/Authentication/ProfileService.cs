namespace BudgetManagement.Authentication;

public enum AccountSettingsOutcome
{
    Continue,
    CredentialsChanged,
    SessionEnded
}

public static class ProfileService
{
    public static bool ChangeUsername(
        string currentUsername,
        string password,
        string newUsername,
        out string message)
    {
        if (string.IsNullOrWhiteSpace(newUsername))
        {
            message = "Username cannot be empty.";
            return false;
        }

        return UserStore.TryChangeUsername(currentUsername, password, newUsername, out message);
    }

    public static bool ChangePassword(
        string currentUsername,
        string currentPassword,
        string newPassword,
        string confirmPassword,
        out string message)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            message = "Password cannot be empty.";
            return false;
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            message = "Passwords do not match.";
            return false;
        }

        return UserStore.TryChangePassword(currentUsername, currentPassword, newPassword, out message);
    }

    public static bool SetAlias(
        string username,
        string password,
        string? alias,
        out string message) =>
        UserStore.TrySetAlias(username, password, alias, out message);

    public static bool SuspendAccount(
        string username,
        string password,
        string? reason,
        out string message) =>
        UserStore.TrySuspendAccount(username, password, reason, out message);

    public static bool ReactivateAccount(string username, string password, out string message) =>
        UserStore.TryUnsuspendAccount(username, password, out message);

    public static bool DeleteAccount(string username, string password, out string message) =>
        UserStore.TryDeleteAccount(username, password, out message);

    public static string FormatLoginLabel(string username, string? alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return username;
        }

        return $"{alias.Trim()} ({username})";
    }
}
