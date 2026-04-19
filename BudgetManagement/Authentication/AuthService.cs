namespace BudgetManagement.Authentication;

public static class AuthService
{
    public static bool Register(string username, string password, string confirmPassword, out string message)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            message = "Nazwa uzytkownika nie moze byc pusta.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            message = "Haslo nie moze byc puste.";
            return false;
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            message = "Hasla nie sa takie same.";
            return false;
        }

        var users = UserStore.LoadUsers();
        if (users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
        {
            message = "Uzytkownik o tej nazwie juz istnieje.";
            return false;
        }

        var salt = PasswordHasher.CreateSalt();
        users.Add(new UserAccount
        {
            Username = username.Trim(),
            Salt = salt,
            PasswordHash = PasswordHasher.Hash(password, salt)
        });

        UserStore.SaveUsers(users);
        message = "Konto zostalo utworzone.";
        return true;
    }

    public static bool Login(string username, string password, out string message)
    {
        var users = UserStore.LoadUsers();
        var user = users.FirstOrDefault(u =>
            string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            message = "Nie znaleziono uzytkownika.";
            return false;
        }

        var hash = PasswordHasher.Hash(password, user.Salt);
        if (!string.Equals(hash, user.PasswordHash, StringComparison.Ordinal))
        {
            message = "Niepoprawne haslo.";
            return false;
        }

        message = "Zalogowano poprawnie.";
        return true;
    }
}
