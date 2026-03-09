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
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1": Console.WriteLine("-> Adding income..."); break;
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
                        Console.WriteLine("-> Goodbye!");
                        return;
                    case "5": Console.WriteLine($"->Current expenses: " + Expenses.Total_Expenses); break;
                    default: Console.WriteLine("-> Invalid option."); break;
                    
                }
            }
        }

    }
}