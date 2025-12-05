namespace EdgePMO.API.Contracts
{
    public interface IEmailService
    {
        Task<bool> SendEmailVerficationAsync(string toEmail, string subject, string VerificationCode);
        int GenerateVerificationCode();
    }
}
