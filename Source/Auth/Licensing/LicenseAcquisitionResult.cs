namespace MultiSoftSRB.Auth.Licensing;

public class LicenseAcquisitionResult
{
    public bool Success { get; set; }
    public string SessionId { get; set; } = string.Empty;
}