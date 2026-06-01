
using BudgetManagement.FileManagement;
using BudgetManagement.Authentication;
using BudgetManagement.MoneyManagement;
using BudgetManagement.Miscellaneous;

namespace main
{
    class Program
    {
        static void Main(string[] args)
        {
            using var soundPlayer = new AsyncSoundPlayer();
            bool isSoundEnabled = true;
            var currentTextColor = ConsoleColor.White;
            void PlaySound(SoundEffect effect)
            {
                if (isSoundEnabled)
                {
                    soundPlayer.Play(effect);
                }
            }

            var loginResult = ShowAuthScreen(soundPlayer);
            if (loginResult is null)
            {
                return;
            }

            var loggedInUsername = loginResult.Username;
            var loggedInPassword = loginResult.Password;
            var userFiles = new UserFilePaths(loggedInUsername);

            try
            {
                Files.BindEncryptionSession(loggedInUsername, loggedInPassword, userFiles.DataFilePath);
                Files.EnsureUserDataFile(userFiles.DataFilePath);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                soundPlayer.Play(SoundEffect.Error);
                Aestetics.WaitForEnter();
                return;
            }

            while (true)
            {
                Incomes.Total_Incomes = Files.ReadTotalIncome(userFiles.DataFilePath);
                Expenses.Total_Expenses = Files.ReadTotalExpense(userFiles.DataFilePath);

                try
                {
                    Console.Clear();
                }
                catch (IOException)
                {
                    // Some debug/host environments don't support console clear.
                }

                Console.ForegroundColor = currentTextColor;
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
                Console.ForegroundColor = currentTextColor;
                Console.WriteLine("7. Exit");
                Console.WriteLine($"8. Toggle sounds ({(isSoundEnabled ? "ON" : "OFF")})");
                Console.WriteLine($"9. Change text color (current: {currentTextColor})");
                Console.WriteLine("10. Account settings");
                Console.Write("Choose option (1-10): ");
                string? input = Console.ReadLine();

                if (input is null)
                {
                    Console.WriteLine("-> Invalid option.");
                    PlaySound(SoundEffect.Error);
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
                            Files.AppendIncomeByDate(userFiles.DataFilePath, income);
                            Console.WriteLine("Income added.");
                            PlaySound(SoundEffect.Success);
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount.");
                            PlaySound(SoundEffect.Error);
                        }
                        break;
                    case "2": Console.WriteLine("-> Adding expense...");
                    Console.Write("Enter expense amount: ");
                        if (double.TryParse(Console.ReadLine(), out double expense))
                        {
                            Expenses.AddExpense(expense);
                            Files.AppendExpenseByDate(userFiles.DataFilePath, expense);
                            Console.WriteLine("Expense added.");
                            PlaySound(SoundEffect.Success);
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount.");
                            PlaySound(SoundEffect.Error);
                        }
                    break;
                    case "3":
                        Console.WriteLine("-> Current balance:");
                        Console.WriteLine((Incomes.Total_Incomes - Expenses.Total_Expenses).ToString("F2"));
                        break;
                    case "4":
                        Console.WriteLine("-> Expense history:");
                        {
                            var history = Files.ReadExpenseAmountsByDate(userFiles.DataFilePath);
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
                            var history = Files.ReadIncomeAmountsByDate(userFiles.DataFilePath);
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
                            Files.ResetUserData(userFiles.DataFilePath);
                            Console.WriteLine("All entries deleted.");
                            Incomes.Total_Incomes = 0.0;
                            Expenses.Total_Expenses = 0.0;
                            PlaySound(SoundEffect.Warning);
                        }
                        else
                        {
                            Console.WriteLine("Delete cancelled.");
                            PlaySound(SoundEffect.Info);
                        }
                        break;
                    case "7":
                        Console.WriteLine("-> Goodbye!");
                        PlaySound(SoundEffect.Info);
                        Aestetics.WaitForEnter();
                        return;
                    case "8":
                        isSoundEnabled = !isSoundEnabled;
                        Console.WriteLine($"Sounds are now {(isSoundEnabled ? "ON" : "OFF")}.");
                        break;
                    case "9":
                        Console.WriteLine("Choose text color:");
                        Console.WriteLine("1. White");
                        Console.WriteLine("2. Green");
                        Console.WriteLine("3. Blue");
                        Console.WriteLine("4. Yellow");
                        Console.Write("Your choice (1-4): ");
                        var colorChoice = Console.ReadLine();

                        switch (colorChoice)
                        {
                            case "1":
                                currentTextColor = ConsoleColor.White;
                                break;
                            case "2":
                                currentTextColor = ConsoleColor.Green;
                                break;
                            case "3":
                                currentTextColor = ConsoleColor.Blue;
                                break;
                            case "4":
                                currentTextColor = ConsoleColor.Yellow;
                                break;
                            default:
                                Console.WriteLine("Invalid color choice.");
                                PlaySound(SoundEffect.Error);
                                break;
                        }

                        Console.ForegroundColor = currentTextColor;
                        Console.WriteLine($"Text color changed to: {currentTextColor}");
                        break;
                    case "10":
                        if (ShowAccountSettings(
                                ref loggedInUsername,
                                ref loggedInPassword,
                                ref userFiles,
                                PlaySound))
                        {
                            try
                            {
                                Files.BindEncryptionSession(
                                    loggedInUsername,
                                    loggedInPassword,
                                    userFiles.DataFilePath);
                            }
                            catch (InvalidOperationException ex)
                            {
                                Console.WriteLine(ex.Message);
                                PlaySound(SoundEffect.Error);
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("-> Invalid option.");
                        PlaySound(SoundEffect.Error);
                        break;
                    
                }

                Aestetics.WaitForEnter();
            }
        }

        private sealed record LoginResult(string Username, string Password);

        private static bool ShowAccountSettings(
            ref string loggedInUsername,
            ref string loggedInPassword,
            ref UserFilePaths userFiles,
            Action<SoundEffect> playSound)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== Account settings ===");
                Console.WriteLine("1. Change username");
                Console.WriteLine("2. Change password");
                Console.WriteLine("3. Back");
                Console.Write("Choose option (1-3): ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write($"Current username: {loggedInUsername}");
                        Console.WriteLine();
                        Console.Write("New username: ");
                        var newUsername = Console.ReadLine() ?? string.Empty;
                        Console.Write("Password (to confirm): ");
                        var confirmPassword = ConsoleInput.ReadMaskedLine();

                        if (ProfileService.ChangeUsername(
                                loggedInUsername,
                                confirmPassword,
                                newUsername,
                                out var changeMessage))
                        {
                            loggedInUsername = newUsername.Trim();
                            userFiles = new UserFilePaths(loggedInUsername);
                            Console.WriteLine(changeMessage);
                            playSound(SoundEffect.Success);
                            return true;
                        }

                        Console.WriteLine(changeMessage);
                        playSound(SoundEffect.Error);
                        Aestetics.WaitForEnter();
                        break;
                    case "2":
                        Console.Write("Current password: ");
                        var currentPassword = ConsoleInput.ReadMaskedLine();
                        Console.Write("New password: ");
                        var newPassword = ConsoleInput.ReadMaskedLine();
                        Console.Write("Repeat new password: ");
                        var repeatNewPassword = ConsoleInput.ReadMaskedLine();

                        if (ProfileService.ChangePassword(
                                loggedInUsername,
                                currentPassword,
                                newPassword,
                                repeatNewPassword,
                                out var passwordMessage))
                        {
                            loggedInPassword = newPassword;
                            Console.WriteLine(passwordMessage);
                            playSound(SoundEffect.Success);
                            return true;
                        }

                        Console.WriteLine(passwordMessage);
                        playSound(SoundEffect.Error);
                        Aestetics.WaitForEnter();
                        break;
                    case "3":
                        return false;
                    default:
                        Console.WriteLine("Invalid option.");
                        playSound(SoundEffect.Error);
                        Aestetics.WaitForEnter();
                        break;
                }
            }
        }

        private static LoginResult? ShowAuthScreen(AsyncSoundPlayer soundPlayer)
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
                        var loginPassword = ConsoleInput.ReadMaskedLine();

                        if (AuthService.Login(loginUsername, loginPassword, out var loginMessage, out var loggedInUsername)
                            && loggedInUsername is not null)
                        {
                            Console.WriteLine(loginMessage);
                            soundPlayer.Play(SoundEffect.Success);
                            Aestetics.WaitForEnter();
                            return new LoginResult(loggedInUsername, loginPassword);
                        }

                        Console.WriteLine(loginMessage);
                        soundPlayer.Play(SoundEffect.Error);
                        Aestetics.WaitForEnter();
                        break;
                    case "2":
                        Console.Write("Username: ");
                        var registerUsername = Console.ReadLine() ?? string.Empty;
                        Console.Write("Password: ");
                        var registerPassword = ConsoleInput.ReadMaskedLine();
                        Console.Write("Repeat password: ");
                        var repeatPassword = ConsoleInput.ReadMaskedLine();

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
    }
}