namespace BudgetManagement.Authentication;

public sealed class UserFilePaths
{
    public string DataFilePath { get; }

    public UserFilePaths(string username)
    {
        var safeUsername = BuildSafeSegment(username);
        DataFilePath = $"{safeUsername}_finance.json";
    }

    private static string BuildSafeSegment(string input)
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

        return cleaned.Replace(' ', '_');
    }
}
