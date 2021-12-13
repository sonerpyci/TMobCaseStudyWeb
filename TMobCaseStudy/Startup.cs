using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IO.Compression;
using System.Text.Json.Serialization;

namespace TMobCaseStudy.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConfiguration(_configuration.GetSection("Logging"));
            });
            
            services.AddControllers(options =>
            {
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
            services.AddResponseCompression();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        



        public void Configure(
            IApplicationBuilder app,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory
             )
        {
            if (env.EnvironmentName == "Local")
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseResponseCompression();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
