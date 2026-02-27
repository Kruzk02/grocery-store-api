using Application.Repository;

using Domain.Entity;

using Infrastructure.Persistence;

namespace Infrastructure.Repository;

public class RefreshTokenRepository(ApplicationDbContext dbContext) : IRefreshTokenRepository
{
    public async Task Add(RefreshToken refreshToken)
    {
        _ = await dbContext.RefreshTokens.AddAsync(refreshToken);
        _ = await dbContext.SaveChangesAsync();
    }
}
