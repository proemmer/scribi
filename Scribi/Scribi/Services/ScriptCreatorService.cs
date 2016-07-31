using Microsoft.Extensions.Logging;
using Scribi.Attributes;
using Scribi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Threading;
using Scribi.Helper;
using Scribi.CodeGeneration;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace Scribi.Services
{
    public class ScriptCreatorService : IScriptCreatorService
    {

        private readonly ReaderWriterLockSlim _serviceLock = new ReaderWriterLockSlim();
        private readonly ILogger _logger;
        private readonly IRuntimeCompilerService _compiler;
        private readonly IServiceCollection _services;
        private bool _servicesChanged = false;
        private readonly ApplicationPartManager _apm;
        private IServiceProvider _serviceProvider;
        private readonly ScribiAssemblyLocator _locator;


        public IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null || _servicesChanged)
                {
                    using (var guard = new UpgradeableGuard(_serviceLock))
                    {
                        if (_serviceProvider == null || _servicesChanged)
                        {
                            using (var writerGuard = guard.UpgradeToWriterLock())
                            {
                                _serviceProvider = _services.BuildServiceProvider();
                                _servicesChanged = false;
                            }
                        }
                    }
                }
                return _serviceProvider;
            }
        }
        public List<Type> Scripts { get; private set; } = new List<Type>();

        public ScriptCreatorService(ILogger<ScriptCreatorService> logger,
                                        IRuntimeCompilerService compiler,
                                        ApplicationPartManager apm,
                                        IServiceCollection services,
                                        IAssemblyLocator assemblyLocator)
        {
            _logger = logger;
            _compiler = compiler;
            _services = services;
            _apm = apm;
            _locator = assemblyLocator as ScribiAssemblyLocator;
        }

        #region IService Interface
        public void Configure(IConfigurationSection config)
        {
        }

        public void Init()
        {
            Bootstrap(_compiler.GetAssemblies(), _compiler.GetTypes());
        }

        public void Release()
        {
            //Release all
        }
        #endregion

        public void Bootstrap(IEnumerable<Assembly> assemblies, IEnumerable<Type> types)
        {
            var generatedServices = new List<string>();
            var clientProxiesToGenerate = new Dictionary<string,Type>();
            foreach (var type in types)
            {
                var ti = type.GetTypeInfo();
                var attr = ti.GetCustomAttribute<ScriptUnitAttribute>();
                if (attr != null)
                {
                    Scripts.Add(type);
                    if (attr.AccessType == AccessType.Rest || attr.AccessType == AccessType.Remote)
                    {
                        generatedServices.Add(ControllerCreator.Create(type, attr));
                        _logger.LogInformation($"WebApi Controller for type {type} was generated.");
                    }

                    if (attr.AccessType == AccessType.SignalR || attr.AccessType == AccessType.Remote)
                    {
                        var result = HubCreator.Create(type, attr);
                        generatedServices.Add(result.Item2);
                        _logger.LogInformation($"SignalR Hub for type {type} was generated.");
                        if (attr.ClientInterface != null)
                        {
                            //var ifType = typeof(IClientProxy<>).MakeGenericType(attr.ClientInterface);
                            clientProxiesToGenerate.Add(result.Item1, attr.ClientInterface);
                        }
                    }

                    switch (attr.LifecycleType)
                    {
                        case LifecycleType.Singleton:
                            using (var guard = new WriterGuard(_serviceLock))
                            {
                                _services.AddSingleton(type, type);
                                _servicesChanged = true;
                                _logger.LogInformation($"Singleton service {type} was registred.");
                            }
                            break;
                        case LifecycleType.Transient:
                            using (var guard = new WriterGuard(_serviceLock))
                            {
                                _services.AddTransient(type, type);
                                _servicesChanged = true;
                                _logger.LogInformation($"Transient service {type} was registred.");
                            }
                            break;
                    }
                }
            }

            var clientProxies = new List<string>();
            if (generatedServices.Any())
            {
                var compiledType = _compiler.CompileFiles(generatedServices, "Remote");
                _apm.ApplicationParts.Add(new AssemblyPart(compiledType.Item1));
                _locator.AddAssemblyReference(compiledType.Item1);
                foreach (var type in compiledType.Item2)
                {
                    Type t;
                    if(clientProxiesToGenerate.TryGetValue(type.Name.Split('.').Last(),out t))
                    {
                        clientProxies.Add(ClientProxyCreator.Create(type,t));
                    }
                    _services.AddTransient(type, type);
                    _logger.LogInformation($"Transient service {type} was registred.");
                }
            }

            if(clientProxies.Any())
            {
                var compiledType = _compiler.CompileFiles(clientProxies, "Proxies");
                _apm.ApplicationParts.Add(new AssemblyPart(compiledType.Item1));
                foreach (var type in compiledType.Item2)
                {
                    _services.AddTransient(type.GetInterfaces().FirstOrDefault(),type);
                    _logger.LogInformation($"Transient service {type} was registred.");
                }
            }
        }
    }
}
