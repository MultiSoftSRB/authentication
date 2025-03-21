namespace MultiSoftSRB.Auth.Permissions;

public class CustomPolicies
{
    public const string SuperAdminOnly = nameof(SuperAdminOnly);
    public const string AdminsOnly = nameof(AdminsOnly);
    public const string ConsultantAndAbove = nameof(ConsultantAndAbove);
}