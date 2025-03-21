using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Admin.Companies.GetCompanies;

sealed class Response
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DatabaseType DatabaseType { get; set; }
}