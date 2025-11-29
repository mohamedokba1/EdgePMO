using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IUserServices
    {
        Task<IEnumerable<UserReadDto>> GetAllUsersAsync();
        Task<Response> Login(LoginDto dto);
        Task<Response> Logout(Guid userId);
        Task<Response> Register(RegisterUserDto dto);
        Task<Response> EmailVerification(VerifyEmailDto dto);
        Task<Response> SendVerificationMail(VerifyRequestDto dto);
        Task<Response> SendPasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(PasswordResetDto dto);
    }
}
