namespace BudgetManagement.Authentication;

public sealed class UserFilePaths
{
    public string DataFilePath { get; }
    public string AccountFilePath { get; }

    public UserFilePaths(string username)
    {
        var safeUsername = BuildSafeSegment(username);
        DataFilePath = $"{safeUsername}_finance.json";
        AccountFilePath = $"{safeUsername}.account";
    }

    public static string GetAccountFilePath(string username) =>
        $"{BuildSafeSegment(username)}.account";

    public static string BuildSafeSegment(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "user";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(input
            .Trim()
            .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
            .ToArray());

        return cleaned.Replace(' ', '_').ToLowerInvariant();
    }
}
