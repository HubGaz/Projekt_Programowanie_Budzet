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
                Console.Write("Choose option (1-4): ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1": Console.WriteLine("-> Adding income..."); break;
                    case "2": Console.WriteLine("-> Adding expense..."); break;
                    case "3": Console.WriteLine("-> Showing balance..."); break;
                    case "4":
                        Console.WriteLine("-> Goodbye!");
                        return;
                    default: Console.WriteLine("-> Invalid option."); break;
                    //Zmiana z brancha 
                    //Zmiana 2 z brancha
                }
            }
        }

    }
}