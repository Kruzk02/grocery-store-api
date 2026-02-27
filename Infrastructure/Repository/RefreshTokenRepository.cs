using Application.Repository;

using Domain.Entity;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class RefreshTokenRepository(ApplicationDbContext dbContext) : IRefreshTokenRepository
{
    public async Task Add(RefreshToken refreshToken)
    {
        _ = await dbContext.RefreshTokens.AddAsync(refreshToken);
        _ = await dbContext.SaveChangesAsync();
    }

    public async Task<RefreshToken?> FindByToken(string RefreshToken)
    {
        return await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == RefreshToken);
    }

    public async Task RevokeTokenByUserId(string UserId)
    {
        RefreshToken[] userTokens = await dbContext.RefreshTokens.Where(x => x.UserId == UserId && !x.IsRevoked).ToArrayAsync();

        foreach (RefreshToken token in userTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        _ = await dbContext.SaveChangesAsync();
    }
}
