namespace MultiSoftSRB.Entities.Main;

public class LicenseFeature
{
    public long LicenseId { get; set; }
    public string FeaturePermissionCode { get; set; } = null!;
        
    // Navigation property
    public virtual License License { get; set; } = null!;
}