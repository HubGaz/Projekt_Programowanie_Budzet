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
}
