namespace MultiSoftSRB.Entities.Main;

public class Country
{
    public short Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string IsoCode { get; set; } = string.Empty;

    public string? Iso3Code { get; set; }

    public short? NumericCode { get; set; }

    public string? PhoneCode { get; set; }

    public string? Capital { get; set; }

    public string? Currency { get; set; }

    public string? CurrencySymbol { get; set; }

    public string? Tld { get; set; }

    public string? NativeName { get; set; }

    public float? Latitude { get; set; }

    public float? Longitude { get; set; }

    public bool IsActive { get; set; } = true;
    
    public ICollection<Region> Regions { get; set; }
}