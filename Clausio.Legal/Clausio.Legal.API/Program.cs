using System.Text;
using Clausio.Legal.Cache;
using Clausio.Legal.Core.Settings;
using Clausio.Legal.Infrastructure;
using Clausio.Legal.Infrastructure.Ai;
using Clausio.Legal.Infrastructure.Storage;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

builder.Services.AddDbContext<ClausioDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

var storageRootPath = builder.Configuration["Storage:RootPath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "documents");
builder.Services.AddSingleton<IDocumentStorage>(new LocalDiskDocumentStorage(storageRootPath));

builder.Services.AddSingleton<IAiClient, AnthropicAiClient>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<IActionPlanService, ActionPlanService>();
builder.Services.AddScoped<IContradictionService, ContradictionService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IHearingService, HearingService>();
builder.Services.AddScoped<ILegalResearchService, LegalResearchService>();
builder.Services.AddScoped<ITimelineService, TimelineService>();
builder.Services.AddScoped<IReadinessService, ReadinessService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IAiService, AiService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = feature?.Error;
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception is InvalidOperationException
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { message = exception?.Message ?? "An unexpected error occurred." });
    });
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
