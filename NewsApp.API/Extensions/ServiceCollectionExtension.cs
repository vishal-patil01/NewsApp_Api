using Microsoft.AspNetCore.ResponseCompression;
using NewsApp.API.Middlewares;
using NewsApp.Services.Implementation;
using NewsApp.Services.Interface;
using Newtonsoft.Json;


namespace NewsApp.API.Extensions
{
    /// <summary>
    /// Contains extension methods for setting up dependency injection and configuring middlewares.
    /// </summary>
    public static class ServiceExtension
    {
        /// <summary>
        /// Configures dependency injection services including database context, repositories, services, response compression, controllers, and Swagger.
        /// </summary>
        /// <param name="serviceCollection">The collection of service descriptors to configure.</param>
        /// <param name="configuration">The application configuration properties.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
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


            // Add response compression extension.
            serviceCollection.AddResponseCompression(opt =>
                             {
                                 opt.EnableForHttps = true;
                                 opt.Providers.Add<GzipCompressionProvider>();
                             });
            serviceCollection.AddHttpClient();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddHttpContextAccessor();
            serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            serviceCollection.AddSingleton<INewsService, NewsService>();
            serviceCollection.AddSingleton<IMemoryCacheWrapper, MemoryCacheWrapper>();
            serviceCollection.AddSwaggerGen();

            return serviceCollection;
        }
        /// <summary>
        /// Configures the application's HTTP request pipeline with Swagger, security headers, compression, exception handling, CORS, and HTTPS redirection.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/> instance.</returns>
        public static IApplicationBuilder ConfigureMiddlewares(this WebApplication app)
        {
            // Configure the HTTP request pipeline.

            // Swagger is enabled for API documentation.
            app.UseSwagger();
            app.UseSwaggerUI();

            // Enables response compression.
            app.UseResponseCompression();
            // Applies global exception handling middleware.
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            // Redirects HTTP requests to HTTPS.
            app.UseHttpsRedirection();

            // Maps controller endpoints.
            app.MapControllers();

            // Configures Cross-Origin Resource Sharing to allow any origin, header, and method.
            app.UseCors(x =>
            {
                x.AllowAnyHeader();
                x.AllowAnyMethod();
                x.AllowAnyOrigin();
            });
            return app;
        }
    }
}
