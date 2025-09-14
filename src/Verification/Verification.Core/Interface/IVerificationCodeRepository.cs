using Verification.Core.Enum;
using Verification.Core.Model;

namespace Verification.Core.Interface;

public interface IVerificationCodeRepository
{
    Task<VerificationCode?> GetByUserIdAsync(Guid userId);
    Task<VerificationCode?> GetByUserIdAndCodeAsync(Guid userId, string code);
    Task<VerificationCode?> GetLatestValidCodeByUserIdAndTypeAsync(Guid userId, VerificationCodeType type);
    Task<VerificationCode> CreateAsync(VerificationCode entity);
    Task<VerificationCode> UpdateAsync(VerificationCode entity);
}