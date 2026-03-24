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
                                              .FirstOrDefaultAsync(c => c.CertificateId == certificateId);

            if (cert == null) return null;

            User? user = await _context.Users.FindAsync(Guid.Parse(cert.CertificateDescription));
            string studentName = $"{user?.FirstName} {user?.LastName}";

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
                                            @import url('https://fonts.googleapis.com/css2?family=Pinyon+Script&family=Montserrat:wght@400;700&display=swap');
                                            body {{ font-family: 'Montserrat', sans-serif; text-align: center; padding: 50px; border: 20px solid #1a237e; }}
                                            .title {{ font-size: 50px; color: #1a237e; }}
                                            .name {{ font-family: 'Pinyon Script', cursive; font-size: 80px; color: #c5a059; margin: 20px 0; }}
                                            .course {{ font-size: 30px; font-weight: bold; }}
                                            .footer {{ margin-top: 100px; display: flex; justify-content: space-between; padding: 0 50px; }}
                                        </style>
                                    </head>
                                    <body>
                                        <div class='title'>Certificate of Completion</div>
                                        <p>This is to certify that</p>
                                        <div class='name'>{studentName}</div>
                                        <p>has successfully completed the course</p>
                                        <div class='course'>{cert.Course.Name}</div>
                                        <div class='footer'>
                                            <div>Date: {DateTime.UtcNow:dd/MM/yyyy}</div>
                                            <div>Verify ID: {cert.CertificateTitle}</div>
                                        </div>
                                    </body>
                                    </html>";

            await page.SetContentAsync(htmlContent);
            return await page.PdfDataAsync(new PdfOptions { Landscape = true, PrintBackground = true });
        }
    }
}
