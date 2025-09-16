using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Model;
using Verification.Core.Enum;

namespace Verification.Core.Model;

[Index(nameof(Code), IsUnique = true)]
[Index(nameof(UserId))]
public class VerificationCode : BaseModel
{
    [Required]
    public Guid UserId { get; private set; }

    [Required]
    public VerificationCodeType Type { get; private set; }

    [Required]
    [MaxLength(50)]
    public string Content { get; private set; }

    public bool Used { get; private set; }

    public DateTime? VerifiedAt { get; private set; }

    [Required]
    public DateTime ExpiresAt { get; private set; }
    
    protected VerificationCode() { }

    public VerificationCode(Guid userId, VerificationCodeType type)
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