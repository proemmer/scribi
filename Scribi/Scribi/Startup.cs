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
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scribi.Helper;

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

            //add filters
            services.AddScoped<ActionLoggerFilterAttribute>();
            services.AddScoped<ScribiExceptionFilterAttribute>();

            //add the custom services
            services.AddSingleton<IServiceCollection>(services);
            services.AddSingleton<IRuntimeCompilerService, RuntimeCompilerService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IScriptCreatorService, ScriptCreatorService>();
            services.AddSingleton<IScriptFactoryService, ScriptFactoryService>();
            services.AddSingleton<ICyclicExecutorService, CyclicExecutorService>();

            //configure the auth
            services.AddScribiAuthentication(Configuration.GetSection("Auth")?.GetValue<string>("KeyFile"));
            
            InitializeScriptCreation(services);

            //Latebinding
            //add signal r usage
            services.AddSignalR(options =>
            {
                options.Hubs.EnableDetailedErrors = true;
            });

            //services.Replace(new ServiceDescriptor(typeof(IAssemblyLocator), typeof(ScribiAssemblyLocator),ServiceLifetime.Singleton));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(  IApplicationBuilder app, 
                                IHostingEnvironment env, 
                                ILoggerFactory loggerFactory,
                                IAuthenticationService authService,
                                IScriptFactoryService scriptFactoryService)
        {
            var globalConfig = Configuration.GetSection("Global");
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (globalConfig.GetValue("UseLogFiles", true))
            {
                loggerFactory.AddNLog();

                env.ConfigureNLog("nlog.config");
            }

            scriptFactoryService.Configure(Configuration.GetSection("ScriptFactory"));
            scriptFactoryService.Init();

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

            if (globalConfig.GetValue("UseSignalR", true))
            {
                if (globalConfig.GetValue("UseWebSockets", true))
                    app.UseWebSockets();
                app.UseSignalR();
            }

            app.UseMvc();

            if (globalConfig.GetValue("UseSwagger", true))
            {
                app.UseSwagger();
                app.UseSwaggerUi();
            }
        }

        private void InitializeScriptCreation(IServiceCollection services)
        {
            services.AddSingleton<IAssemblyLocator, ScribiAssemblyLocator>();
            var serviceProvider = services.BuildServiceProvider();
            //services.Replace(new ServiceDescriptor(typeof(IAssemblyLocator), typeof(ScribiAssemblyLocator), ServiceLifetime.Singleton));
            

            var runtimeCompilerService = serviceProvider.GetService<IRuntimeCompilerService>();
            runtimeCompilerService.Configure(Configuration.GetSection("RuntimeCompiler"));
            runtimeCompilerService.Init();

            var scriptCreatorService = serviceProvider.GetService<IScriptCreatorService>();
            scriptCreatorService.Configure(Configuration.GetSection("ScriptCreator"));
            scriptCreatorService.Init();
        }
    }
}
