
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BudgetManagement.FileManagement;

public static class Files
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private const string IncomeKey = "income";
    private const string ExpenseKey = "expense";
    private static EncryptedFileSession? _session;

    public static void BindEncryptionSession(string username, string password, string dataFilePath)
    {
        _session = EncryptedFileSession.Create(username, password, dataFilePath);
    }

    public static void MigrateEncryptedDataFile(
        string oldFilePath,
        string newFilePath,
        string oldKeyUsername,
        string newKeyUsername,
        string password)
    {
        if (!System.IO.File.Exists(oldFilePath))
        {
            return;
        }

        if (string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
        {
            var samePathSession = EncryptedFileSession.Create(newKeyUsername, password, oldFilePath);
            var fileBytes = System.IO.File.ReadAllBytes(oldFilePath);

            if (FileCrypto.IsEncryptedFile(fileBytes))
            {
                var plaintext = samePathSession.Decrypt(fileBytes);
                System.IO.File.WriteAllBytes(oldFilePath, samePathSession.Encrypt(plaintext));
            }

            return;
        }

        var oldSession = EncryptedFileSession.Create(oldKeyUsername, password, oldFilePath);
        var oldBytes = System.IO.File.ReadAllBytes(oldFilePath);
        byte[] plaintextBytes;

        if (FileCrypto.IsEncryptedFile(oldBytes))
        {
            plaintextBytes = oldSession.Decrypt(oldBytes);
        }
        else if (FileCrypto.IsPlaintextJson(oldBytes))
        {
            plaintextBytes = oldBytes;
        }
        else
        {
            throw new CryptographicException("Unrecognized user data file format.");
        }

        var newSession = EncryptedFileSession.Create(newKeyUsername, password, newFilePath);
        System.IO.File.WriteAllBytes(newFilePath, newSession.Encrypt(plaintextBytes));
    }

    public static void ReencryptWithNewPassword(
        string filePath,
        string username,
        string oldPassword,
        string newPassword)
    {
        if (!System.IO.File.Exists(filePath))
        {
            return;
        }

        var oldSession = EncryptedFileSession.Create(username, oldPassword, filePath);
        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        byte[] plaintextBytes;

        if (FileCrypto.IsEncryptedFile(fileBytes))
        {
            plaintextBytes = oldSession.Decrypt(fileBytes);
        }
        else if (FileCrypto.IsPlaintextJson(fileBytes))
        {
            plaintextBytes = fileBytes;
        }
        else
        {
            throw new CryptographicException("Unrecognized user data file format.");
        }

        var newSession = EncryptedFileSession.Create(username, newPassword, filePath);
        System.IO.File.WriteAllBytes(filePath, newSession.Encrypt(plaintextBytes));
    }

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
        RequireSession();

        if (!System.IO.File.Exists(filePath))
        {
            var empty = new UserFinanceData();
            SaveUserFinanceData(filePath, empty);
            return empty;
        }

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        if (fileBytes.Length == 0)
        {
            return new UserFinanceData();
        }

        try
        {
            string json;
            if (FileCrypto.IsEncryptedFile(fileBytes))
            {
                var plaintext = _session!.Decrypt(fileBytes);
                json = Encoding.UTF8.GetString(plaintext);
            }
            else if (FileCrypto.IsPlaintextJson(fileBytes))
            {
                json = Encoding.UTF8.GetString(fileBytes);
            }
            else
            {
                throw new CryptographicException("Unrecognized user data file format.");
            }

            var parsed = JsonSerializer.Deserialize<UserFinanceData>(json);
            if (parsed is not null)
            {
                EnsureEntryType(parsed, IncomeKey);
                EnsureEntryType(parsed, ExpenseKey);
                parsed.CurrentBalance = ReadTotalByType(parsed, IncomeKey) - ReadTotalByType(parsed, ExpenseKey);

                if (!FileCrypto.IsEncryptedFile(fileBytes))
                {
                    SaveUserFinanceData(filePath, parsed);
                }

                return parsed;
            }
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException(
                "Cannot decrypt user data. The file may be corrupted or the password is incorrect.",
                ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("User data file is corrupted.", ex);
        }

        return new UserFinanceData();
    }

    private static void SaveUserFinanceData(string filePath, UserFinanceData data)
    {
        RequireSession();

        var json = JsonSerializer.Serialize(data, JsonOptions);
        var plaintext = Encoding.UTF8.GetBytes(json);
        var encrypted = _session!.Encrypt(plaintext);
        System.IO.File.WriteAllBytes(filePath, encrypted);
    }

    private static void RequireSession()
    {
        if (_session is null)
        {
            throw new InvalidOperationException("Encryption session is not initialized. Log in first.");
        }
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
