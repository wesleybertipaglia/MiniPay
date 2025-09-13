using Verification.Core.Model;

namespace Verification.Core.Interface;

public interface IVerificationCodeRepository
{
    Task<VerificationCode?> GetByIdAsync(Guid id);
    Task<VerificationCode?> GetByUserIdAsync(Guid userId);
    Task<VerificationCode> CreateAsync(VerificationCode entity);
    Task<VerificationCode> UpdateAsync(VerificationCode entity);
}