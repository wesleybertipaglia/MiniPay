using Microsoft.EntityFrameworkCore;
using Verification.Core.Interface;
using Verification.Core.Model;
using Verification.Infrastructure.Data;

namespace Verification.Infrastructure.Repository;

public class VerificationCodeRepository(AppDbContext context) : IVerificationCodeRepository
{
    public async Task<VerificationCode?> GetByIdAsync(Guid id)
    {
        return await context.VerificationCodes.FindAsync(id);
    }

    public async Task<VerificationCode?> GetByUserIdAsync(Guid userId)
    {
        return await context.VerificationCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(vc => vc.UserId == userId);
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