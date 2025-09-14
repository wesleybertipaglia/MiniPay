using Microsoft.EntityFrameworkCore;
using Verification.Core.Enum;
using Verification.Core.Interface;
using Verification.Core.Model;
using Verification.Infrastructure.Data;

namespace Verification.Infrastructure.Repository;

public class VerificationCodeRepository(AppDbContext context) : IVerificationCodeRepository
{
    public async Task<VerificationCode?> GetByUserIdAsync(Guid userId)
    {
        return await context.VerificationCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(vc => vc.UserId == userId);
    }
    
    public async Task<VerificationCode?> GetByUserIdAndCodeAsync(Guid userId, string code)
    {
        return await context.VerificationCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(vc => vc.UserId == userId && vc.Content.Equals(code, StringComparison.OrdinalIgnoreCase));
    }
    
    public async Task<VerificationCode?> GetLatestValidCodeByUserIdAndTypeAsync(Guid userId, VerificationCodeType type)
    {
        return await context.VerificationCodes
            .AsNoTracking()
            .Where(vc =>
                vc.UserId == userId &&
                vc.Type == type &&
                !vc.Used &&
                vc.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(vc => vc.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<VerificationCode> CreateAsync(VerificationCode entity)
    {
        context.VerificationCodes.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }
    
    public async Task<VerificationCode> UpdateAsync(VerificationCode entity)
    {
        context.VerificationCodes.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }
}
