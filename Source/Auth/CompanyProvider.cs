using Microsoft.Extensions.Options;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Auth;

public class CompanyProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CompanyConnectionStrings _connectionStrings;

    public CompanyProvider(
        IHttpContextAccessor httpContextAccessor)
       // IOptions<CompanyConnectionStrings> connectionStrings)
    {
        _httpContextAccessor = httpContextAccessor;
       // _connectionStrings = connectionStrings.Value;
    }

    public short GetCompanyId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var companyClaim = user?.FindFirst(CustomClaimTypes.CompanyId)?.Value;

        if (string.IsNullOrEmpty(companyClaim) || !short.TryParse(companyClaim, out short companyId))
        {
            ValidationContext.Instance.ThrowError("Company ID is not present in the token.", StatusCodes.Status401Unauthorized);
            return default;
        }

        return companyId;
    }
    
    /*public DatabaseType GetDatabaseType()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var databaseClaim = user?.FindFirst(CustomClaimTypes.DatabaseType)?.Value;

        if (string.IsNullOrEmpty(databaseClaim) || !Enum.TryParse(databaseClaim, out DatabaseType databaseType))
        {
            ValidationContext.Instance.ThrowError("Database information is missing in the token.", StatusCodes.Status401Unauthorized);
            return default;
        }

        return databaseType;
    }

    public string GetConnectionString()
    {
        var database = GetDatabaseType();
        if (!_connectionStrings.Values.TryGetValue((int)database, out string? connectionString))
        {
            ValidationContext.Instance.ThrowError("Invalid database selection.", StatusCodes.Status401Unauthorized);
            return default;
        }

        return connectionString;
    }
    
    public string GetFirstConnectionStringForMigration()
    {
        return _connectionStrings.Values.First().Value;
    }*/
}