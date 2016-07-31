using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scribi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scribi.CodeGeneration
{
    public static class CodeGenerationExtension
    {
        public static void AddCodeGeneration(this IServiceCollection services, IConfigurationRoot config)
        {
            services.AddSingleton<IRuntimeCompilerService, RuntimeCompilerService>();
            services.AddSingleton<IScriptCreatorService, ScriptCreatorService>();
            services.AddSingleton<IAssemblyLocator, ScribiAssemblyLocator>();
            var serviceProvider = services.BuildServiceProvider();

            var runtimeCompilerService = serviceProvider.GetService<IRuntimeCompilerService>();
            runtimeCompilerService.Configure(config.GetSection("RuntimeCompiler"));
            runtimeCompilerService.Init();

            var scriptCreatorService = serviceProvider.GetService<IScriptCreatorService>();
            scriptCreatorService.Configure(config.GetSection("ScriptCreator"));
            scriptCreatorService.Init();
        }
    }
}
