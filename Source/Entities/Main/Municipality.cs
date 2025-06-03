namespace MultiSoftSRB.Entities.Main;

public class Municipality
{
    public int Id { get; set; }
    public int RegionId { get; set; }
    public string Name { get; set; } = null!;
    public string? MunicipalityCode { get; set; }
    public bool IsActive { get; set; } 
    
    public Region Region { get; set; }
    
    public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
}