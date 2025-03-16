using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Entities.Company;

public class Article : CompanyEntity
{
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public decimal Price { get; set; }
}