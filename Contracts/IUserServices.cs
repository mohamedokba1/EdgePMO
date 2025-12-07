using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IUserServices
    {
        Task<Response> GetAllUsersAsync();
        Task<Response> Refresh(string refreshToken);
        Task<Response> Login(LoginDto dto);
        Task<Response> Logout(Guid userId);
        Task<Response> Register(RegisterUserDto dto, bool isAdmin = false);
        Task<Response> EmailVerification(VerifyEmailDto dto);
        Task<Response> SendVerificationMail(VerifyRequestDto dto, string subject);
        Task<Response> SendPasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(PasswordResetDto dto);
        Task<Response> GetProfileAsync(Guid? userId, string? email);
    }
}
