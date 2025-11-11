using System.Data.SqlTypes;
using Storage.Services;
using Storage.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Storage.Repositories;
using Storage.FileStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WopiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IFileStorage, FileStorage>();
builder.Services.AddScoped<IFileService, FileService>();
//builder.Services.AddScoped<IFileStorage>(_ => new FileStorage());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "WopiHost API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

app.Run();

//builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddScoped<FileService>();
