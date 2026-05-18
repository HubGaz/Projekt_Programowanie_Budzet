
using System.Text.Json;

namespace BudgetManagement.FileManagement;

public static class Files
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private const string IncomeKey = "income";
    private const string ExpenseKey = "expense";

    public static void EnsureUserDataFile(string dataFilePath)
    {
        try
        {
            ReadUserFinanceData(dataFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while creating user data file: {ex.Message}");
        }
    }

    public static void ResetUserData(string dataFilePath)
    {
        try
        {
            var emptyData = new UserFinanceData();
            SaveUserFinanceData(dataFilePath, emptyData);
            Console.WriteLine($"User data reset in: {dataFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while resetting user data: {ex.Message}");
        }
    }

    public static void AppendIncomeByDate(string dataFilePath, double amount, DateTime? date = null)
    {
        AppendEntryByDate(dataFilePath, IncomeKey, amount, date);
    }

    public static void AppendExpenseByDate(string dataFilePath, double amount, DateTime? date = null)
    {
        AppendEntryByDate(dataFilePath, ExpenseKey, amount, date);
    }

    public static Dictionary<string, List<double>> ReadIncomeAmountsByDate(string dataFilePath)
    {
        return ReadEntriesByDate(dataFilePath, IncomeKey);
    }

    public static Dictionary<string, List<double>> ReadExpenseAmountsByDate(string dataFilePath)
    {
        return ReadEntriesByDate(dataFilePath, ExpenseKey);
    }

    public static double ReadTotalIncome(string dataFilePath)
    {
        return ReadIncomeAmountsByDate(dataFilePath).Values.SelectMany(x => x).Sum();
    }

    public static double ReadTotalExpense(string dataFilePath)
    {
        return ReadExpenseAmountsByDate(dataFilePath).Values.SelectMany(x => x).Sum();
    }

    public static double ReadCurrentBalance(string dataFilePath)
    {
        return ReadTotalIncome(dataFilePath) - ReadTotalExpense(dataFilePath);
    }

    private static void AppendEntryByDate(string dataFilePath, string entryType, double amount, DateTime? date = null)
    {
        try
        {
            var data = ReadUserFinanceData(dataFilePath);
            var effectiveDate = (date ?? DateTime.Now).ToString("yyyy-MM-dd");
            var entries = EnsureEntryType(data, entryType);

            if (!entries.TryGetValue(effectiveDate, out var list))
            {
                list = new List<double>();
                entries[effectiveDate] = list;
            }

            list.Add(amount);
            data.CurrentBalance = ReadTotalByType(data, IncomeKey) - ReadTotalByType(data, ExpenseKey);
            data.UpdatedAt = DateTimeOffset.Now;

            SaveUserFinanceData(dataFilePath, data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while writing dated amounts JSON: {ex.Message}");
        }
    }

    private static Dictionary<string, List<double>> ReadEntriesByDate(string dataFilePath, string entryType)
    {
        try
        {
            var data = ReadUserFinanceData(dataFilePath);
            return EnsureEntryType(data, entryType);
        }
        catch
        {
            return new Dictionary<string, List<double>>();
        }
    }

    private static UserFinanceData ReadUserFinanceData(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            var empty = new UserFinanceData();
            SaveUserFinanceData(filePath, empty);
            return empty;
        }

        var text = System.IO.File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(text))
        {
            return new UserFinanceData();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<UserFinanceData>(text);
            if (parsed is not null)
            {
                EnsureEntryType(parsed, IncomeKey);
                EnsureEntryType(parsed, ExpenseKey);
                parsed.CurrentBalance = ReadTotalByType(parsed, IncomeKey) - ReadTotalByType(parsed, ExpenseKey);
                return parsed;
            }
        }
        catch
        {
            // ignored: return empty
        }

        return new UserFinanceData();
    }

    private static void SaveUserFinanceData(string filePath, UserFinanceData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        System.IO.File.WriteAllText(filePath, json);
    }

    private static Dictionary<string, List<double>> EnsureEntryType(UserFinanceData data, string entryType)
    {
        if (!data.Entries.TryGetValue(entryType, out var entries))
        {
            entries = new Dictionary<string, List<double>>();
            data.Entries[entryType] = entries;
        }

        return entries;
    }

    private static double ReadTotalByType(UserFinanceData data, string entryType)
    {
        var entries = EnsureEntryType(data, entryType);
        return entries.Values.SelectMany(x => x).Sum();
    }

    private sealed class UserFinanceData
    {
        public Dictionary<string, Dictionary<string, List<double>>> Entries { get; set; } =
            new()
            {
                [IncomeKey] = new Dictionary<string, List<double>>(),
                [ExpenseKey] = new Dictionary<string, List<double>>()
            };
        public double CurrentBalance { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    }
}
