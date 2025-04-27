using NewsApp.Services.Interface;
using NewsApp.Services.Implementation;
using NewsApp.API.Middlewares;
using Microsoft.AspNetCore.ResponseCompression;
using Newtonsoft.Json;


namespace NewsApp.API.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection SetupDependency(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddControllers()
              .AddJsonOptions(jsonOptions =>
              {
                  jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
              })
              .AddNewtonsoftJson(options =>
              {
                  options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                  options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
              });

            // add service extensions
            serviceCollection.AddResponseCompression(opt =>
                             {
                                 opt.EnableForHttps = true;
                                 opt.Providers.Add<GzipCompressionProvider>();
                             });
            serviceCollection.AddHttpClient();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddHttpContextAccessor();
            serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            serviceCollection.AddSingleton<INewsService,NewsService>();
            serviceCollection.AddSingleton<IMemoryCacheWrapper,MemoryCacheWrapper>();
            serviceCollection.AddSwaggerGen();

            return serviceCollection;
        }

        public static IApplicationBuilder ConfigureMiddlewares(this WebApplication app)
        {
            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("dev"))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseSecurityHeader();
            app.UseResponseCompression();
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseHttpsRedirection();


            app.UseCors(x =>
            {
                x.AllowAnyOrigin();
                x.AllowAnyMethod();
                x.AllowAnyHeader();
            });
            app.MapControllers();

            return app;
        }
    }
}
