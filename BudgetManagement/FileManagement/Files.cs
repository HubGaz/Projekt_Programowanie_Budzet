
using System.Text.Json;

namespace BudgetManagement.FileManagement;

public static class Files
{
    public static void Create(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                using (System.IO.File.Create(filePath)) { }
                Console.WriteLine($"File created at: {filePath}");
            }
            else
            {
                Console.WriteLine($"File already exists at: {filePath}");
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

    public static void WriteCurrentBalance(string filePath, double currentBalance)
    {
        try
        {
            var payload = new
            {
                currentBalance = currentBalance,
                updatedAt = DateTimeOffset.Now
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while writing balance JSON: {ex.Message}");
        }
    }
}