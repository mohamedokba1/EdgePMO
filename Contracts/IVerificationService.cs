namespace EdgePMO.API.Contracts
{
    public interface IVerificationService
    {
        string GenerateVerificationToken();
        DateTime GetExpiry();
    }
}
