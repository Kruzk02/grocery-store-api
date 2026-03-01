
using Domain.Entity;

namespace Application.Repository;

public interface IRefreshTokenRepository
{
    Task Add(RefreshToken refreshToken);
    Task<RefreshToken?> FindByToken(string RefreshToken);
    Task<List<RefreshToken>> FindAllByUserId(string userId);
    Task RevokeTokenByUserId(string userId);
    Task DeleteByToken(RefreshToken refreshToken);
}
