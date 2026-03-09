
using System.Collections.Generic;

namespace main
{
    public static class Expenses
    {
        public static double Current_Balance = 0.0;
        public static double Total_Expenses = 0.0;

        public static List<(double Amount, string Description)> ExpenseList { get; } = new();

        public static void AddExpense(double amount, string description)
        {
            if (amount < 0)
            {
                amount = Math.Abs(amount);
            }

            Total_Expenses += amount;
            Current_Balance -= amount;

            ExpenseList.Add((amount, description));
        }
    }
}
