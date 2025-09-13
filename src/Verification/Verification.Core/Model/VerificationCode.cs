using Shared.Core.Model;

namespace Verification.Core.Model;

public class VerificationCode : BaseModel
{
    public Guid UserId { get; set; }

    public string Content { get; set; }

    public bool Used { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
    
    protected VerificationCode() { }

    public VerificationCode(Guid userId)
    {
        UserId = userId;
        Content = GenerateRandomCode();
        Used = false;
        ExpiresAt = DateTime.UtcNow.AddMinutes(15);
    }

    private static string GenerateRandomCode()
    {
        var id = Guid.NewGuid();
        return id.ToString("N")[..8].ToLower();
    }
}