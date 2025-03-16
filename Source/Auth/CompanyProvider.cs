using Microsoft.Extensions.Options;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Auth;

public class CompanyProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CompanyConnectionStrings _connectionStrings;

    public CompanyProvider(
        IHttpContextAccessor httpContextAccessor,
        IOptions<CompanyConnectionStrings> connectionStrings)
    {
        _httpContextAccessor = httpContextAccessor;
        _connectionStrings = connectionStrings.Value;
    }

    public long GetCompanyId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var companyClaim = user?.FindFirst(CustomClaimTypes.CompanyId)?.Value;

        if (string.IsNullOrEmpty(companyClaim) || !long.TryParse(companyClaim, out long companyId))
        {
            throw new ApplicationException("Company ID is not present in the token.");
        }

        return companyId;
    }
    
    public DatabaseType GetDatabaseType()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var databaseClaim = user?.FindFirst(CustomClaimTypes.DatabaseType)?.Value;

        if (string.IsNullOrEmpty(databaseClaim) || !Enum.TryParse(databaseClaim, out DatabaseType databaseType))
        {
            throw new ApplicationException("Database information is missing in the token.");
        }

        return databaseType;
    }

    public string GetConnectionString()
    {
        var database = GetDatabaseType();
        if (!_connectionStrings.Values.TryGetValue((int)database, out string? connectionString))
        {
            throw new ApplicationException("Invalid database selection.");
        }

        return connectionString;
    }
    
    public string GetFirstConnectionStringForMigration()
    {
        return _connectionStrings.Values.First().Value;
    }
}