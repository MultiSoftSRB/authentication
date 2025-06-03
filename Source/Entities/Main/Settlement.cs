namespace MultiSoftSRB.Entities.Main;

public class Settlement
{
    public int Id { get; set; }

    public int MunicipalityId { get; set; }
    
    
    public Municipality? Municipality { get; set; }
    public required string Name { get; set; }
    public string? PostalCode { get; set; }
    public float? Latitude { get; set; }
    public float? Longitude { get; set; }
    public int? Elevation { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Address> Addresses { get; set; } = new List<Address>();

}