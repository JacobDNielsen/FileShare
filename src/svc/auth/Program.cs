using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Auth.Services;
using Auth.Interfaces;
using Auth.Models;
using Auth.Data;
using Auth.Repository;



var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var keyPath = "/run/secrets/auth-jwt-key";

if (File.Exists(keyPath))
{
    var key = File.ReadAllText(keyPath);
    builder.Configuration["Authentication:Jwt:SigningKeys:0:PrivateKey"] = key;
}

builder.Services.AddOptions<JwtConfig>()
    .Bind(builder.Configuration.GetSection("Authentication:Jwt"))
    .Validate(config =>
    {
        return !string.IsNullOrWhiteSpace(config.Issuer) &&
               !string.IsNullOrWhiteSpace(config.Audience) &&
                !string.IsNullOrWhiteSpace(config.LatestKeyId) &&
                (config.SigningKeys?.Count ?? 0) > 0 &&
               config.ExpiresMinutes > 0;
    }, "Invalid JWT configuration")
    .ValidateOnStart();
builder.Services.AddSingleton<JwtSigningKeyStore>();

builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
});

builder.Services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();

builder.Services.AddAuthorization();
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

    var retries = 10;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch
        {
            retries--;
            Thread.Sleep(5000);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/debug/authconfig", (IConfiguration config) =>
    {
        return Results.Json(config.GetSection("Authentication").AsEnumerable());
    })
    .WithTags("DebugAuthConfiguration");
} else
{
    app.UseHttpsRedirection();
}

app.MapGet("/", () => Results.Redirect("/swagger/index.html"))
    .WithTags("RootRedirect");

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();