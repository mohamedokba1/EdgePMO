using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EdgePMO.API.Controllers
{
    [Route("api/v2.0/[controller]")]
    [ApiController]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateServices _certServices;
        public CertificatesController(ICertificateServices certServices) => _certServices = certServices;

        [HttpPost("claim/{courseId}")]
        public async Task<IActionResult> Claim(Guid courseId)
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Response? res = await _certServices.ProcessCertificateClaimAsync(userId, courseId);
            return StatusCode((int)res.Code, res);
        }

        [HttpGet("download/{certificateId}")]
        public async Task<IActionResult> Download(Guid certificateId)
        {
            byte[]? bytes = await _certServices.GenerateCertificateFileAsync(certificateId);
            if (bytes == null) return NotFound();
            return File(bytes, "application/pdf", "Certificate.pdf");
        }
    }
}
