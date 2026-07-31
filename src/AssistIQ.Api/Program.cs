using System.Text;
using AssistIQ.Api.Auth;
using AssistIQ.Api.Errors;
using AssistIQ.Api.Middleware;
using AssistIQ.Api.Security;
using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Analytics;
using AssistIQ.Application.AuditLogs;
using AssistIQ.Application.Auth;
using AssistIQ.Application.Drafts;
using AssistIQ.Application.Knowledge;
using AssistIQ.Application.Tickets;
using AssistIQ.Application.UsageLogs;
using AssistIQ.Infrastructure.Ai;
using AssistIQ.Infrastructure.Auth;
using AssistIQ.Infrastructure.Persistence;
using AssistIQ.Infrastructure.Persistence.Seed;
using AssistIQ.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ApiErrorResponseFactory.CreateValidationError;
    });
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddAssistIQRateLimiting(builder.Configuration);
builder.AddAssistIQRequestInputSecurity();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000", "http://localhost:5173"];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AssistIQDbContext>("database");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};Database={uri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SslMode=Prefer";
}

builder.Services.AddDbContext<AssistIQDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<UsageCostOptions>(builder.Configuration.GetSection("UsageCost"));

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ISystemClock, SystemClock>();
builder.Services.AddScoped<IKnowledgeIndexer, FakeKnowledgeIndexer>();
builder.Services.AddScoped<IRetrievalService, FakeRetrievalService>();
builder.Services.AddAssistIQAi(builder.Configuration);
builder.Services.AddScoped<IUsageRecorder, UsageRecorder>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IKnowledgeDocumentRepository, KnowledgeDocumentRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IDraftRepository, DraftRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IUsageLogRepository, UsageLogRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<KnowledgeDocumentService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<DraftService>();
builder.Services.AddScoped<AuditLogQueryService>();
builder.Services.AddScoped<UsageLogQueryService>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<DemoDataSeeder>();
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options => options.AddAssistIQPolicies());

var app = builder.Build();

ProductionSecurityValidator.Validate(app.Configuration, app.Environment.IsProduction());

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseMiddleware<RequestBodySizeLimitMiddleware>();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/ready");

if (app.Configuration.GetValue<bool>("ApplyMigrationsOnStartup") ||
    app.Configuration.GetValue<bool>("SeedDemoDataOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AssistIQDbContext>();

    if (app.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
    {
        await dbContext.Database.MigrateAsync();
    }

    if (app.Configuration.GetValue<bool>("SeedDemoDataOnStartup"))
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedAsync();
    }
}

app.Run();

public partial class Program;
