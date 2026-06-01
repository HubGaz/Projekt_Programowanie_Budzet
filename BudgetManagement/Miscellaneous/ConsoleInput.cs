using System.Text;

namespace BudgetManagement.Miscellaneous;

public static class ConsoleInput
{
    private static bool? _supportsMaskedInput;
    private static bool _fallbackNoticeShown;

    public static string ReadMaskedLine(char mask = '*')
    {
        if (!SupportsMaskedInput())
        {
            return Console.ReadLine() ?? string.Empty;
        }

        return ReadMaskedLineCore(mask);
    }

    private static bool SupportsMaskedInput()
    {
        if (_supportsMaskedInput.HasValue)
        {
            return _supportsMaskedInput.Value;
        }

        try
        {
            _supportsMaskedInput = !Console.IsInputRedirected;
        }
        catch
        {
            _supportsMaskedInput = false;
        }

        return _supportsMaskedInput.Value;
    }

    private static void DisableMaskedInput()
    {
        _supportsMaskedInput = false;
    }

    private static void ShowFallbackNotice()
    {
        if (_fallbackNoticeShown)
        {
            return;
        }

        _fallbackNoticeShown = true;
        Console.WriteLine("(Masked input unavailable — password will be visible.)");
    }

    private static string ReadMaskedLineCore(char mask)
    {
        var buffer = new StringBuilder();

        while (true)
        {
            ConsoleKeyInfo key;
            try
            {
                key = Console.ReadKey(intercept: true);
            }
            catch (InvalidOperationException)
            {
                DisableMaskedInput();
                ShowFallbackNotice();
                Console.WriteLine();

                if (buffer.Length == 0)
                {
                    return Console.ReadLine() ?? string.Empty;
                }

                var remainder = Console.ReadLine() ?? string.Empty;
                return buffer.ToString() + remainder;
            }

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return buffer.ToString();
                case ConsoleKey.Backspace:
                    if (buffer.Length > 0)
                    {
                        buffer.Length--;
                        Console.Write("\b \b");
                    }
                    break;
                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        buffer.Append(key.KeyChar);
                        Console.Write(mask);
                    }
                    break;
            }
        }
    }
}
