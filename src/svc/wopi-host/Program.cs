using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var jwt = builder.Configuration.GetSection("Authentication:Jwt");
var issuer = jwt["Issuer"]!.TrimEnd('/');
var audience = jwt["Audience"]!.TrimEnd('/');



builder.Services.AddOptions<JwtConsumerConfig> ()
    .Bind(builder.Configuration.GetSection("Authentication:Jwt"))
    .Validate(config =>
    {
        return !string.IsNullOrWhiteSpace(config.Issuer) &&
               !string.IsNullOrWhiteSpace(config.Audience);
    }, "Invalid JWT configuration")
    .ValidateOnStart();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = issuer;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.FromSeconds(30),

        ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },

        //Map of OIDC-standard claim names to ASP-NET claim names
        NameClaimType = JwtRegisteredClaimNames.PreferredUsername,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "WopiHost API", Version = "v1" });
    s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Paste bearer token, excluding 'Bearer ' prefix",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    s.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>() //Empty list as no permission scopes needed for JWT auth
        }
    });
});


builder.Services.AddAuthorization();
// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; //sørger for at vi skriver i Pascal-case til wopi client
    });

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Register the typed HttpClient for communicating with the Storage microservice
builder.Services.AddHttpClient<IStorageClient, StorageClient>(client =>
{
    // Base URL of your Storage service (set in appsettings.json or environment variable)
    client.BaseAddress = new Uri(builder.Configuration["Services:Storage:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
