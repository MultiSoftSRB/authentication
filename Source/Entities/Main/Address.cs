namespace MultiSoftSRB.Entities.Main;

public class Address
{
    public int Id { get; set; }
    public int SettlementId { get; set; }
    public Settlement Settlement { get; set; }
    public required string Street { get; set; }
    public string StreetNumber { get; set; }
    public string Apartment { get; set; }
    public string Description { get; set; }
    public bool IsPrimary { get; set; } = true;
    
    
}