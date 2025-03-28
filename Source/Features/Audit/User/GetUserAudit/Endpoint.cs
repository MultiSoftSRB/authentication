using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Contracts.Common;
using MultiSoftSRB.Database.Audit;
using MultiSoftSRB.Entities.Audit;

namespace MultiSoftSRB.Features.Audit.User.GetUserAudit;

sealed class Response { }

sealed class Endpoint : EndpointWithoutRequest<List<AuditLogResponse>>
{
    public MainAuditDbContext MainAuditDbContext { get; set; }
    public UserProvider UserProvider { get; set; }
    
    public override void Configure()
    {
        Get("audit/users/{id}");
        Permissions(ResourcePermissions.AuditRead);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var userId = Route<long>("id");
        var companyId = UserProvider.GetCurrentCompanyId();

        if (companyId != null && !await MainAuditDbContext.UserCompanies.AnyAsync(
                uc => uc.CompanyId == companyId && uc.UserId == userId,
                cancellationToken: cancellationToken))
        {
            AddError("User is not part of this company.");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        var auditLogs = await MainAuditDbContext.Users
            .Where(x => x.EntityId == userId.ToString())
            .OrderByDescending(x => x.Timestamp)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = auditLogs.Select(
            x => new AuditLogResponse
            {
                Id = x.Id,
                EntityId = x.EntityId,
                ActionType = x.ActionType,
                Timestamp = x.Timestamp,
                CompanyId = x.CompanyId,
                UserId = x.UserId,
                UserName = x.UserName,
                ApiKeyId = x.ApiKeyId,
                Endpoint = x.EntityId,
                ChangedProperties = x.ChangedProperties == null ? [] : JsonSerializer.Deserialize<List<PropertyChange>>(x.ChangedProperties)
            }).ToList();
        
        await SendOkAsync(response, cancellationToken);
    }
}