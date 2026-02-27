using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OptimizationHeuristics.Api.Middleware;
using OptimizationHeuristics.Api.Services;
using OptimizationHeuristics.Api.Validators;
using OptimizationHeuristics.Core.Services;
using OptimizationHeuristics.Infrastructure.Data;
using OptimizationHeuristics.Infrastructure.Repositories;
using OptimizationHeuristics.Infrastructure.Services;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, config) =>
    config.ReadFrom.Configuration(ctx.Configuration)
          .ReadFrom.Services(services)
          .WriteTo.Console(new CompactJsonFormatter()));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProblemDefinitionService, ProblemDefinitionService>();
builder.Services.AddScoped<IAlgorithmConfigurationService, AlgorithmConfigurationService>();
builder.Services.AddScoped<IOptimizationService, OptimizationService>();
builder.Services.AddSingleton<IRunProgressStore, RunProgressStore>();

// Auth services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService>(sp => new AuthService(
    sp.GetRequiredService<IUnitOfWork>(),
    sp.GetRequiredService<ITokenService>(),
    sp.GetRequiredService<IPasswordHasher>(),
    int.Parse(builder.Configuration["Jwt:RefreshTokenExpiryDays"] ?? "7")));

// JWT authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKeyBase64 = jwtSection["SecretKey"] ?? Convert.ToBase64String(new byte[32]);
if (!builder.Environment.IsDevelopment())
{
    var keyBytes = Convert.FromBase64String(secretKeyBase64);
    if (keyBytes.All(b => b == 0) || keyBytes.Length < 32)
        throw new InvalidOperationException("JWT SecretKey must be configured with a strong key (>= 32 bytes) in non-Development environments.");
}
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(secretKeyBase64)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProblemDefinitionValidator>();

var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var path = httpContext.Request.Path;
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("IsHealthCheck", path.StartsWithSegments("/health"));
        diagnosticContext.Set("ProbeType", path.Value switch
        {
            "/health/live"  => "liveness",
            "/health/ready" => "readiness",
            _               => null
        });
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var progressStore = app.Services.GetRequiredService<IRunProgressStore>();
lifetime.ApplicationStopping.Register(() => progressStore.CancelAll());

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.MapGet("/health/live", () =>
    Results.Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow }));

app.MapGet("/health/ready", async (ApplicationDbContext db) =>
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
    var canConnect = await db.Database.CanConnectAsync(cts.Token);
    return canConnect
        ? Results.Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow })
        : Results.Json(new { Status = "Unavailable", Timestamp = DateTime.UtcNow }, statusCode: 503);
});

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Failed to initialize database on startup. The application will continue but database operations may fail.");
}

app.Run();

public partial class Program { }
