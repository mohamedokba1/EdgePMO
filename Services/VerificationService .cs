using EdgePMO.API.Contracts;
using EdgePMO.API.Settings;
using Microsoft.Extensions.Options;

namespace EdgePMO.API.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly VerificationSettings _settings;

        public VerificationService(IOptions<VerificationSettings> options)
        {
            _settings = options.Value;
        }

        public string GenerateVerificationToken()
        {
            Random? random = new Random();
            return random.Next(0, 999999).ToString("D6");
        }

        public DateTime GetExpiry()
        {
            return DateTime.UtcNow.AddMinutes(_settings.TokenExpiryMinutes).ToLocalTime();
        }
    }
}
