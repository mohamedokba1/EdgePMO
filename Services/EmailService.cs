using EdgePMO.API.Contracts;
using System.Net;
using System.Net.Mail;

namespace EdgePMO.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public int GenerateVerificationCode()
        {
            return 1;
        }

        public async Task<bool> SendEmailVerficationAsync(string to, string VerificationCode)
        {
            using SmtpClient? client = new SmtpClient(_config["SmtpSettings:host"], 587)
            {
                Credentials = new NetworkCredential(_config["SmtpSettings:UserName"], _config["SmtpSettings:Password"]),
                EnableSsl = true
            };

            MailMessage? mail = new MailMessage()
            {
                From = new MailAddress(_config["SmtpSettings:UserName"] ?? "operation@edgepmo.com", _config["SmtpSettings:FromEmail"]),
                Subject = "Your Verification Code",
                Body = GetHtmlTemplate(VerificationCode),
                IsBodyHtml = true
            };
            mail.To.Add(new MailAddress(to));

            await client.SendMailAsync(mail);
            return true;
        }

        private string GetHtmlTemplate(string token)
        {
            return $@"
                <table style='max-width:550px;width:100%;margin:auto;font-family:Arial, sans-serif;background:#ffffff;padding:20px;border-radius:10px;color:#333'>
                <tr>
                <td style='text-align:center'>
                <h2 style='color:#4a76ff'>Your Verification Code</h2>
                <p style='font-size:16px'>Use the following code to verify your email:</p>

                <div style='font-size:32px;letter-spacing:5px;font-weight:bold;margin:20px 0;color:#4a76ff'>
                {token}
                </div>

                <p style='font-size:14px;color:#777'>
                This code will expire in 10 minutes.
                </p>

                <hr />

                <p style='font-size:12px;color:#999'>If you didn’t request this, please ignore this email.</p>
                </td>
                </tr>
                </table>";
        }
    }
}
