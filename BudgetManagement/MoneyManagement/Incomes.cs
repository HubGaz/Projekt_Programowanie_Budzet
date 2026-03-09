namespace BudgetManagement.MoneyManagement;

public static class Incomes
{
    public static double Current_Balance = 0.0;
    public static double Total_Incomes = 0.0;
    public static void AddIncome(double amount)
    {
        if (amount < 0)
        {
            amount = Math.Abs(amount);
        }
        Total_Incomes += amount;
        Current_Balance += amount;
    }
}

