namespace MultiSoftSRB.Entities.Main;

public class Region 
{
    public int Id { get; set; }
    public short CountryId { get; set; }
    
    public Country Country { get; set; }
    
    public required string Name { get; set; }
    
    public string? Code { get; set; }
    
    public string? Type { get; set; }
    
    public float? Latitude { get; set; }
    
    public float? Longitude { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    
    public ICollection<Municipality> Municipalities { get; set; } = new List<Municipality>();
    
}