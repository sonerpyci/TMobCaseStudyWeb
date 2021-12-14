using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;


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

            services.AddEndpointsApiExplorer();
            ConfigureSwagger(services);
            
            
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
            services.AddResponseCompression();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
        private static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "Mars Rover",
                    Description = "TMob Case Study",
                    Contact = new OpenApiContact()
                    {
                        Name = "Soner Paycı",
                        Email = "sonerpayci@gmail.com"
                    },
                });
                options.DescribeAllParametersInCamelCase();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
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
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseResponseCompression();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
