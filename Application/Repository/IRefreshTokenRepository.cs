
using Domain.Entity;

namespace Application.Repository;
public interface IRefreshTokenRepository
{
    Task Add(RefreshToken refreshToken);
}
