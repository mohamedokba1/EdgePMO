using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IUserServices
    {
        Task<Response> Activate(Guid userId);

        Task<Response> Deactivate(Guid userId);

        Task<Response> EmailVerification(VerifyEmailDto dto);

        Task<Response> GetAllUsersAsync();

        Task<Response> GetProfileAsync(Guid? userId, string? email);

        Task<Response> Login(LoginDto dto);

        Task<Response> Logout(Guid userId);

        Task<Response> Refresh(string refreshToken);

        Task<Response> Register(RegisterUserDto dto, bool isAdmin = false);

        Task<bool> ResetPasswordAsync(PasswordResetDto dto);

        Task<Response> SendPasswordResetTokenAsync(string email);

        Task<Response> SendVerificationMail(VerifyRequestDto dto, string subject);
    }
}