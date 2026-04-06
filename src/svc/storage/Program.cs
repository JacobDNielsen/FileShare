using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Storage.Services;
using Storage.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using Storage.FileStorage;
using Storage.Repositories;
using System.ComponentModel;
using FileShareApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var clientCert = MtlsExtensions.LoadMtlsClientCert(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddDbContext<WopiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    options.RequireHttpsMetadata = issuer.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
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
    // Use client cert for OIDC discovery / JWKS fetch when on mTLS issuer
    if (clientCert is not null)
        options.BackchannelHttpHandler = new HttpClientHandler
            { ClientCertificates = { clientCert } };
});

var storageGatewayPrefix = builder.Configuration["SwaggerGatewayPrefix"];
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "Storage API", Version = "v1" });
    s.AddServer(new OpenApiServer { Url = "/" });
    if (!string.IsNullOrEmpty(storageGatewayPrefix))
        s.AddServer(new OpenApiServer { Url = storageGatewayPrefix });
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

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IFileStorage, FileStorage>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IFileService, FileService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WopiDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/debug/mtls", (HttpContext ctx) =>
    {
        var cert = ctx.Connection.ClientCertificate;
        return Results.Ok(new
        {
            port = ctx.Connection.LocalPort,
            clientCert = cert == null ? null : (object)new
            {
                subject    = cert.Subject,
                thumbprint = cert.Thumbprint,
                notAfter   = cert.NotAfter
            }
        });
    }).AllowAnonymous().WithTags("Debug");
} else
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("swagger/index.html"))
    .WithTags("RootRedirect");


app.Run();

//builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddScoped<FileService>();
