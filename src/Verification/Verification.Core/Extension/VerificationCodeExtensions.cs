using Verification.Core.Dto;

namespace Verification.Core.Extension;

public static class VerificationCodeExtensions
{
    public static bool IsValid(this VerificationCodeDto code)
    {
        ArgumentNullException.ThrowIfNull(code);
        return !code.Used && code.ExpiresAt > DateTime.UtcNow;
    }
}
