namespace MultiSoftSRB.Entities.Main;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = null!;
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    
    public long? CompanyId { get; set; }
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Company? Company { get; set; }
}