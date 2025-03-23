namespace MultiSoftSRB.Features.Admin.Consultants.GetConsultant;

sealed class Response
{
    public long Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public List<ConsultantCompanyDto> Companies { get; set; } = [];
    
    internal sealed class ConsultantCompanyDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}