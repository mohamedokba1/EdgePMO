using EdgePMO.API.Contracts;
using EdgePMO.API.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EdgePMO.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        public EmailService(IOptions<EmailSettings> config)
        {
            _emailSettings = config.Value;
        }
        public int GenerateVerificationCode()
        {
            return 1;
        }

        public async Task<bool> SendEmailVerficationAsync(string to, string subject, string verificationCode)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            BodyBuilder? bodyBuilder = new BodyBuilder();

            bodyBuilder.HtmlBody = GetHtmlTemplate(verificationCode);
            bodyBuilder.TextBody = $"Your verification code is: {verificationCode}";

            message.Body = bodyBuilder.ToMessageBody();

            using SmtpClient client = new SmtpClient();

            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            BodyBuilder bodyBuilder = new BodyBuilder();

            if (isHtml)
            {
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }

            message.Body = bodyBuilder.ToMessageBody();

            using SmtpClient client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }

        private string GetHtmlTemplate(string token)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px; }}
                        .container {{ max-width: 550px; width: 100%; margin: auto; background: #ffffff; padding: 30px; border-radius: 10px; color: #333; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; color: #4a76ff; margin-bottom: 20px; }}
                        .token {{ font-size: 32px; letter-spacing: 5px; font-weight: bold; margin: 25px 0; color: #4a76ff; text-align: center; padding: 15px; background: #f8f9ff; border-radius: 5px; }}
                        .footer {{ margin-top: 25px; padding-top: 20px; border-top: 1px solid #eee; text-align: center; color: #999; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Your Verification Code</h2>
                            <p style='font-size: 16px; color: #666;'>Use the following code to verify your email address:</p>
                        </div>
            
                        <div class='token'>{token}</div>
            
                        <p style='text-align: center; font-size: 14px; color: #777;'>
                            This code will expire in 10 minutes for security reasons.
                        </p>
            
                        <div class='footer'>
                            <p>If you didn't request this verification, please ignore this email.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}
