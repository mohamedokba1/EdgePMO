using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Middlwares;
using EdgePMO.API.Models;
using EdgePMO.API.Services;
using EdgePMO.API.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using System.Text;

namespace EdgePMP.API;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<EdgepmoDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        JwtSettings? jwtSettings = builder.Configuration.GetSection("JWT").Get<JwtSettings>();

        #region Serilog Configuration
        bool enableDbLogging = builder.Configuration.GetValue<bool>("Logging:EnableDatabaseLogging");
        var columnWriters = new Dictionary<string, ColumnWriterBase>
        {
            { "Level", new LevelColumnWriter(true, NpgsqlTypes.NpgsqlDbType.Text) },
            { "Message", new RenderedMessageColumnWriter() },
            { "Exception", new ExceptionColumnWriter() },
            { "Path", new SinglePropertyColumnWriter("Path") },
            { "QueryString", new SinglePropertyColumnWriter("QueryString") },
            { "TimeStamp", new TimestampColumnWriter() }
        };

        LoggerConfiguration? loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .WriteTo.Console()
            .WriteTo.PostgreSQL(
                connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
                tableName: "Logs",
                needAutoCreateTable: true,
                columnOptions: columnWriters
            );

        if (!enableDbLogging)
        {
            loggerConfig = new LoggerConfiguration()
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName();
        }

        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog();
        #endregion

        builder.Services.Configure<VerificationSettings>(builder.Configuration.GetSection("VerificationSettings"));
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        builder.Services.Configure<ContentSettings>(builder.Configuration.GetSection("Content"));
        builder.Services.AddTransient<GlobalExceptionMiddleware>();
        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddScoped<IUserServices, UsersServices>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IVerificationService, VerificationService>();
        builder.Services.AddScoped<ICourseServices, CoursesServices>();
        builder.Services.AddScoped<IInstructorServices, InstructorsServices>();
        builder.Services.AddScoped<ITestimonialServices, TestimonialsServices>();
        builder.Services.AddScoped<IContentServices, ContentServices>();
        builder.Services.AddScoped<ICourseVideoServices, CourseVideoServices>();
        builder.Services.AddScoped<ITemplateServices, TemplatesServices>();

        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        }, typeof(Program).Assembly);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks()
                    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"), name: "localhost");


        builder.Services.AddSingleton(jwtSettings);
        builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero
            };

            // For cookies
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    string? authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader))
                        return Task.CompletedTask;

                    string? cookie = context.Request.Cookies["accessToken"];
                    if (!string.IsNullOrEmpty(cookie))
                        context.Token = cookie;

                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
                policy.RequireRole("Admin", "admin"));
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("EdgePmo", policy =>
            {
                policy.WithOrigins(
                    "https://edgepmo.com",
                    "https://www.edgepmo.com",
                    "http://localhost:4200",
                    "https://localhost:4200",
                    "http://localhost:3000"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("Content-Disposition"); // If you need to expose headers
            });
        });
        WebApplication? app = builder.Build();

        using (IServiceScope? scope = app.Services.CreateScope())
        {
            EdgepmoDbContext? db = scope.ServiceProvider.GetRequiredService<EdgepmoDbContext>();
            db.Database.Migrate();
        }
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                c.RoutePrefix = "docs";
            });
        }
        app.UseCors("EdgePmo");
        app.UseStaticFiles();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }

}
