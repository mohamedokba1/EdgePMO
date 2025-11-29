namespace EdgePMO.API.Contracts
{
    public interface IEmailService
    {
        Task<bool> SendEmailVerficationAsync(string toEmail, string VerificationCode);
        int GenerateVerificationCode();
    }
}
