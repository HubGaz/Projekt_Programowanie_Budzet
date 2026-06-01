namespace BudgetManagement.Authentication;

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
}
