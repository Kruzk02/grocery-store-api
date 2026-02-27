
using Domain.Entity;

namespace Application.Repository;

public interface IRefreshTokenRepository
{
    Task Add(RefreshToken refreshToken);
    Task<RefreshToken?> FindByToken(string RefreshToken);
    Task RevokeTokenByUserId(string UserId);
}
