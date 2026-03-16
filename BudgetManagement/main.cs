using System;
using System.IO;

using BudgetManagement.FileManagement;
using BudgetManagement.MoneyManagement;

namespace main
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Files.Create("income.json");

                try
                {
                    Console.Clear();
                }
                catch (IOException)
                {
                    // Some debug/host environments don't support console clear.
                }
                Console.WriteLine("=== Menu ===");
                Console.WriteLine("1. Add income");
                Console.WriteLine("2. Add expense");
                Console.WriteLine("3. View balance");
                Console.WriteLine("4. Check current Expenses");
                Console.WriteLine("5. Exit");
                Console.Write("Choose option (1-5): ");
                string? input = Console.ReadLine();

                if (input is null)
                {
                    Console.WriteLine("-> Invalid option.");
                    continue;
                }

                switch (input)
                {
                    case "1": Console.WriteLine("-> Adding income...");
                            Console.Write("Enter income amount: ");
                        if (double.TryParse(Console.ReadLine(), out double income))
                        {
                            Incomes.AddIncome(income);
                            Files.Append("income.json", income);
                            Console.WriteLine("Income added.");
                        }
                        break;
                    case "2": Console.WriteLine("-> Adding expense...");
                    Console.Write("Enter expense amount: ");
                        if (double.TryParse(Console.ReadLine(), out double expense))
                        {
                            Expenses.AddExpense(expense);
                            Console.WriteLine("Expense added.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount.");
                        }
                    break;
                    case "3": Console.WriteLine("-> Showing balance..."); break;
                    case "4":
                        Console.WriteLine($"-> Current expenses: " + Expenses.Total_Expenses);
                        break;
                    case "5":
                        Console.WriteLine("-> Goodbye!");
                        return;
                    default: Console.WriteLine("-> Invalid option."); break;
                    
                }
            }
        }

    }
}