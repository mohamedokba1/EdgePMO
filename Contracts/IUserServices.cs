using EdgePMO.API.Dtos;

namespace EdgePMO.API.Contracts
{
    public interface IUserServices
    {
        Task<Response> Activate(Guid userId);

        Task<Response> Deactivate(Guid userId);

        Task<Response> Delete(Guid userId);

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

        Task<Response> GoogleLoginAsync(string idToken);

        /// <summary>
        /// Requirement 3.6 — one device at a time for video playback. Rotates the user's
        /// SessionId (identical mechanism to login/refresh) and issues a fresh token for
        /// the calling device. Any other device holding the old SessionId gets logged out
        /// on its very next authenticated request, via the existing OnTokenValidated check
        /// in Program.cs — no new enforcement path needed, just reusing it at a new trigger
        /// point (start of video playback, not just login).
        /// </summary>
        Task<Response> ClaimPlaybackSessionAsync(Guid userId);
    }
}