
using System.Text.Json;

namespace BudgetManagement.FileManagement;

public static class Files
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void Create(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                using (System.IO.File.Create(filePath)) { }
                Console.WriteLine($"File created at: {filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while creating the file: {ex.Message}");
        }
    }

    public static void Delete(string filePath)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                Console.WriteLine($"File deleted at: {filePath}");
            }
            else
            {
                Console.WriteLine($"File does not exist at: {filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while deleting the file: {ex.Message}");
        }
    }

    public static void Append(string filePath, double amount)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.AppendAllText(filePath, amount.ToString() + Environment.NewLine);
                Console.WriteLine($"Amount appended to file at: {filePath}");
            }
            else
            {
                Console.WriteLine($"File does not exist at: {filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while appending to the file: {ex.Message}");
        }
    }

    public static void AppendAmountByDate(string filePath, double amount, DateTime? date = null)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                using (System.IO.File.Create(filePath)) { }
            }

            var effectiveDate = (date ?? DateTime.Now).ToString("yyyy-MM-dd");
            var data = ReadDateKeyedAmountsOrConvertLegacy(filePath, effectiveDate);

            if (!data.TryGetValue(effectiveDate, out var list))
            {
                list = new List<double>();
                data[effectiveDate] = list;
            }

            list.Add(amount);

            var json = JsonSerializer.Serialize(data, JsonOptions);
            System.IO.File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while writing dated amounts JSON: {ex.Message}");
        }
    }

    public static Dictionary<string, List<double>> ReadAmountsByDate(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                return new Dictionary<string, List<double>>();
            }

            var fallbackKey = DateTime.Now.ToString("yyyy-MM-dd");
            return ReadDateKeyedAmountsOrConvertLegacy(filePath, fallbackKey);
        }
        catch
        {
            return new Dictionary<string, List<double>>();
        }
    }

    public static double ReadTotalAmount(string filePath)
    {
        var data = ReadAmountsByDate(filePath);
        return data.Values.SelectMany(x => x).Sum();
    }

    private static Dictionary<string, List<double>> ReadDateKeyedAmountsOrConvertLegacy(string filePath, string fallbackDateKey)
    {
        var text = System.IO.File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(text))
        {
            return new Dictionary<string, List<double>>();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, List<double>>>(text);
            if (parsed is not null)
            {
                return parsed;
            }
        }
        catch
        {
            // ignored: try legacy conversion below
        }

        // Legacy format: one amount per line (not JSON). Convert it under today's key.
        var converted = new Dictionary<string, List<double>>();
        var lines = System.IO.File.ReadAllLines(filePath);
        var amounts = new List<double>();
        foreach (var line in lines)
        {
            if (double.TryParse(line, out var value))
            {
                amounts.Add(value);
            }
        }

        if (amounts.Count > 0)
        {
            converted[fallbackDateKey] = amounts;
        }

        return converted;
    }

    public static void WriteCurrentBalance(string filePath, double currentBalance)
    {
        try
        {
            var payload = new
            {
                currentBalance = currentBalance,
                updatedAt = DateTimeOffset.Now
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            System.IO.File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while writing balance JSON: {ex.Message}");
        }
    }
}