using System;
namespace main
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("=== Menu ===");
                Console.WriteLine("1. Add income");
                Console.WriteLine("2. Add expense");
                Console.WriteLine("3. View balance");
                Console.WriteLine("4. Exit");
                Console.WriteLine("5. Check current Expenses");
                Console.Write("Choose option (1-5): ");
                string? input = Console.ReadLine();

                if (input is null)
                {
                    Console.WriteLine("-> Invalid option.");
                    continue;
                }

                switch (input)
                {
                    case "1": Console.WriteLine("-> Adding income..."); break;
                    case "2": Console.WriteLine("-> Adding expense...");
                        Console.Write("Enter expense amount: ");
                        if (double.TryParse(Console.ReadLine(), out double expense))
                        {
                            Console.Write("Enter expense description: ");
                            string? description = Console.ReadLine();

                            if (string.IsNullOrWhiteSpace(description))
                            {
                                Console.WriteLine("Description is required. Expense not added.");
                            }
                            else
                            {
                                Expenses.AddExpense(expense, description);
                                Console.WriteLine("Expense added.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount.");
                        }
                        break;
                    case "3": Console.WriteLine("-> Showing balance..."); break;
                    case "4":
                        Console.WriteLine("-> Goodbye!");
                        return;
                    case "5":
                        Console.WriteLine("-> Current expenses:");
                        if (Expenses.ExpenseList.Count == 0)
                        {
                            Console.WriteLine("No expenses recorded.");
                        }
                        else
                        {
                            foreach (var item in Expenses.ExpenseList)
                            {
                                Console.WriteLine($"- {item.Amount} : {item.Description}");
                            }

                            Console.WriteLine($"Total expenses: {Expenses.Total_Expenses}");
                        }
                        break;
                    default: Console.WriteLine("-> Invalid option."); break;
                    
                }
            }
        }

    }
}