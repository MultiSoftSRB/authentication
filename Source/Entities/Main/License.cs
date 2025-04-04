namespace MultiSoftSRB.Entities.Main;

public class License : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; } = 0;
    
    public virtual ICollection<LicenseFeature> Features { get; set; } = new List<LicenseFeature>();
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
}