namespace BudgetManagement.Authentication;

public sealed class UserFilePaths
{
    public string IncomeFilePath { get; }
    public string ExpenseFilePath { get; }
    public string BalanceFilePath { get; }

    public UserFilePaths(string username)
    {
        var safeUsername = BuildSafeSegment(username);
        IncomeFilePath = $"{safeUsername}_income.json";
        ExpenseFilePath = $"{safeUsername}_expense.json";
        BalanceFilePath = $"{safeUsername}_balance.json";
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
