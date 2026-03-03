  using Ocelot.DependencyInjection;
  using Ocelot.Middleware;

  var builder = WebApplication.CreateBuilder(args);

  //Ocelot configuration
  builder.Configuration
      .SetBasePath(builder.Environment.ContentRootPath)
      .AddOcelot(); // is using single ocelot.json, using read-only mode
  builder.Services
      .AddOcelot(builder.Configuration);

  if (builder.Environment.IsDevelopment())
  {
      builder.Logging.AddConsole();
  }

  var app = builder.Build();
  await app.UseOcelot();
  await app.RunAsync();