using Clausio.Legal.API.Middleware;
using Clausio.Legal.Cache;
using Clausio.Legal.Core.Settings;
using Clausio.Legal.Infrastructure;
using Clausio.Legal.Infrastructure.Ai;
using Clausio.Legal.Infrastructure.Storage;
using Clausio.Legal.Service;
using Clausio.Legal.Service.AI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers with JSON fix
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Database
builder.Services.AddDbContext<ClausioDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Cache
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Storage
var storageRootPath = builder.Configuration["Storage:RootPath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "documents");
builder.Services.AddSingleton<IDocumentStorage>(new LocalDiskDocumentStorage(storageRootPath));

// AI
builder.Services.AddSingleton<IAiClient, AnthropicAiClient>();
builder.Services.AddSingleton<AiResponseParser>();

// Services
builder.Services.AddScoped<IAuthService,         AuthService>();
builder.Services.AddScoped<IClientService,        ClientService>();
builder.Services.AddScoped<ICaseService,          CaseService>();
builder.Services.AddScoped<IActionPlanService,    ActionPlanService>();
builder.Services.AddScoped<IContradictionService, ContradictionService>();
builder.Services.AddScoped<IDocumentService,      DocumentService>();
builder.Services.AddScoped<IHearingService,       HearingService>();
builder.Services.AddScoped<ILegalResearchService, LegalResearchService>();
builder.Services.AddScoped<ITimelineService,      TimelineService>();
builder.Services.AddScoped<IReadinessService,     ReadinessService>();
builder.Services.AddScoped<IStatsService,         StatsService>();
builder.Services.AddScoped<IAiService,            AiService>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Secret"] ??
                    throw new InvalidOperationException("Jwt:Secret is not configured")
                )),
            NameClaimType = ClaimTypes.NameIdentifier,
        };
    });
builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Middleware
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clausio Legal API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
