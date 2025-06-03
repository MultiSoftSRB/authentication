namespace MultiSoftSRB.Features.Admin.Consultants.AssignCompany;

sealed class Request
{
    public long ConsultantUserId { get; set; }
    public short CompanyId { get; set; }
}