using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace EdgePMO.API.Controllers
{
    [Route("api/v2.0/[controller]")]
    [ApiController]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateServices _certServices;
        public CertificatesController(ICertificateServices certServices) => _certServices = certServices;

        // This action had no [Authorize] at all and parsed the user-id claim with no
        // null-check (`User.FindFirst(...).Value` throws NullReferenceException if the
        // claim isn't present, which the global exception handler turns into a bare 500
        // with no useful message). Reproduced live: every claim attempt 500'd, for both
        // an incomplete course and a completely unenrolled one — which rules out anything
        // inside ProcessCertificateClaimAsync itself (a null enrollment there returns a
        // trivial object, it can't throw), so the crash has to happen before its first
        // query ever runs, i.e. right here. Added the same guarded pattern already used
        // successfully in CoursesController.SyncProgress.
        [HttpPost("claim/{courseId}")]
        [Authorize]
        public async Task<IActionResult> Claim(Guid courseId)
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, new Response
                {
                    IsSuccess = false,
                    Message = "Authentication required",
                    Code = HttpStatusCode.Unauthorized,
                });
            }

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
