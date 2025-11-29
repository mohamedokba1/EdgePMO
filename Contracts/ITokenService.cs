using EdgePMO.API.Dtos;
using EdgePMO.API.Models;

namespace EdgePMO.API.Contracts
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken();
    }
}
