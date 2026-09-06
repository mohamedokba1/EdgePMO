using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using System.Net;

namespace EdgePMO.API.Services
{
    public class CertificateServices : ICertificateServices
    {
        private readonly EdgepmoDbContext _context;

        public CertificateServices(EdgepmoDbContext context)
        {
            _context = context;
        }

        public async Task<Response> ProcessCertificateClaimAsync(Guid userId, Guid courseId)
        {
            Response? response = new Response();

            CourseUser? enrollment = await _context.CourseUsers
                                                   .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CourseId == courseId);

            if (enrollment == null || enrollment.Progress < 100.0)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "You must complete 100% of the course to claim a certificate.",
                    Code = HttpStatusCode.BadRequest
                };
            }

            Certificate? existingCert = await _context.Certificates
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.CertificateDescription == userId.ToString());

            if (existingCert != null)
            {
                response.IsSuccess = true;
                response.Result.Add("certificateId", existingCert.CertificateId);
                response.Code = HttpStatusCode.Conflict;
                return response;
            }

            Certificate? newCert = new Certificate
            {
                CertificateId = Guid.NewGuid(),
                CourseId = courseId,
                CertificateTitle = $"CERT-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}",
                CertificateDescription = userId.ToString()
            };

            _context.Certificates.Add(newCert);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Code = HttpStatusCode.Created;
            response.Result.Add("certificateId", newCert.CertificateId);
            return response;
        }

        public async Task<byte[]> GenerateCertificateFileAsync(Guid certificateId)
        {
            Certificate? cert = await _context.Certificates
                                              .Include(c => c.Course)
                                                  .ThenInclude(c => c.Instructor)
                                              .FirstOrDefaultAsync(c => c.CertificateId == certificateId);

            if (cert == null) return null;

            User? user = await _context.Users.FindAsync(Guid.Parse(cert.CertificateDescription));
            string studentName = $"{user?.FirstName} {user?.LastName}";
            string issuedOn = cert.IssuedAt.ToString("dd MMMM yyyy");

            Instructor? instructor = cert.Course.Instructor;
            string instructorName = instructor?.InstructorName ?? "";
            // No signature on file for this instructor yet — leave the line blank
            // rather than broken-image-icon a missing src.
            string signatureImgTag = !string.IsNullOrWhiteSpace(instructor?.SignatureImageUrl)
                ? $"<img src='{instructor.SignatureImageUrl}' class='signature-img' />"
                : "";

            LaunchOptions? options = new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            };

            using IBrowser? browser = await Puppeteer.LaunchAsync(options);
            using IPage? page = await browser.NewPageAsync();

            string htmlContent = $@"
<html>
<head>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:wght@600;700&family=Poppins:wght@400;500;600&display=swap');

    * {{ box-sizing: border-box; }}
    body {{
      margin: 0;
      padding: 28px;
      background: #F4F1EA;
      font-family: 'Poppins', sans-serif;
    }}

    .frame {{
      position: relative;
      height: calc(100vh - 56px);
      border: 3px solid #1B4F91;
      padding: 8px;
    }}
    .frame::before {{
      content: '';
      position: absolute;
      inset: 8px;
      border: 1px solid #1B4F91;
      pointer-events: none;
    }}

    .content {{
      height: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: 30px 70px;
    }}

    h1 {{
      font-family: 'Playfair Display', serif;
      font-weight: 700;
      font-size: 46px;
      color: #17181A;
      margin: 0 0 14px;
    }}
    .rule {{
      width: 340px;
      height: 3px;
      background: #C9A227;
      margin: 0 0 22px;
    }}

    .lede {{
      font-size: 17px;
      color: #6B7280;
      margin: 0 0 6px;
    }}

    .student-name {{
      font-family: 'Playfair Display', serif;
      font-weight: 700;
      font-size: 34px;
      color: #17181A;
      margin: 6px 0 18px;
    }}

    .course-name {{
      font-family: 'Poppins', sans-serif;
      font-weight: 600;
      font-size: 22px;
      color: #17181A;
      margin: 6px 0 26px;
    }}

    .blurb {{
      font-size: 13.5px;
      line-height: 1.7;
      color: #26282C;
      max-width: 780px;
      margin: 0 0 40px;
    }}

    .footer {{
      width: 100%;
      display: flex;
      align-items: flex-end;
      justify-content: space-between;
    }}

    .signature-block {{
      text-align: left;
      min-width: 220px;
    }}
    .signature-img {{
      max-height: 46px;
      max-width: 180px;
      display: block;
      margin-bottom: 4px;
    }}
    .signature-line {{
      width: 200px;
      border-bottom: 1px solid #17181A;
      margin-bottom: 6px;
      height: 22px;
    }}
    .signature-name {{
      font-weight: 700;
      font-size: 15px;
      color: #17181A;
    }}
    .signature-title {{
      font-size: 12.5px;
      color: #6B7280;
    }}

    .issued {{
      font-size: 14px;
      color: #17181A;
    }}

    .logo {{
      text-align: right;
    }}
    .logo .mark {{
      display: inline-block;
      width: 38px;
      height: 4px;
      background: #26282C;
      margin-bottom: 4px;
    }}
    .logo .word {{
      font-family: 'Poppins', sans-serif;
      font-weight: 700;
      font-size: 24px;
      color: #FF0400;
      letter-spacing: 0.5px;
    }}
  </style>
</head>
<body>
  <div class='frame'>
    <div class='content'>
      <h1>Certificate of Completion</h1>
      <div class='rule'></div>

      <p class='lede'>Presents This</p>
      <div class='student-name'>{studentName}</div>

      <p class='lede'>For the Successful Completion of the</p>
      <div class='course-name'>{cert.Course.Name}</div>

      <p class='blurb'>
        This certificate is proudly presented in recognition of the successful completion of this
        professional training program. The recipient has demonstrated dedication, commitment, and
        practical understanding of the concepts and skills covered throughout the course, supporting
        continued professional development and excellence in project environments.
      </p>

      <div class='footer'>
        <div class='signature-block'>
          {signatureImgTag}
          <div class='signature-line'></div>
          <div class='signature-name'>{instructorName}</div>
          <div class='signature-title'>Course Instructor</div>
        </div>

        <div class='issued'>Issued on: {issuedOn}</div>

        <div class='logo'>
          <span class='mark'></span>
          <div class='word'>EDGE PMO</div>
        </div>
      </div>
    </div>
  </div>
</body>
</html>";

            await page.SetContentAsync(htmlContent);
            return await page.PdfDataAsync(new PdfOptions { Landscape = true, PrintBackground = true });
        }
    }
}
