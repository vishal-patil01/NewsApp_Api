using NewsApp.API.Extensions;
using NewsApp.Models.Configurations;
using Serilog;
using Serilog.Events;

#region Configuring builder
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false;
});
builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
             .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
             .AddEnvironmentVariables();
#endregion

#region Add Services to the Container.
AppSettings.ConfigurationSettings = builder.Configuration.GetSection("ConfigurationSettings").Get<ConfigurationSettings>();
builder.Services.SetupDependency(builder.Configuration);

//Logging Configuration
builder.Logging.ClearProviders();
builder.Host.UseSerilog((ctx, lc) => lc
                 .WriteTo.Logger(lc => lc
                     .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Error)
                     .WriteTo.File("logs/Error_.txt", LogEventLevel.Error, rollingInterval: RollingInterval.Day))
                 .WriteTo.Logger(lc => lc
                         .Filter.ByIncludingOnly(evt => evt.Level <= LogEventLevel.Fatal)
                         .WriteTo.File("logs/logs_.txt", rollingInterval: RollingInterval.Day)));
#endregion

#region Configure the HTTP request pipeline.
WebApplication app = builder.Build();
app.ConfigureMiddlewares();
app.Run();
#endregion
