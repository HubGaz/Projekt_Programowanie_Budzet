
using BudgetManagement.FileManagement;
using BudgetManagement.MoneyManagement;
using BudgetManagement.Miscellaneous;

namespace main
{
    class Program
    {


        static void Main(string[] args)
        {
            Files.Create("income.json");
            Files.Create("expense.json");
            Files.Create("balance.json");

            while (true)
            {
                Incomes.Total_Incomes = Files.ReadTotalAmount("income.json");
                Expenses.Total_Expenses = Files.ReadTotalAmount("expense.json");
                Files.WriteCurrentBalance("balance.json", Incomes.Total_Incomes - Expenses.Total_Expenses);

                try
                {
                    Console.Clear();
                }
                catch (IOException)
                {
                    // Some debug/host environments don't support console clear.
                }

                Aestetics.Logo();

                Console.WriteLine("");
                Console.WriteLine("=== Menu ===");
                Console.WriteLine("1. Add income");
                Console.WriteLine("2. Add expense");
                Console.WriteLine("3. View balance");
                Console.WriteLine("4. Check expense history");
                Console.WriteLine("5. Check income history");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("6. Clear all entries");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("7. Exit");
                Console.Write("Choose option (1-7): ");
                string? input = Console.ReadLine();

                if (input is null)
                {
                    Console.WriteLine("-> Invalid option.");
                    Aestetics.WaitForEnter();
                    continue;
                }

                switch (input)
                {
                    case "1": Console.WriteLine("-> Adding income...");
                            Console.Write("Enter income amount: ");
                        if (double.TryParse(Console.ReadLine(), out double income))
                        {
                            Incomes.AddIncome(income);
                            Files.AppendAmountByDate("income.json", income);
                            Files.WriteCurrentBalance("balance.json", Incomes.Total_Incomes - Expenses.Total_Expenses);
                            Console.WriteLine("Income added.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount.");
                        }
                        break;
                    case "2": Console.WriteLine("-> Adding expense...");
                    Console.Write("Enter expense amount: ");
                        if (double.TryParse(Console.ReadLine(), out double expense))
                        {
                            Expenses.AddExpense(expense);
                            Files.AppendAmountByDate("expense.json", expense);
                            Files.WriteCurrentBalance("balance.json", Incomes.Total_Incomes - Expenses.Total_Expenses);
                            Console.WriteLine("Expense added.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount.");
                        }
                    break;
                    case "3":
                        Console.WriteLine("-> Current balance:");
                        Console.WriteLine((Incomes.Total_Incomes - Expenses.Total_Expenses).ToString("F2"));
                        break;
                    case "4":
                        Console.WriteLine("-> Expense history:");
                        {
                            var history = Files.ReadAmountsByDate("expense.json");
                            if (history.Count == 0)
                            {
                                Console.WriteLine("(empty)");
                                break;
                            }

                            foreach (var day in history.OrderBy(kvp => kvp.Key))
                            {
                                var sum = day.Value.Sum();
                                Console.WriteLine($"{day.Key}: {sum} ({day.Value.Count} entries)");
                            }
                        }
                        break;
                    case "5":
                        Console.WriteLine("-> Income history:");
                        {
                            var history = Files.ReadAmountsByDate("income.json");
                            if (history.Count == 0)
                            {
                                Console.WriteLine("(empty)");
                                break;
                            }

                            foreach (var day in history.OrderBy(kvp => kvp.Key))
                            {
                                var sum = day.Value.Sum();
                                Console.WriteLine($"{day.Key}: {sum} ({day.Value.Count} entries)");
                            }
                        }
                        break;
                    case "6":
                        Console.Write("Are you sure you want to delete all entries? (y/n): ");
                        string? confirmDelete = Console.ReadLine();
                        if (string.Equals(confirmDelete, "y", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("-> Deleting all entries...");
                            Files.Delete("income.json");
                            Files.Delete("expense.json");
                            Files.Delete("balance.json");
                            Console.WriteLine("All files deleted.");
                            Files.Create("income.json");
                            Files.Create("expense.json");
                            Files.Create("balance.json");
                            Incomes.Total_Incomes = 0.0;
                            Expenses.Total_Expenses = 0.0;
                        }
                        else
                        {
                            Console.WriteLine("Delete cancelled.");
                        }
                        break;
                    case "7":
                        Console.WriteLine("-> Goodbye!");
                        Aestetics.WaitForEnter();
                        return;
                    default: Console.WriteLine("-> Invalid option."); break;
                    
                }

                Aestetics.WaitForEnter();
            }
        }

    }
}