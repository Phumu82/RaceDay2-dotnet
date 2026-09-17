using RaceDay.Domain.Entities;

namespace RaceDay.Api.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(Profile profile);
}
