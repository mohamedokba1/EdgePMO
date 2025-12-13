using AutoMapper;
using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Services
{
    public class UsersServices : IUserServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IVerificationService _verificationService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        public UsersServices(EdgepmoDbContext context, ITokenService tokenService, IVerificationService verificationService, IEmailService emailService, IMapper mapper)
        {
            _context = context;
            _tokenService = tokenService;
            _verificationService = verificationService;
            _emailService = emailService;
            _mapper = mapper;
        }


        public async Task<Response> GetAllUsersAsync()
        {
            Response response = new Response();
            List<User>? users = await _context.Users
                                .AsNoTracking()
                                .OrderByDescending(u => u.CreatedAt)
                                .ToListAsync();

            response.IsSuccess = true;
            response.Message = "Users retrieved successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("users", JsonSerializer.SerializeToNode(_mapper.Map<IEnumerable<UserReadDto>>(users)));
            return response;
        }

        public async Task<Response> EmailVerification(VerifyEmailDto dto)
        {
            Response response = new Response();
            User? user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = "Invalid request.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            if (user.EmailVerified)
            {
                response.IsSuccess = false;
                response.Message = "Email already verified.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            if (user.EmailVerificationToken != dto.Code)
            {
                response.IsSuccess = false;
                response.Message = "Invalid verification code.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            if (user.EmailVerificationExpiresAt < DateTime.UtcNow)
            {
                response.IsSuccess = false;
                response.Message = "Verification code expired.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationExpiresAt = null;

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Email verified successfully.";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        public async Task<Response> Login(LoginDto dto)
        {
            Response response = new Response();
            User? user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == dto.Email.ToLower());

            if (user == null || (user.IsActive.HasValue && !user.IsActive.Value))
            {
                response.IsSuccess = false;
                response.Message = "Invalid email or password";
                response.Code = HttpStatusCode.Unauthorized;
                return response;
            }

            if (!user.EmailVerified)
            {
                response.IsSuccess = false;
                response.Message = "Please verify your email before logging in.";
                response.Code = HttpStatusCode.Unauthorized;
                return response;
            }

            bool isValid = PasswordHasher.Verify(dto.Password, user.PasswordHash, user.PasswordSalt);

            if (!isValid)
            {
                response.IsSuccess = false;
                response.Message = "Invalid email or password";
                response.Code = HttpStatusCode.Unauthorized;
                return response;
            }

            string accessToken = _tokenService.GenerateAccessToken(user);
            RefreshToken? refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenCreatedAt = refreshToken.CreatedAt;
            user.RefreshTokenExpiresAt = refreshToken.ExpiresAt;
            user.RefreshTokenRevokedAt = null;
            user.UpdatedAt = DateTime.Now.ToLocalTime();

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Login successfully";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("accessToken", accessToken);
            response.Result.Add("refreshToken", refreshToken.Token);
            response.Result.Add("expiresAt", DateTime.UtcNow.AddMinutes(15).ToLocalTime());

            return response;
        }

        public async Task<Response> Logout(Guid userId)
        {
            Response response = new Response();
            User? user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                user.RefreshTokenRevokedAt = DateTime.UtcNow.ToLocalTime();
                user.RefreshToken = null;
                user.UpdatedAt = DateTime.Now.ToLocalTime();
            }
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Logout successful";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        public async Task<Response> Refresh(string refreshToken)
        {
            Response response = new Response();
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiresAt < DateTime.UtcNow || user.RefreshTokenRevokedAt != null)
            {
                response.IsSuccess = false;
                response.Message = "Invalid refresh token";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            RefreshToken? newRefreshToken = _tokenService.GenerateRefreshToken();
            string? accessToken = _tokenService.GenerateAccessToken(user);

            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenCreatedAt = newRefreshToken.CreatedAt;
            user.RefreshTokenExpiresAt = newRefreshToken.ExpiresAt;

            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Token refreshed successfully";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("accessToken", accessToken);
            response.Result.Add("refreshToken", newRefreshToken.Token);
            response.Result.Add("expiresAt", DateTime.UtcNow.AddMinutes(15));
            return response;
        }

        public async Task<Response> Register(RegisterUserDto dto, bool isAdmin = false)
        {
            Response response = new Response();
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            {
                response.IsSuccess = false;
                response.Message = "Email already registered";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }
            byte[]? salt = PasswordHasher.GenerateSalt();
            string? hashedPassword = PasswordHasher.Hash(dto.Password, salt);

            User? user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email.ToLower(),
                Role = isAdmin ? "admin" : "user",
                PhoneNo_1 = dto.Phone1,
                PhoneNo_2 = dto.Phone2,
                PasswordSalt = salt,
                IsAdmin = isAdmin,
                LastCompnay = dto.LastCompany,
                PasswordHash = hashedPassword,
                EmailVerified = false
            };

            _context.Users.Add(user);
            int affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                response.IsSuccess = true;
                response.Message = "User registered successfully";
                response.Code = HttpStatusCode.Created;
                return response;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Registration failed";
                response.Code = HttpStatusCode.InternalServerError;
                return response;
            }
        }

        public async Task<bool> ResetPasswordAsync(PasswordResetDto dto)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return false;

            PasswordResetToken? tokenEntry = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed && t.Expiration > DateTime.UtcNow)
                .OrderByDescending(t => t.Expiration)
                .FirstOrDefaultAsync();

            if (tokenEntry == null || tokenEntry.Token != dto.VerificationCode)
                return false;

            byte[]? salt = PasswordHasher.GenerateSalt();
            string? hashedPassword = PasswordHasher.Hash(dto.NewPassword, salt);

            user.PasswordHash = hashedPassword;
            user.PasswordSalt = salt;
            tokenEntry.IsUsed = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Response> SendPasswordResetTokenAsync(string email)
        {
            Response response = new Response();
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                response.IsSuccess = true;
                response.Message = "If the email is registered, a verification code has been sent.";
                response.Code = HttpStatusCode.OK;
                return response;
            }
            string? token = _verificationService.GenerateVerificationToken();

            PasswordResetToken? resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(15)
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailVerficationAsync(user.Email, "Password Reset Verification", token);

            response.IsSuccess = true;
            response.Message = "If the email is registered, a verification code has been sent.";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        public async Task<Response> SendVerificationMail(VerifyRequestDto request, string subject)
        {
            Response response = new Response();
            User? user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = "User not found";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            string? token = _verificationService.GenerateVerificationToken();

            user.EmailVerificationToken = token;
            user.EmailVerificationExpiresAt = _verificationService.GetExpiry();
            user.UpdatedAt = DateTime.Now.ToLocalTime();
            await _context.SaveChangesAsync();

            await _emailService.SendEmailVerficationAsync(user.Email, subject, token);

            response.IsSuccess = true;
            response.Message = "Verification code sent";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        public async Task<Response> GetProfileAsync(Guid? userId, string? email)
        {
            Response response = new Response();

            IQueryable<User> query = _context.Users
                .AsNoTracking()
                .Include(u => u.CourseUsers)
                    .ThenInclude(cu => cu.Course)
                        .ThenInclude(c => c.Instructor)
                .Include(u => u.CourseUsers)
                    .ThenInclude(cu => cu.Course)
                .Include(u => u.UserTemplates)
                    .ThenInclude(ut => ut.Template);

            User? user = null;

            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                user = await query.FirstOrDefaultAsync(u => u.Id == userId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(email))
            {
                string emailLower = email.Trim().ToLowerInvariant();
                user = await query.FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower);
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Either id or email must be provided.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = "User not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            UserReadDto? userDto = _mapper.Map<UserReadDto>(user);

            List<Course>? courses = user.CourseUsers?
                .Where(cu => cu.Course != null && cu.Course.CourseOutline != null && cu.Course.CourseOutline.Any())
                .Select(cu => cu.Course)
                .Distinct()
                .ToList() ?? new List<Course>();

            IEnumerable<CourseReadDto>? courseDtos = _mapper.Map<IEnumerable<CourseReadDto>>(courses);

            List<UserTemplateReadDto>? templates = _mapper
                                                .Map<IEnumerable<UserTemplateReadDto>>(user.UserTemplates ?? Enumerable.Empty<UserTemplate>())
                                                .ToList();

            response.IsSuccess = true;
            response.Message = "User profile retrieved.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("user", JsonSerializer.SerializeToNode(userDto) ?? JsonValue.Create(new { }));
            response.Result.Add("courses", JsonSerializer.SerializeToNode(courseDtos) ?? JsonValue.Create(Array.Empty<object>()));
            response.Result.Add("templates", JsonSerializer.SerializeToNode(templates) ?? JsonValue.Create(Array.Empty<object>()));

            return response;
        }
    }
}
