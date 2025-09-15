using Microsoft.EntityFrameworkCore;
using Shared.Core.Model;
using Verification.Core.Enum;

namespace Verification.Core.Model;

[Index(nameof(Code), IsUnique = true)]
public class VerificationCode : BaseModel
{
    public Guid UserId { get; init; }
    public VerificationCodeType Type { get; init; }
    public string Content { get; init; }
    public bool Used { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime ExpiresAt { get; init; }

    protected VerificationCode() { }

    public VerificationCode(Guid userId, VerificationCodeType type): base()
    {
        UserId = userId;
        Type = type;
        Content = GenerateRandomCode();
        Used = false;
        ExpiresAt = DateTime.UtcNow.AddMinutes(15);
    }

    private static string GenerateRandomCode()
    {
        var id = Guid.NewGuid();
        return id.ToString("N")[..8].ToLower();
    }

    public bool IsValid()
    {
        return !Used && ExpiresAt > DateTime.UtcNow;
    }

    public bool Matches(string code)
    {
        return string.Equals(Content, code, StringComparison.OrdinalIgnoreCase);
    }

    public void MarkAsUsed()
    {
        if (Used) return;
        Used = true;
        VerifiedAt = DateTime.UtcNow;
    }
}