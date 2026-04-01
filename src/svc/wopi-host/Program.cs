using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
//using WopiHost.StorageClient;

var builder = WebApplication.CreateBuilder(args);

var mtlsEnabled = builder.Configuration.GetValue<bool>("Mtls:Enabled");
var allowStartupWithoutClientCertInDevelopment = builder.Configuration.GetValue<bool>("Mtls:AllowStartupWithoutClientCertInDevelopment");
X509Certificate2? clientCert = null;
if (mtlsEnabled)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ConfigureHttpsDefaults(https =>
            https.ClientCertificateMode = ClientCertificateMode.AllowCertificate);
    });
    var certPath = builder.Configuration["Mtls:ClientCertPath"]!;
    var certPassword = builder.Configuration["Mtls:ClientCertPassword"]!;
    try
    {
        clientCert = X509CertificateLoader.LoadPkcs12FromFile(certPath, certPassword, X509KeyStorageFlags.EphemeralKeySet);
    }
    catch (Exception ex) when (builder.Environment.IsDevelopment() && allowStartupWithoutClientCertInDevelopment)
    {
        Console.WriteLine($"[WARN] Could not load mTLS client certificate from '{certPath}'. Continuing without outbound client certificate in Development. {ex.GetType().Name}: {ex.Message}");
    }
}
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
})
.AddCertificate(options =>
{
    options.AllowedCertificateTypes = CertificateTypes.Chained;
    options.RevocationMode = X509RevocationMode.NoCheck;
})
.AddCertificateCache();

var wopiGatewayPrefix = builder.Configuration["SwaggerGatewayPrefix"];
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "WopiHost API", Version = "v1" });
    s.AddServer(new OpenApiServer { Url = "/" });
    if (!string.IsNullOrEmpty(wopiGatewayPrefix))
        s.AddServer(new OpenApiServer { Url = wopiGatewayPrefix });
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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Register the typed HttpClient for communicating with the Storage microservice
builder.Services.AddHttpClient<IStorageClient, StorageClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Storage:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (clientCert != null) handler.ClientCertificates.Add(clientCert);
    return handler;
});

builder.Services.AddHttpClient<ILockClient, LockClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:LockManager:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (clientCert != null) handler.ClientCertificates.Add(clientCert);
    return handler;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} else
{
    app.UseHttpsRedirection();
}

app.MapGet("/", () => Results.Redirect("swagger/index.html"))
    .WithTags("RootRedirect");

if (mtlsEnabled)
{
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value ?? "";
        var exempt = path == "/" || path.StartsWith("/swagger");
        if (!exempt)
        {
            var result = await context.AuthenticateAsync(CertificateAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }
        await next(context);
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseCors();
app.Run();
