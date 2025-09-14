using Shared.Core.Dto;
using Verification.Core.Dto;
using Verification.Core.Enum;

namespace Verification.Core.Interface;

public interface IVerificationCodeService
{
    Task<VerificationCodeDto> CreateAsync(Guid userId, VerificationCodeType type);
    Task ValidateAsync(Guid userId, string code);
}