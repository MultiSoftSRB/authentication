# MultiSoftSRB

---

## Table of Contents

- [Project Structure](#project-structure)
- [Technologies Used](#technologies-used)
- [Key Features](#key-features)
- [Getting Started](#getting-started)
- [Database and Multi-Tenancy Architecture](#database-and-multi-tenancy-architecture)
- [Audit System](#audit-system)
- [Authentication and Authorization](#authentication-and-authorization)
- [API Keys](#api-keys)
- [FastEndpoints Implementation](#fastendpoints-implementation)
- [Docker Support](#docker-support)
- [CI/CD with GitHub Actions and Azure](#cicd-with-github-actions-and-azure)
- [Code Quality Guidelines](#code-quality-guidelines)

---

## Project Structure

```
Source/
├── Auth/                  # Authentication and authorization components
│   ├── ApiKey/            # API key authentication implementation
│   └── Permissions/       # Permission definitions and mapping
│
├── Audit/                 # Audit logging system
│
├── Contracts/             # Shared contract definitions
│
├── Database/              # Database contexts and configurations
│   ├── Audit/             # Audit database contexts
│   ├── Company/           # Company-specific database context
│   └── Main/              # Main system database context and migrations
│       └── Configurations/  # Entity configurations
│
├── Entities/              # Domain entities
│   ├── Audit/             # Audit log entity definitions
│   ├── Company/           # Company-specific entities
│   └── Main/              # Core system entities
│
├── Extensions/            # Extension methods and helpers
│
├── Features/              # Features implementations, in other words - endpoints
│
├── Services/              # Services for business logic that repets itself and needs reusability
│
├── appsettings.json       # Application configuration
├── Dockerfile             # Container definition
└── Program.cs             # Application entry point and configuration
```

---

## Technologies Used

- **.NET 9**
- **FastEndpoints**
- **Entity Framework Core**
- **PostgreSQL** (via EF Core)
- **JWT + Refresh Tokens**
- **API Key Authentication**
- **Multi-Tenancy**
- **Docker**

---

## Key Features

- **Multi-Tenant Architecture**: All tenants/companies share the same schema, filtered via `company_id`.
- **Main Database**: Stores user accounts, companies, roles, permissions, and such.
- **Default Roles**: Each company registration triggers creation of default roles (e.g. Admin, Member) via `RolesService`.
- **Permissions System**:
    - Resource-level permissions
    - Page-to-resource mapping
    - Claim hydration for permission resolution
- **Authentication**:
    - JWT with refresh token support
    - API key support for system-level integrations
- **Auditing**: Logs actions via a separate audit db contexts

---


## Database and Multi-Tenancy Architecture

### Database Context Overview

The application uses multiple database contexts, each with a specific purpose:

1. **MainDbContext**: Central system database for authentication, authorization, and tenant management
2. **CompanyDbContext**: Tenant-specific business data with dynamic connection resolution
3. **CompanyMigrationDbContext**: Special context used only for migrations across all tenant databases
4. **MainAuditDbContext**: Audit logging database that reflects structure of main database with AuditLog entity
5. **CompanyAuditDbContext**: Audit logging database that reflects structure of company database with AuditLog entity

### Database Architecture Diagram

```
┌─────────────────────┐    ┌─────────────────────┐    ┌─────────────────────┐
│                     │    │                     │    │                     │
│    MainDatabase     │    │  MainAuditDatabase  │    │ CompanyAuditDatabase│
│                     │    │                     │    │                     │
│  - Users            │    │  - UserAuditLogs    │    │  - ArticleAuditLogs │
│  - Companies        │    │  - CompanyAuditLogs │    │  - Other company    │
│  - Roles            │    │  - RoleAuditLogs    │    │    entity audit     │
│  - Permissions      │    │  - Other main       │    │    logs             │
│  - API Keys         │    │    entity audit     │    │                     │
│                     │    │    logs             │    │                     │
└─────────────────────┘    └─────────────────────┘    └─────────────────────┘
          │                           │                          │
          │                           │                          │
          v                           v                          v
┌───────────────────────────────────────────────────────────────────────────┐
│                                                                           │
│                                 Application                               │
│                                                                           │
└───────────────────────────────────────────────────────────────────────────┘
          │                           │                          │
          │                           │                          │
          v                           v                          v
┌───────────────────────┐ ┌───────────────────────┐ ┌───────────────────────┐
│                       │ │                       │ │                       │
│     Company DB #1     │ │      Company DB       │ │       Company DB      │
│     (Finance)         │ │      #2 (Retail)      │ │       #3 (Mfg)        │
│                       │ │                       │ │                       │
└───────────────────────┘ └───────────────────────┘ └───────────────────────┘
```

### MainDbContext

The `MainDbContext` is the central database for system-wide data:

```csharp
public class MainDbContext : DbContext
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserCompany> UserCompanies { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    // ... other central entities
}
```

Key characteristics:
- Single database instance for all users
- Uses a static connection string from configuration
- Contains all identity and permission data
- Configured with Identity framework for user management
- Each entity has its configuration in a separate file

### CompanyDbContext

The `CompanyDbContext` handles tenant-specific business data with dynamic connection resolution:

```csharp
public class CompanyDbContext : DbContext
{
    private readonly CompanyProvider _companyProvider;
    private readonly long _companyId;

    // Constructor injects CompanyProvider for connection resolution
    public CompanyDbContext(
        DbContextOptions<CompanyDbContext> options, 
        IServiceProvider serviceProvider,
        CompanyProvider companyProvider) 
        : base(options)
    {
        _companyProvider = companyProvider;
        _companyId = companyProvider.GetCompanyId();
    }

    public DbSet<Article> Articles { get; set; }
    // ... other business entities

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Dynamic connection string based on the current user's company
        optionsBuilder.UseNpgsql(_companyProvider.GetConnectionString());
        
        // Add audit interceptor
        optionsBuilder.UseAudit(
            _serviceProvider, "CompanyDb", _companyId, 
            (int)_companyProvider.GetDatabaseType()
         );
    }
}
```

Key characteristics:
- **Dynamic Connection Resolution**: Uses `CompanyProvider` to determine which connection string to use based on the authenticated user's context
- **Multi-Tenancy via Query Filters**: Automatically filters all data by `CompanyId` using global query filters
- **Automatic Data Isolation**: All entities inherit from `CompanyEntity` which includes a `CompanyId` property
- **Transparent to Business Logic**: Services work with the context normally; the multi-tenancy happens behind the scenes

### How Tenant Resolution Works

1. When a user logs in, their JWT token includes:
   - `UserID`
   - `CompanyId`
   - `DatabaseType` (determines which database to connect to)

2. The `CompanyProvider` extracts these claims from the current HTTP context:
   ```csharp
   public DatabaseType GetDatabaseType()
   {
       var databaseClaim = httpContext.User.FindFirst(CustomClaimTypes.DatabaseType)?.Value;
       if (string.IsNullOrEmpty(databaseClaim) || !Enum.TryParse(databaseClaim, out DatabaseType databaseType))
       {
           throw new UnauthorizedException("Database information is missing");
       }
       return databaseType;
   }
   ```

3. The provider maps the `DatabaseType` to a connection string from configuration:
   ```csharp
   public string GetConnectionString()
   {
       var database = GetDatabaseType();
       if (!_connectionStrings.Values.TryGetValue((int)database, out string connectionString))
       {
           throw new UnauthorizedException("Invalid database selection");
       }
       return connectionString;
   }
   ```

4. The `CompanyDbContext` uses this connection string when it's instantiated

### Entity Configurations

Entity configurations are separated into individual files per entity.

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.Property(e => e.UserName).HasMaxLength(256);
        builder.Property(e => e.NormalizedUserName).HasMaxLength(256);
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.NormalizedEmail).HasMaxLength(256);
    }
}
```

These configurations are automatically discovered and applied:

```csharp
// Apply configuration for all entities defined in specific namespace/folder
builder.ApplyConfigurationsFromAssembly(typeof(MainDbContext).Assembly,
    t => t.Namespace == "MultiSoftSRB.Database.Main.Configurations");
```

Benefits of this approach:
- Keeps entity configurations focused and maintainable
- Avoids large, unwieldy `OnModelCreating` methods
- Makes it easier to locate and modify specific entity configurations
- Provides clear separation between different database contexts

### Multi-Tenant Data Filtering

The `CompanyDbContext` automatically filters data by the current company using global query filters:

```csharp
// Apply global query filter for all entities inheriting CompanyEntity
foreach (var entityType in builder.Model.GetEntityTypes())
{
    if (typeof(CompanyEntity).IsAssignableFrom(entityType.ClrType))
    {
        var parameter = Expression.Parameter(entityType.ClrType, "e");
        var companyIdProperty = Expression.Property(parameter, nameof(CompanyEntity.CompanyId));
        var companyIdValue = Expression.Constant(_companyId);
        var predicate = Expression.Lambda(Expression.Equal(companyIdProperty, companyIdValue), parameter);

        builder.Entity(entityType.ClrType).HasQueryFilter(predicate);
    }
}
```

This ensures that:
- Users can only access data for their own company
- No explicit filtering is needed in queries or services
- Entities inherit from `CompanyEntity` to opt into this automatic filtering

### Database Migrations and Creation

```
✅ Migrations are executed for all contexts on app startup using `MigrationExtensions`. Running `dotnet ef database update` is optional for manual DB prep.
```

The application features automatic database migration on startup:


```csharp
// In Program.cs
app.ApplyMigrations();
```

The `MigrationExtensions.cs` handles applying migrations to all contexts:

```csharp
public static void ApplyMigrations(this IApplicationBuilder app)
{
    using var serviceScope = app.ApplicationServices.CreateScope();
    
    // Apply migrations to MainDbContext
    var mainContext = serviceScope.ServiceProvider.GetRequiredService<MainDbContext>();
    mainContext.Database.Migrate();
    
    // Apply migrations to AuditDbContext
    var auditContext = serviceScope.ServiceProvider.GetRequiredService<AuditDbContext>();
    auditContext.Database.Migrate();
    
    // Apply migrations to all company databases using CompanyMigrationDbContext
    var companyProvider = serviceScope.ServiceProvider.GetRequiredService<CompanyProvider>();
    var companyMigrations = serviceScope.ServiceProvider.GetRequiredService<CompanyMigrationDbContext>();
    
    foreach (var connectionString in companyProvider.GetAllConnectionStrings())
    {
        // Configure migration context to use this connection string
        companyMigrations.Database.SetConnectionString(connectionString);
        companyMigrations.Database.Migrate();
    }
}
```

This means:
1. **Automatic Database Creation**: If you add a new database to `appsettings.json`, it will be automatically created and migrated on startup
2. **Zero-Downtime Updates**: New tenants can be added without service interruption
3. **Configuration-Driven**: No code changes needed to add a new tenant database

Example configuration in `appsettings.json`:

```json
"CompanyConnectionStrings": {
  "Values": {
    "1": "Host=localhost;Database=xysoft_finance;",
    "2": "Host=localhost;Database=xysoft_retail;",
    "3": "Host=localhost;Database=xysoft_manufacturing;",
    "4": "Host=localhost;Database=xysoft_service;"
  }
}
```

Adding a new tenant is as simple as adding a new entry to this configuration.

### Adding New Entities

To add a new entity to the system:

1. **Main Database Entity**:
   - Create a new class in `Entities.Main` namespace
   - Create a corresponding configuration in `Database.Main.Configurations` folder
   - Add a `DbSet<YourEntity>` property to `MainDbContext`
   - Run `dotnet ef migrations add AddYourEntity --context MainDbContext --output-dir Database/Main/Migrations`

2. **Company Database Entity**:
   - Create a new class in `Entities.Company` namespace inheriting from `CompanyEntity`
   - Create a corresponding configuration in `Database.Company.Configurations` folder
   - Add a `DbSet<YourEntity>` property to `CompanyDbContext` and `CompanyMigrationDbContext`
   - Run `dotnet ef migrations add AddYourEntity --context CompanyMigrationDbContext --output-dir Database/Company/Migrations`

### DateTime Handling

All contexts handle DateTime values consistently, as required by the PostgreSQL:

```csharp
// DateTime configuration for PostgreSQL
foreach (var entityType in builder.Model.GetEntityTypes())
{
    foreach (var property in entityType.GetProperties())
    {
        if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
        {
            property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),      // Convert to UTC before saving
                v => DateTime.SpecifyKind(v, DateTimeKind.Local) // Convert to Local timezone when reading
            ));
        }
    }
}
```

This ensures that:
- All dates are stored as UTC in the database
- Dates are converted to local time when retrieved
- No timezone inconsistencies occur between different environments
---

## Audit System

The application implements an audit logging system that tracks changes to entities across both the main and company-specific databases. This system captures who made changes, what was changed, and when the changes occurred, providing a complete audit trail for compliance and troubleshooting purposes.

### Audit Architecture

The audit system is built around a multi-level architecture:

```
┌─────────────────────┐    ┌─────────────────────┐
│                     │    │                     │
│  MainDbContext      │    │  CompanyDbContext   │
│                     │    │                     │
└─────────────────────┘    └─────────────────────┘
          │                           │
          │                           │
          v                           v
┌─────────────────────┐    ┌─────────────────────┐
│                     │    │                     │
│AuditSaveChanges     │    │AuditSaveChanges     │
│Interceptor          │    │Interceptor          │
│                     │    │                     │
└─────────────────────┘    └─────────────────────┘
          │                           │
          │                           │
          v                           v
┌─────────────────────┐    ┌─────────────────────┐
│                     │    │                     │
│ MainAuditDbContext  │    │CompanyAuditDbContext│
│                     │    │                     │
└─────────────────────┘    └─────────────────────┘
```

### How the Audit System Works

1. **Change Interception**: The `AuditSaveChangesInterceptor` hooks into Entity Framework's save pipeline
2. **Change Analysis**: Modified entities are examined to determine what properties changed
3. **Audit Log Creation**: Specialized audit log entities are created for each changed entity
4. **Context Detection**: The system determines whether to use `MainAuditDbContext` or `CompanyAuditDbContext`
5. **Persistence**: Audit logs are saved in a separate database operation after the main transaction completes

### Audit Entity Design

The audit system uses Table-Per-Concrete-Type (TPC) inheritance to segregate audit logs by entity type:

```csharp
// Base abstract class for all audit logs
public abstract class AuditLog
{
    public int Id { get; set; }
    public AuditActionType ActionType { get; set; }
    public DateTime Timestamp { get; set; }
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public long? ApiKeyId { get; set; }
    public long? CompanyId { get; set; }
    public string EntityId { get; set; }
    public string Endpoint { get; set; }
    public string ChangedProperties { get; set; }
}

// Entity-specific audit logs inherit from AuditLog
public class UserAuditLog : AuditLog { }
public class RoleAuditLog : AuditLog { }
// ... other entity-specific audit logs
```

The system also maintains a fallback `DefaultAuditLog` for any entities that don't have a specific mapping.

### Audit Type Mapping

Entity types are mapped to their corresponding audit log types using a static dictionary:

```csharp
private static readonly Dictionary<Type, Type> EntityToAuditTypeMap = new()
{
    // Main context entities
    [typeof(Company)] = typeof(CompanyAuditLog),
    [typeof(User)] = typeof(UserAuditLog),
    [typeof(Role)] = typeof(RoleAuditLog),
    // ... other mappings
    
    // Company context entities
    [typeof(Article)] = typeof(ArticleAuditLog),
    // ... other company entity mappings
};
```

This mapping system makes it easy to associate each entity with its specialized audit log type.
And it was necesarry to provide functionality of interceptor to determine which class object to create and where to save the data (in which table).

### Audit Data Storage

Audit logs are stored in separate tables based on entity type, which provides several benefits:

1. **Query Performance**: Filtering by entity type happens at the table level
2. **Data Volume Management**: Large-volume entity logs don't impact other audit queries
3. **Retention Policies**: Different retention periods can be applied to different audit log types

### Captured Audit Information

For each change, the system records:

- **Action Type**: Create, Update, or Delete
- **Timestamp**: When the change occurred (UTC)
- **User Information**: ID and name of the user (if authenticated via JWT)
- **API Key**: ID of the API key (if authenticated via API key)
- **Company ID**: The tenant context in which the change occurred
- **Entity ID**: The primary key of the changed entity (**It's a string to support defining multi-property keys**)
- **Endpoint**: The API endpoint that triggered the change
- **Changed Properties**: JSON serialization of old and new values for each changed property

Example audit log entry:
```json
{
  "ActionType": "Update",
  "Timestamp": "2025-03-28T15:42:18.123Z",
  "UserId": 42,
  "UserName": "john.doe",
  "ApiKeyId": null,
  "CompanyId": 7,
  "EntityId": "123",
  "Endpoint": "PUT /api/users/123",
  "ChangedProperties": [
    {
      "PropertyName": "Email",
      "OldValue": "john@example.com",
      "NewValue": "john.doe@example.com"
    },
    {
      "PropertyName": "SomeBoolProperty",
      "OldValue": false,
      "NewValue": true
    }
  ]
}
```

### Selective Auditing

The audit system allows excluding specific entities or properties from auditing:

```csharp
// In AuditExclusionConfiguration.cs
public static void ConfigureExclusions(AuditOptions options)
{
    // Exclude whole entity from auditing
    options.ExcludeEntity<RefreshToken>();
    
    // Exclude specific property of entity from auditing
    options.ExcludeProperty<ApiKey>(u => u.KeyHash);
    options.ExcludeProperty<User>(u => u.ConcurrencyStamp);
    options.ExcludeProperty<User>(u => u.SecurityStamp);
    // ... other exclusions
}
```

Typical exclusions include:
- Sensitive fields (passwords, security tokens)
- High-frequency changing properties (timestamps, concurrency stamps)
- Large binary data (files, images)
- Temporary or transient entities


### Audit Database Migrations

Audit database migrations are managed alongside the main and company databases:

```csharp
public static void ApplyMigrations(this IApplicationBuilder app)
{
    using var serviceScope = app.ApplicationServices.CreateScope();
    
    // Apply migrations to MainDbContext
    var mainContext = serviceScope.ServiceProvider.GetRequiredService<MainDbContext>();
    mainContext.Database.Migrate();
    
    // Apply migrations to AuditDbContext
    var mainAuditContext = serviceScope.ServiceProvider.GetRequiredService<MainAuditDbContext>();
    mainAuditContext.Database.Migrate();
    
    var companyAuditContext = serviceScope.ServiceProvider.GetRequiredService<CompanyAuditDbContext>();
    companyAuditContext.Database.Migrate();
    
    // Apply migrations to all company databases
    // ...
}
```

### Configuring Entity Auditing

When adding a new entity that needs to be audited:

1. Create a specialized audit log class:
   ```csharp
   public class NewEntityAuditLog : AuditLog { }
   ```

2. Add it to the appropriate audit context:
   ```csharp
   // In MainAuditDbContext or CompanyAuditDbContext
   public DbSet<NewEntityAuditLog> NewEntityAuditLogs { get; set; }
   ```

3. Register the mapping in `AuditTypeMapper`:
   ```csharp
   EntityToAuditTypeMap[typeof(NewEntity)] = typeof(NewEntityAuditLog);
   ```

4. Configure any property exclusions if necessary:
   ```csharp
   options.ExcludeProperty<NewEntity>(e => e.SensitiveProperty);
   ```

### Querying Audit Logs

Audit logs can be queried using standard Entity Framework methods:

```csharp
// Get all changes to a specific entity
var entityChanges = await mainAuditContext.Users
    .Where(a => a.EntityId == "123")
    .OrderByDescending(a => a.Timestamp)
    .ToListAsync();

// Get all changes by a specific user
var userChanges = await mainAuditContext.Set<AuditLog>()
    .Where(a => a.UserId == userId)
    .OrderByDescending(a => a.Timestamp)
    .ToListAsync();

// Get recent delete operations
var recentDeletes = await mainAuditContext.Set<AuditLog>()
    .Where(a => a.ActionType == AuditActionType.Delete && 
                a.Timestamp > DateTime.UtcNow.AddDays(-7))
    .OrderByDescending(a => a.Timestamp)
    .ToListAsync();
```

### Performance Considerations

The audit system is designed with performance in mind:

1. **Table Segregation**: Entity-specific tables prevent full table scans
2. **Separate Database**: Audit operations don't impact application performance
3. **Selective Auditing**: Only necessary fields are tracked, reducing storage requirements
4. **Optimized Change Detection**: Only modified properties are recorded

For high-throughput systems, consider implementing periodic audit table archiving or partitioning the database tables.
Changes in the interceptor are stored in the dictionary, which can be easily extended to save them later via some job queue.

---

## Authentication and Authorization

Project has an authentication and authorization system using JWT tokens, refresh tokens, API keys, and a multi-layered permission system.

### Authentication Flow

1. **JWT Authentication**
    - Upon successful login, the system issues two tokens:
        - An **access token** (short-lived, default: 15 minutes)
        - A **refresh token** (longer-lived, default: 1 day)
    - JWT tokens contain minimal claims:
      ```
      {
        "nameid": "123",         // User ID
        "CompanyId": "456",      // Company ID
        "UserType": "TenantUser", // User role type
        "DatabaseType": "2",     // Database connection identifier
        "exp": 1616161616,       // Expiration timestamp
        "iss": "MultiSoftSRB",      // Issuer
        "aud": "MultiSoftSRBClients" // Audience
      }
      ```
    - Tokens are validated using the JWT authentication middleware

2. **API Key Authentication**
    - Alternative authentication scheme for system integrations
    - API keys are passed via `x-api-key your-api-key-here` header
    - Runs in parallel with JWT authentication

### Claim Hydration Process

A key feature of this system is **claim hydration** - permissions are not stored in the JWT but dynamically loaded on each request:

1. `UserPermissionClaimHydrator` implements `IClaimsTransformation` to inject permission claims
2. When a request is received, the hydrator:
    - Extracts `userId` and `companyId` from the incoming JWT
    - Checks if the request uses API key auth (skips hydration if true)
    - Queries the database for the user's permissions via `UserProvider` (cached)
    - Transforms permissions into claims and adds them to the user identity

This approach ensures:
- JWTs remain small and manageable
- Permissions stay up-to-date without requiring re-login
- Permission changes take effect immediately (if cache is cleared)

### Database Resolution

The system determines which database to connect to using claims:

1. `CompanyProvider` extracts the `DatabaseType` claim
2. It maps this value to a connection string using the `CompanyConnectionStrings` configuration
3. The appropriate database context is initialized with the resolved connection string

### Permission System Architecture

The permission system consists of three interconnected layers:

#### 1. User Types

User types define coarse-grained access levels:
- `SuperAdmin`: System-wide administrator with all permissions
- `Consultant`: User with all permissions for assigned companies, used to provide support to clients
- `TenantAdmin`: Company-level administrator with all permissions for their company
- `TenantUser`: Regular user with permissions defined by assigned roles

#### 2. Page Permissions

Page permissions control UI-level access:
- Stored in `PagePermissions.cs` as constants (e.g., `users.view-page`, `roles.edit-page`)
- Used by the frontend to show/hide UI elements
- Assigned to roles which are then assigned to users
- Retrieved via `UserProvider.GetCurrentUserPagePermissionsAsync()`

#### 3. Resource Permissions

Resource permissions define granular backend access control:
- Stored in `ResourcePermissions.cs` as constants (e.g., `user.read`, `invoice.create`)
- Used for authorization checks in services and endpoints
- Mapped from page permissions using `PermissionMapper.cs`
- Verified using `UserProvider.HasResourcePermission()` or FastEndpoints' Permissions method in endpoint configuration

### Permission Mapping Flow

The mapping between page and resource permissions works as follows:

1. Page permissions are assigned to roles in the database
2. When a user is authenticated, their roles are retrieved
3. From roles, page permissions are loaded
4. Page permissions are mapped to resource permissions using `PermissionMapper`
5. Both permission types are cached for 15 minutes per user per company

Example mapping in `PermissionMapper.cs`:
```csharp
[PagePermissions.UsersView] = [
    ResourcePermissions.UserList, 
    ResourcePermissions.UserRead
]
```

This means a user with the `users.view-page` permission gets both `user.list` and `user.read` resource permissions.

### Authorization Checks

The system provides multiple ways to check permissions:

1. **Code-based checks** using `UserProvider`:
   ```csharp
   if (userProvider.HasResourcePermission(ResourcePermissions.UserCreate))
   {
       // Perform protected operation
   }
   ```

2. **Attribute-based auth** in FastEndpoints:
   ```csharp
   public class CreateUserEndpoint : Endpoint<CreateUserRequest>
   {
       public override void Configure()
       {
           Post("auth/users");
           Permissions(ResourcePermissions.UserCreate);
       }
   }
   ```

### Caching Strategy

To optimize performance, permissions are cached:

1. Permission data is cached using `FusionCache` for 15 minutes
2. Cache keys follow patterns:
    - `PagePermissions_{userId}_{companyId}`
    - `ResourcePermissions_{userId}_{companyId}`
3. Cache is invalidated when:
    - User's roles are changed
    - User is assigned to a new company
    - Roles are updated with different permissions

### Implementing New Permissions

To add new permissions to the system:

1. Add page permission constants to `PagePermissions.cs`:
   ```csharp
   public const string NewFeatureView = "newfeature.view-page";
   ```

2. Add resource permission constants to `ResourcePermissions.cs`:
   ```csharp
   public const string NewFeatureRead = "newfeature.read";
   ```

3. Map page to resource permissions in `PermissionMapper.cs`:
   ```csharp
   [PagePermissions.NewFeatureView] = [
       ResourcePermissions.NewFeatureRead,
       // Other related permissions
   ]
   ```

4. Update database by adding permissions to appropriate roles

---

# API Keys

The API Key authentication system provides an alternative authentication mechanism designed for machine-to-machine communication, service integrations, and automated tasks. It runs in parallel with JWT authentication, allowing both authentication methods to coexist within the same application.

### Key Features

- **Secure Storage**: API keys are stored as SHA-256 hashes in the database
- **Permission-based Access**: Each API key can be assigned specific resource permissions
- **Company Scoping**: Keys are associated with a specific company/tenant
- **Performance Optimization**: Validation results are cached to minimize database queries
- **Expiration Support**: Optional expiration dates for time-limited access

### How API Key Authentication Works

1. **Authentication Process**:

    - The `ApiKeyAuthenticationHandler` intercepts the request
    - Extracts the API key from the `x-api-key` header
    - Hashes the provided key and compares it with stored hashes
    - On success, creates a `ClaimsPrincipal` with appropriate claims
    - On failure, continues to JWT authentication or returns 401


2. **Claim Generation**:

   When authenticated via API key, the following claims are added:

   ```csharp
   // API key identifier
   new Claim(CustomClaimTypes.ApiKeyId, validationResult.ApiKeyId.ToString()!)
   
   // Associated company
   new Claim(CustomClaimTypes.CompanyId, validationResult.CompanyId.ToString())
   
   // Each permission as a separate claim 
   foreach (var permission in validationResult.Permissions)
   {
       claims.Add(new Claim(CustomClaimTypes.ResourcePermission, permission));
   }
   ```


### Implementation Details

1. **Key Components**:

    - `ApiKeyAuthenticationHandler`: Custom authentication handler implementing the API key verification logic
    - `ApiKeyProvider`: Helper service to extract API key information from the current request
    - `ApiKeyValidationResult`: Data model for validation outcomes
    - `ApiKeyInfo`: Data model representing API key metadata


2. **Validation and Caching**:

   API key validation results are cached for 15 minutes to improve performance:

   ```csharp
   // Cache key based on the API key value
   var cacheKey = $"ApiKey_{providedApiKey}";
   
   // Cached validation using FusionCache
   return await cache.GetOrSetAsync(
       cacheKey,
       async _ => {
           // ...
       },
       options);
   ```

---

## FastEndpoints Implementation

This project uses [FastEndpoints](https://fast-endpoints.com/) as a structured, high-performance alternative to traditional ASP.NET controllers. FastEndpoints provides a vertical slice architecture approach that organizes code around features rather than technical concerns.

### Endpoint Organization

Each API endpoint is organized in a feature-based folder structure:

```
Features/
├── Auth/
│   ├── Login/
│   │   ├── Endpoint.cs
│   │   ├── Request.cs
│   │   └── Response.cs
│   ├── Users/
│       ├── CreateUser/
│       │   ├── Endpoint.cs
│       │   ├── Request.cs
│       │   └── Response.cs
│       ├── GetUser/
│       │   ├── Endpoint.cs
│       │   ├── Request.cs
│       │   └── Response.cs
│       └── UpdateUser/
│           ├── Endpoint.cs
│           ├── Request.cs
│           └── Response.cs
├── Companies/
    ├── CreateCompany/
    │   ├── Endpoint.cs
    │   ├── Request.cs
    │   └── Response.cs
    └── ...
```

This structure offers several advantages:
- **Feature Encapsulation**: All components of a feature are contained in a single folder
- **Discoverability**: Developers can easily locate endpoints by their business function
- **Isolation**: Changes to one feature are less likely to affect others
- **Testability**: Each endpoint can be unit tested in isolation

### Endpoint Components

Each endpoint consists of three primary files:

1. **Endpoint.cs**: Contains the endpoint logic, route configuration, and permission requirements
   ```csharp
   sealed class Endpoint : Endpoint<Request, Response>
   {
       public MainDbContext MainDbContext { get; set; }
       public UserManager<User> UserManager { get; set; }
       
       public override void Configure()
       {
           Post("auth/users");
           Permissions(ResourcePermissions.UserCreate);
       }

       public override async Task HandleAsync(Request request, CancellationToken ct)
       {
           // Implementation logic
           await SendOkAsync(new Response { Id = user.Id }, ct);
       }
   }
   ```

2. **Request.cs**: Defines the request model and validation rules
   ```csharp
   sealed class Request
   {
       public string Email { get; set; } = null!;
       public string FirstName { get; set; } = null!;
       public string Password { get; set; } = null!;
       public long RoleId { get; set; }
       
       internal sealed class Validator : AbstractValidator<Request>
       {
           public Validator()
           {
               RuleFor(x => x.Email).NotEmpty().EmailAddress();
               // Additional validation rules
           }
       }
   }
   ```

3. **Response.cs**: Defines the response model
   ```csharp
   sealed class Response 
   {
       public long Id { get; set; }
   }
   ```

### Dependency Injection

FastEndpoints automatically injects services into endpoints:

```csharp
public MainDbContext MainDbContext { get; set; }
public UserManager<User> UserManager { get; set; }
public CompanyProvider CompanyProvider { get; set; }
```

No constructor injection is needed, which reduces boilerplate code.

### Permission-Based Authorization

Permissions are applied in the `Configure()` method:

```csharp
public override void Configure()
{
    Post("auth/users");
    Permissions(ResourcePermissions.UserCreate);
}
```

This integrates with our custom permission system to enforce authorization rules.

### Request Validation

Validation is defined alongside the request model using FluentValidation:

```csharp
internal sealed class CreateCompanyUserValidator : Validator<Request>
{
    public CreateCompanyUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required");
        
        // Additional validation rules
    }
}
```

Validation is automatically applied before the endpoint handler executes.

### Error Handling

Errors can be managed using the built-in error handling methods:

```csharp
if (!roleExists)
    ThrowError("Invalid role");

// Or for field-specific errors:
AddError(r => r.Email, "A user with this email already exists");
await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
```

This produces consistent error responses across all endpoints.

### Global Configuration

FastEndpoints is configured in `Program.cs`:

```csharp
app.UseFastEndpoints(c =>
{
    c.Binding.ReflectionCache.AddFromMultiSoftSRB();
    c.Errors.UseProblemDetails();
    c.Security.PermissionsClaimType = CustomClaimTypes.ResourcePermission;
    c.Endpoints.Configurator = ep =>
    {
        ep.AuthSchemes(JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationHandler.SchemeName);
        ep.Description(b => b.Produces<InternalErrorResponse>(500));
    };
});
```

This centralized configuration ensures consistent behavior across all endpoints.

### Benefits Over Controllers

The FastEndpoints approach offers several advantages over traditional controllers:
- **Reduced boilerplate**: No need for controller classes or route attributes
- **Vertical slicing**: Features are self-contained and modular
- **Clear separation**: Request, response, and handling logic are distinct
- **Performance**: FastEndpoints is designed for high-performance scenarios
- **Testability**: Endpoints are easier to test in isolation
- **Consistency**: Enforces a consistent structure across the application

---

## Docker Support


### Development Workflow with Docker
Docker Compose simplifies multi-container application management by using a YAML file to define, configure, and run all your application's services together.
For developers starting with this project, Docker Compose is invaluable because it eliminates environment setup headaches - 
no need to manually install PostgreSQL, configure multiple databases. 

With a single docker compose up command, it automatically creates all required databases (main, audit, and company-specific),
launches the API with proper configurations, and ensures everything works together correctly.

To develop using Docker:

1. **Start all services**:
   ```bash
   docker compose up -d
   ```

2. **View logs**:
   ```bash
   docker compose logs -f api
   ```

3. **Run migrations**:
   ```bash
   # Migrations are applied automatically on startup
   # To manually apply:
   docker compose exec api dotnet ef database update --context MainDbContext
   ```

4. **Stop all services**:
   ```bash
   docker compose down
   ```

5. **Rebuild after code changes**:
   ```bash
   docker compose build api
   docker compose up -d api
   ```

---

## CI/CD with GitHub Actions and Azure

The project implements a straightforward CI/CD pipeline using GitHub Actions for automated building and deployment to Azure App Service when code is merged to the master branch.

### Pipeline Overview

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                 │     │                 │     │                 │
│   Push to       │────►│  Build Docker   │────►│  Deploy to      │
│   master        │     │  Image          │     │  Azure App      │
│                 │     │                 │     │  Service        │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

### GitHub Actions Workflow

The CI/CD pipeline is defined in a single GitHub Actions workflow file (`.github/workflows/azure-deploy.yml`):

### How the Pipeline Works

The workflow consists of two jobs:

1. **Build Job**:
   - Checks out the code repository
   - Sets up Docker Buildx for multi-platform image building
   - Authenticates with GitHub Container Registry (ghcr.io)
   - Builds a Docker image using the Dockerfile in the Source directory
   - Tags the image with both `latest` and the specific commit SHA
   - Pushes the image to GitHub Container Registry

2. **Deploy Job**:
   - Runs after the build job completes successfully
   - Uses the Azure WebApp Deploy action to deploy the container image
   - Deploys to the Development environment in Azure App Service
   - Uses a publish profile stored in GitHub Secrets for authentication
   - References the specific image by commit SHA for deployment

### Triggering the Pipeline

The CI/CD pipeline is triggered in two ways:
- Automatically when code is pushed to the `master` branch
- Manually via GitHub's workflow dispatch interface

### Azure Integration

The application is deployed to Azure App Service as a containerized application. This approach provides several benefits:

1. **Consistency**: The exact same container that passes tests is deployed to production
2. **Isolation**: All dependencies are packaged within the container
3. **Versioning**: Each deployment is tagged with a specific commit SHA for traceability
4. **Rollback**: Easy rollback to previous versions by deploying an earlier image

### Security Considerations

The workflow implements several security best practices:

1. **Limited Permissions**: The workflow uses minimal permissions required for each job
2. **Secrets Management**: Sensitive information is stored in GitHub Secrets
3. **Specific Versioning**: All action versions are pinned to specific versions or SHA hashes
4. **Environment Protection**: Deployment is tied to a specific environment which can have protection rules

### Azure App Service Configuration

The Azure App Service is configured to run the container with the necessary environment variables and configuration:

1. **Container Settings**: Configured to pull from GitHub Container Registry
2. **Environment Variables**: Database connection strings and other configuration
3. **Custom Domain**: Mapped to the appropriate custom domain with SSL
4. **Continuous Deployment**: Set to use the container deployed by GitHub Actions

For local development, developers can use Docker Compose (as described in the Docker Support section), while the CI/CD pipeline handles building and deploying container images to the Azure environment.

---

## Code Quality Guidelines

To maintain high code quality and ensure long-term maintainability of the project, we recommend following these guidelines. While not strictly enforced, adhering to these practices will significantly improve code readability, collaboration, and project longevity.

### Naming Conventions

1. **Be Descriptive**
   - Use clear, descriptive names that reveal intent
   - Prefer longer, more descriptive names over short, cryptic ones
   - Examples:
      - ✅ `GetUserPermissionsForCompany()` instead of ✗ `GetPerms()`
      - ✅ `isUserAuthorized` instead of ✗ `isAuth`
      - ✅ `CompanyConnectionStrings` instead of ✗ `ConnStrs`

2. **Entity Naming**
   - Name entities in singular form (`User`, not `Users`)
   - Use domain-specific terminology that matches business concepts
   - When creating audit log entities, follow the pattern `EntityNameAuditLog`

3. **Method Naming**
   - Use verb-noun format for methods (`GetUser()`, `ValidatePermission()`)
   - Prefix boolean methods with "is", "has", or "can" (`isUserActive()`, `hasPermission()`)

4. **Interface Naming**
   - Prefix interfaces with "I" (`IUserProvider`, `IAuditService`)

### Coding Practices

1. **Single Responsibility**
   - Each class should have a single responsibility
   - Follow the pattern seen in the entity configurations (one file per entity)
   - Break down large methods into smaller, focused functions

2. **Comments and Documentation**
   - Document public APIs with XML comments
   - Explain "why" rather than "what" in comments
   - Add summary comments for complex business logic
   - Example:
     ```csharp
     /// <summary>
     /// Hydrates user permissions based on company and role assignments.
     /// This ensures permissions are always up-to-date without requiring re-login.
     /// </summary>
     public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
     ```

3. **Error Handling**
   - Use meaningful exception messages
   - Create custom exceptions for domain-specific errors
   - Handle null values and edge cases explicitly

### Git Workflow

1. **Conventional Commits**

   We suggest following the [Conventional Commits](https://www.conventionalcommits.org/) specification for commit messages to improve readability and enable automated versioning in future if needed:

   ```
   <type>[optional scope]: <description>

   [optional body]

   [optional footer(s)]
   ```

   Common types:
   - `feat`: A new feature
   - `fix`: A bug fix
   - `docs`: Documentation changes
   - `style`: Formatting changes that don't affect code behavior
   - `refactor`: Code changes that neither fix bugs nor add features
   - `test`: Adding or correcting tests
   - `chore`: Maintenance tasks, dependency updates, etc.

   Examples:
   ```
   feat(auth): add API key expiration support
   fix(database): resolve connection leak in CompanyDbContext
   docs: update README with API key documentation
   refactor(permissions): simplify permission mapping logic
   ```

2. **Pull Requests**
   - Use descriptive PR titles that summarize the change
   - Include a detailed description of changes
   - Reference related issues with "Fixes #123" or "Related to #456"
   - Keep PRs focused on a single concern

3. **Branching**
   - Use feature branches for new features: `feature/add-api-key-expiration`
   - Use bug branches for fixes: `fix/company-db-connection-leak`

   
