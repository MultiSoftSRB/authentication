namespace MultiSoftSRB.Features.Auth.SwitchCompany;

sealed class Response
{
    public string AccessToken { get; set; } = null!;
    public string[] PagePermissions { get; set; } = [];
}