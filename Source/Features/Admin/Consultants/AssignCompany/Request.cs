namespace MultiSoftSRB.Features.Admin.Consultants.AssignCompany;

sealed class Request
{
    public long ConsultantUserId { get; set; }
    public long CompanyId { get; set; }
}