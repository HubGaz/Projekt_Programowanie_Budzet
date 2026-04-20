
using BudgetManagement.FileManagement;
using BudgetManagement.Authentication;
using BudgetManagement.MoneyManagement;
using BudgetManagement.Miscellaneous;
using System.Globalization;

namespace main
{
    class Program
    {
        static void Main(string[] args)
        {
            using var soundPlayer = new AsyncSoundPlayer();

            var loggedInUsername = ShowAuthScreen(soundPlayer);
            if (loggedInUsername is null)
            {
                return;
            }

            var userFiles = new UserFilePaths(loggedInUsername);
            Files.Create(userFiles.IncomeFilePath);
            Files.Create(userFiles.ExpenseFilePath);
            Files.Create(userFiles.BalanceFilePath);
            double? monthlyExpenseLimit = null;

            while (true)
            {
                Incomes.Total_Incomes = Files.ReadTotalAmount(userFiles.IncomeFilePath);
                Expenses.Total_Expenses = Files.ReadTotalAmount(userFiles.ExpenseFilePath);
                Files.WriteCurrentBalance(userFiles.BalanceFilePath, Incomes.Total_Incomes - Expenses.Total_Expenses);

                try
                {
                    Console.Clear();
                }
                catch (IOException)
                {
                    // Some debug/host environments don't support console clear.
                }

                Aestetics.Logo();
                Console.WriteLine($"Logged in as: {loggedInUsername}");

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
                    soundPlayer.Play(SoundEffect.Error);
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
                            Files.AppendAmountByDate(userFiles.IncomeFilePath, income);
                            Files.WriteCurrentBalance(userFiles.BalanceFilePath, Incomes.Total_Incomes - Expenses.Total_Expenses);
                            Console.WriteLine("Income added.");
                            soundPlayer.Play(SoundEffect.Success);
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount.");
                            soundPlayer.Play(SoundEffect.Error);
                        }
                        break;
                    case "2": Console.WriteLine("-> Adding expense...");
                    Console.Write("Set/Change monthly limit now? (y/n): ");
                    string? changeLimit = Console.ReadLine();
                    if (string.Equals(changeLimit, "y", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Write("Enter monthly limit amount: ");
                        if (double.TryParse(Console.ReadLine(), out double newLimit) && newLimit >= 0)
                        {
                            monthlyExpenseLimit = newLimit;
                            Console.WriteLine($"Monthly limit set to: {monthlyExpenseLimit:F2}");
                        }
                        else
                        {
                            Console.WriteLine("Invalid limit amount. Keeping previous limit.");
                            soundPlayer.Play(SoundEffect.Error);
                        }
                    }

                    Console.Write("Enter expense amount: ");
                        if (double.TryParse(Console.ReadLine(), out double expense))
                        {
                            var currentMonthExpenses = GetCurrentMonthTotal(Files.ReadAmountsByDate(userFiles.ExpenseFilePath));
                            if (monthlyExpenseLimit.HasValue && currentMonthExpenses + expense > monthlyExpenseLimit.Value)
                            {
                                Console.WriteLine($"Monthly limit exceeded. Current month spent: {currentMonthExpenses:F2}, limit: {monthlyExpenseLimit.Value:F2}");
                                soundPlayer.Play(SoundEffect.Warning);
                                break;
                            }

                            Expenses.AddExpense(expense);
                            Files.AppendAmountByDate(userFiles.ExpenseFilePath, expense);
                            Files.WriteCurrentBalance(userFiles.BalanceFilePath, Incomes.Total_Incomes - Expenses.Total_Expenses);
                            Console.WriteLine("Expense added.");
                            soundPlayer.Play(SoundEffect.Success);
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount.");
                            soundPlayer.Play(SoundEffect.Error);
                        }
                    break;
                    case "3":
                        Console.WriteLine("-> Current balance:");
                        Console.WriteLine((Incomes.Total_Incomes - Expenses.Total_Expenses).ToString("F2"));
                        break;
                    case "4":
                        Console.WriteLine("-> Expense history:");
                        {
                            var history = Files.ReadAmountsByDate(userFiles.ExpenseFilePath);
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
                            var history = Files.ReadAmountsByDate(userFiles.IncomeFilePath);
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
                            Files.Delete(userFiles.IncomeFilePath);
                            Files.Delete(userFiles.ExpenseFilePath);
                            Files.Delete(userFiles.BalanceFilePath);
                            Console.WriteLine("All files deleted.");
                            Files.Create(userFiles.IncomeFilePath);
                            Files.Create(userFiles.ExpenseFilePath);
                            Files.Create(userFiles.BalanceFilePath);
                            Incomes.Total_Incomes = 0.0;
                            Expenses.Total_Expenses = 0.0;
                            soundPlayer.Play(SoundEffect.Warning);
                        }
                        else
                        {
                            Console.WriteLine("Delete cancelled.");
                            soundPlayer.Play(SoundEffect.Info);
                        }
                        break;
                    case "7":
                        Console.WriteLine("-> Goodbye!");
                        soundPlayer.Play(SoundEffect.Info);
                        Aestetics.WaitForEnter();
                        return;
                    default:
                        Console.WriteLine("-> Invalid option.");
                        soundPlayer.Play(SoundEffect.Error);
                        break;
                    
                }

                Aestetics.WaitForEnter();
            }
        }

        private static string? ShowAuthScreen(AsyncSoundPlayer soundPlayer)
        {
            while (true)
            {
                try
                {
                    Console.Clear();
                }
                catch (IOException)
                {
                    // ignored
                }

                Aestetics.Logo();
                Console.WriteLine();
                Console.WriteLine("=== Login ===");
                Console.WriteLine("1. Sign in");
                Console.WriteLine("2. Create account");
                Console.WriteLine("3. Exit");
                Console.Write("Choose option (1-3): ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Username: ");
                        var loginUsername = Console.ReadLine() ?? string.Empty;
                        Console.Write("Password: ");
                        var loginPassword = Console.ReadLine() ?? string.Empty;

                        if (AuthService.Login(loginUsername, loginPassword, out var loginMessage))
                        {
                            Console.WriteLine(loginMessage);
                            soundPlayer.Play(SoundEffect.Success);
                            Aestetics.WaitForEnter();
                            return loginUsername.Trim();
                        }

                        Console.WriteLine(loginMessage);
                        soundPlayer.Play(SoundEffect.Error);
                        Aestetics.WaitForEnter();
                        break;
                    case "2":
                        Console.Write("Username: ");
                        var registerUsername = Console.ReadLine() ?? string.Empty;
                        Console.Write("Password: ");
                        var registerPassword = Console.ReadLine() ?? string.Empty;
                        Console.Write("Repeat password: ");
                        var repeatPassword = Console.ReadLine() ?? string.Empty;

                        var registerSucceeded = AuthService.Register(registerUsername, registerPassword, repeatPassword, out var registerMessage);
                        Console.WriteLine(registerMessage);
                        if (registerSucceeded)
                        {
                            soundPlayer.Play(SoundEffect.Success);
                        }
                        else
                        {
                            soundPlayer.Play(SoundEffect.Error);
                        }
                        Aestetics.WaitForEnter();
                        break;
                    case "3":
                        soundPlayer.Play(SoundEffect.Info);
                        return null;
                    default:
                        Console.WriteLine("Invalid option.");
                        soundPlayer.Play(SoundEffect.Error);
                        Aestetics.WaitForEnter();
                        break;
                }
            }
        }

        private static double GetCurrentMonthTotal(Dictionary<string, List<double>> history)
        {
            var now = DateTime.Now;
            double total = 0;

            foreach (var day in history)
            {
                if (DateTime.TryParseExact(day.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
                    date.Month == now.Month &&
                    date.Year == now.Year)
                {
                    total += day.Value.Sum();
                }
            }

            return total;
        }
    }
}