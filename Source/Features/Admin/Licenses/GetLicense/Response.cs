namespace MultiSoftSRB.Features.Admin.Licenses.GetLicense;

sealed class Response
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public List<string> Features { get; set; } = [];
}