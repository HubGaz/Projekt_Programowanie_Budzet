using System.Text.Json;

namespace BudgetManagement.Authentication;

public static class UserStore
{
    private const string UsersFilePath = "users.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static List<UserAccount> LoadUsers()
    {
        try
        {
            if (!File.Exists(UsersFilePath))
            {
                return new List<UserAccount>();
            }

            var json = File.ReadAllText(UsersFilePath);
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

    public static void SaveUsers(List<UserAccount> users)
    {
        var json = JsonSerializer.Serialize(users, JsonOptions);
        File.WriteAllText(UsersFilePath, json);
    }
}
