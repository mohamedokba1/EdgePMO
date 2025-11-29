using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Middlwares;
using EdgePMO.API.Models;
using EdgePMO.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        #region Seruilog Configuration
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

        var loggerConfig = new LoggerConfiguration()
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

        builder.Services.AddTransient<GlobalExceptionMiddleware>();
        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddScoped<IUserServices, UsersServices>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IVerificationService, VerificationService>();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks()
                    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"), name: "localhost");


        builder.Services.AddSingleton(jwtSettings);

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
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero
            };
        });
        WebApplication? app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                c.RoutePrefix = "docs";
            });
        }
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }

}
