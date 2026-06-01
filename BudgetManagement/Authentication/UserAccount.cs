namespace BudgetManagement.Authentication;

public sealed class UserAccount
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;

    /// <summary>Optional display name shown in the UI instead of the login username.</summary>
    public string? Alias { get; set; }

    public bool IsSuspended { get; set; }
    public DateTimeOffset? SuspendedAt { get; set; }
    public string? SuspensionReason { get; set; }

    public string GetDisplayName() =>
        string.IsNullOrWhiteSpace(Alias) ? Username : Alias.Trim();
}
