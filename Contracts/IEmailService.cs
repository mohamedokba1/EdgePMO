namespace EdgePMO.API.Contracts
{
    public interface IEmailService
    {
        Task<bool> SendEmailVerficationAsync(string toEmail, string subject, string VerificationCode);
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        int GenerateVerificationCode();
    }
}
