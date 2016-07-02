using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Scribi.Swagger;
using Scribi.Filters;
using Scribi.Services;
using Scribi.Auth;
using Scribi.Interfaces;
using NLog.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Scribi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc()
                    //.AddControllersAsServices()
                    .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options =>
            {
                //options.SingleApiVersion(new Swashbuckle.SwaggerGen.Generator.Info
                //{
                //    Version = "v1",
                //    Title = "webpac",
                //    Description = "a api to access plc data in a structured way"
                //});
                options.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
            });

            //services.AddTransient<IControllerActivator, ServiceBasedControllerActivator>();

            //add filters
            services.AddScoped<ActionLoggerFilterAttribute>();
            services.AddScoped<ScribiExceptionFilterAttribute>();

            //add the custom services
            services.AddSingleton<IServiceCollection>(services);
            services.AddSingleton<IRuntimeCompilerService, RuntimeCompilerService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IControllerCreatorService, ControllerCreatorService>();

            //configure the auth
            services.AddScribiAuthentication(Configuration.GetSection("Auth")?.GetValue<string>("KeyFile"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(  IApplicationBuilder app, 
                                IHostingEnvironment env, 
                                ILoggerFactory loggerFactory,
                                IRuntimeCompilerService runtimeCompilerService,
                                IAuthenticationService authService,
                                IControllerCreatorService controllerCreatorService)
        {
            var globalConfig = Configuration.GetSection("Global");
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (globalConfig.GetValue("UseLogFiles", true))
            {
                loggerFactory.AddNLog();

                env.ConfigureNLog("nlog.config");
            }

            runtimeCompilerService.Configure(Configuration.GetSection("RuntimeCompiler"));
            runtimeCompilerService.Init();

            controllerCreatorService.Configure(Configuration.GetSection("ControllerCreator"));
            controllerCreatorService.Init();


            app.UseScribiAuth();

            app.UseCors(options =>
            {
                options.AllowAnyHeader();
                options.AllowAnyMethod();
                options.AllowAnyOrigin();
                options.AllowCredentials();
            });

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMvc();

            if (globalConfig.GetValue("UseSwagger", true))
            {
                app.UseSwagger();
                app.UseSwaggerUi();
            }
        }
    }
}
