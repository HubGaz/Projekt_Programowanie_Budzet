
namespace main
{
    public static class Expenses
    {
        public static double Current_Balance = 0.0;
        public static double Total_Expenses = 0.0;

        public static void AddExpense(double amount)
        {
            if (amount < 0)
            {
                amount = Math.Abs(amount);
            }

            Total_Expenses += amount;
            Current_Balance -= amount;
        }
    }
}
