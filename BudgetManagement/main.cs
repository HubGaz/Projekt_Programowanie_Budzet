
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
            var isSoundEnabled = true;
            var currentTextColor = ConsoleColor.White;

            var exitApplication = false;
            while (!exitApplication)
            {
                var loginResult = ShowAuthScreen(soundPlayer);
                if (loginResult is null)
                {
                    return;
                }

                if (!TryStartUserSession(
                        loginResult,
                        soundPlayer,
                        ref exitApplication,
                        ref currentTextColor,
                        ref isSoundEnabled))
                {
                    continue;
                }
            }

            return;
        }

        private static bool TryStartUserSession(
            LoginResult loginResult,
            AsyncSoundPlayer soundPlayer,
            ref bool exitApplication,
            ref ConsoleColor sessionTextColor,
            ref bool sessionSoundEnabled)
        {
            var loggedInUsername = loginResult.Username;
            var loggedInPassword = loginResult.Password;
            var loggedInAlias = loginResult.DisplayAlias;
            var userFiles = new UserFilePaths(loggedInUsername);

            var soundsOn = sessionSoundEnabled;
            void PlaySound(SoundEffect effect)
            {
                if (soundsOn)
                {
                    soundPlayer.Play(effect);
                }
            }

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
                return false;
            }

            while (true)
            {
                Incomes.Total_Incomes = Files.ReadTotalIncome(userFiles.DataFilePath);
                Expenses.Total_Expenses = Files.ReadTotalExpense(userFiles.DataFilePath);

                Aestetics.ClearAndShowLogo(sessionTextColor);
                Console.WriteLine($"Logged in as: {ProfileService.FormatLoginLabel(loggedInUsername, loggedInAlias)}");

                Console.WriteLine("");
                Console.WriteLine("=== Menu ===");
                Console.WriteLine("1. Add income");
                Console.WriteLine("2. Add expense");
                Console.WriteLine("3. View balance");
                Console.WriteLine("4. Check expense history");
                Console.WriteLine("5. Check income history");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("6. Clear all entries");
                Console.ForegroundColor = sessionTextColor;
                Console.WriteLine("7. Exit");
                Console.WriteLine($"8. Toggle sounds ({(sessionSoundEnabled ? "ON" : "OFF")})");
                Console.WriteLine($"9. Change text color (current: {sessionTextColor})");
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
                        exitApplication = true;
                        return false;
                    case "8":
                        sessionSoundEnabled = !sessionSoundEnabled;
                        soundsOn = sessionSoundEnabled;
                        Console.WriteLine($"Sounds are now {(sessionSoundEnabled ? "ON" : "OFF")}.");
                        break;
                    case "9":
                        ShowTextColorMenu(ref sessionTextColor, PlaySound);
                        break;
                    case "10":
                        var settingsOutcome = ShowAccountSettings(
                            ref loggedInUsername,
                            ref loggedInPassword,
                            ref loggedInAlias,
                            ref userFiles,
                            sessionTextColor,
                            PlaySound);

                        switch (settingsOutcome)
                        {
                            case AccountSettingsOutcome.SessionEnded:
                                Console.WriteLine("Returning to login screen.");
                                PlaySound(SoundEffect.Info);
                                return false;
                            case AccountSettingsOutcome.CredentialsChanged:
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
                                break;
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

        private sealed record LoginResult(string Username, string Password, string? DisplayAlias);

        private static void ShowTextColorMenu(ref ConsoleColor sessionTextColor, Action<SoundEffect> playSound)
        {
            while (true)
            {
                Aestetics.ClearAndShowLogo(sessionTextColor);
                Console.WriteLine();
                Console.WriteLine("=== Text color ===");
                Console.WriteLine($"Current: {sessionTextColor}");
                Console.WriteLine("1. White");
                Console.WriteLine("2. Green");
                Console.WriteLine("3. Blue");
                Console.WriteLine("4. Yellow");
                Console.WriteLine("5. Back");
                Console.Write("Choose option (1-5): ");
                var colorChoice = Console.ReadLine();

                switch (colorChoice)
                {
                    case "1":
                        sessionTextColor = ConsoleColor.White;
                        Console.WriteLine($"Text color changed to: {sessionTextColor}");
                        playSound(SoundEffect.Success);
                        break;
                    case "2":
                        sessionTextColor = ConsoleColor.Green;
                        Console.WriteLine($"Text color changed to: {sessionTextColor}");
                        playSound(SoundEffect.Success);
                        break;
                    case "3":
                        sessionTextColor = ConsoleColor.Blue;
                        Console.WriteLine($"Text color changed to: {sessionTextColor}");
                        playSound(SoundEffect.Success);
                        break;
                    case "4":
                        sessionTextColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Text color changed to: {sessionTextColor}");
                        playSound(SoundEffect.Success);
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid color choice.");
                        playSound(SoundEffect.Error);
                        Aestetics.WaitForEnter();
                        break;
                }
            }
        }

        private static AccountSettingsOutcome ShowAccountSettings(
            ref string loggedInUsername,
            ref string loggedInPassword,
            ref string? loggedInAlias,
            ref UserFilePaths userFiles,
            ConsoleColor sessionTextColor,
            Action<SoundEffect> playSound)
        {
            while (true)
            {
                Aestetics.ClearAndShowLogo(sessionTextColor);
                Console.WriteLine();
                Console.WriteLine("=== Account settings ===");
                Console.WriteLine($"Login: {loggedInUsername}");
                Console.WriteLine($"Alias: {(string.IsNullOrWhiteSpace(loggedInAlias) ? "(none)" : loggedInAlias)}");
                Console.WriteLine("1. Change username");
                Console.WriteLine("2. Change password");
                Console.WriteLine("3. Change alias");
                Console.WriteLine("4. Suspend account");
                Console.WriteLine("5. Delete account");
                Console.WriteLine("6. Back");
                Console.Write("Choose option (1-6): ");
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
                            return AccountSettingsOutcome.CredentialsChanged;
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
                            return AccountSettingsOutcome.CredentialsChanged;
                        }

                        Console.WriteLine(passwordMessage);
                        playSound(SoundEffect.Error);
                        Aestetics.WaitForEnter();
                        break;
                    case "3":
                        Console.Write("New alias (leave empty to remove): ");
                        var newAlias = Console.ReadLine();
                        Console.Write("Password (to confirm): ");
                        var aliasPassword = ConsoleInput.ReadMaskedLine();

                        if (ProfileService.SetAlias(
                                loggedInUsername,
                                aliasPassword,
                                newAlias,
                                out var aliasMessage))
                        {
                            loggedInAlias = string.IsNullOrWhiteSpace(newAlias) ? null : newAlias.Trim();
                            Console.WriteLine(aliasMessage);
                            playSound(SoundEffect.Success);
                        }
                        else
                        {
                            Console.WriteLine(aliasMessage);
                            playSound(SoundEffect.Error);
                            Aestetics.WaitForEnter();
                        }
                        break;
                    case "4":
                        Console.Write("Reason for suspension (optional): ");
                        var suspensionReason = Console.ReadLine();
                        Console.Write("Password (to confirm): ");
                        var suspendPassword = ConsoleInput.ReadMaskedLine();

                        if (ProfileService.SuspendAccount(
                                loggedInUsername,
                                suspendPassword,
                                suspensionReason,
                                out var suspendMessage))
                        {
                            Console.WriteLine(suspendMessage);
                            playSound(SoundEffect.Warning);
                            return AccountSettingsOutcome.SessionEnded;
                        }

                        Console.WriteLine(suspendMessage);
                        playSound(SoundEffect.Error);
                        Aestetics.WaitForEnter();
                        break;
                    case "5":
                        Console.WriteLine("This permanently deletes your account and all budget data.");
                        Console.Write("Type DELETE to confirm: ");
                        var deleteConfirmation = Console.ReadLine();
                        if (!string.Equals(deleteConfirmation, "DELETE", StringComparison.Ordinal))
                        {
                            Console.WriteLine("Deletion cancelled.");
                            playSound(SoundEffect.Info);
                            Aestetics.WaitForEnter();
                            break;
                        }

                        Console.Write("Password (to confirm): ");
                        var deletePassword = ConsoleInput.ReadMaskedLine();

                        if (ProfileService.DeleteAccount(
                                loggedInUsername,
                                deletePassword,
                                out var deleteMessage))
                        {
                            Console.WriteLine(deleteMessage);
                            playSound(SoundEffect.Warning);
                            return AccountSettingsOutcome.SessionEnded;
                        }

                        Console.WriteLine(deleteMessage);
                        playSound(SoundEffect.Error);
                        Aestetics.WaitForEnter();
                        break;
                    case "6":
                        return AccountSettingsOutcome.Continue;
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
                Aestetics.ClearAndShowLogo();
                Console.WriteLine();
                Console.WriteLine("=== Login ===");
                Console.WriteLine("1. Sign in");
                Console.WriteLine("2. Create account");
                Console.WriteLine("3. Reactivate suspended account");
                Console.WriteLine("4. Exit");
                Console.Write("Choose option (1-4): ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Username: ");
                        var loginUsername = Console.ReadLine() ?? string.Empty;
                        Console.Write("Password: ");
                        var loginPassword = ConsoleInput.ReadMaskedLine();

                        if (AuthService.Login(
                                loginUsername,
                                loginPassword,
                                out var loginMessage,
                                out var loggedInUsername,
                                out var displayAlias)
                            && loggedInUsername is not null)
                        {
                            Console.WriteLine(loginMessage);
                            soundPlayer.Play(SoundEffect.Success);
                            Aestetics.WaitForEnter();
                            return new LoginResult(loggedInUsername, loginPassword, displayAlias);
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
                        Console.Write("Username: ");
                        var reactivateUsername = Console.ReadLine() ?? string.Empty;
                        Console.Write("Password: ");
                        var reactivatePassword = ConsoleInput.ReadMaskedLine();

                        if (AuthService.ReactivateAccount(
                                reactivateUsername,
                                reactivatePassword,
                                out var reactivateMessage))
                        {
                            Console.WriteLine(reactivateMessage);
                            soundPlayer.Play(SoundEffect.Success);
                        }
                        else
                        {
                            Console.WriteLine(reactivateMessage);
                            soundPlayer.Play(SoundEffect.Error);
                        }

                        Aestetics.WaitForEnter();
                        break;
                    case "4":
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