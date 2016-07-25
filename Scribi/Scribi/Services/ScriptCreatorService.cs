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

namespace Scribi.Services
{
    public class ScriptCreatorService : IScriptCreatorService
    {

        private readonly ReaderWriterLockSlim _serviceLock = new ReaderWriterLockSlim();
        private readonly ILogger _logger;
        private readonly IRuntimeCompilerService _compiler;
        private readonly IServiceCollection _services;
        private bool _servicesChanged = false;
        private readonly IControllerFactory _controllerFactory;
        private readonly ApplicationPartManager _apm;
        private IServiceProvider _serviceProvider;


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
                                        IControllerFactory controllerFactory,
                                        ApplicationPartManager apm,
                                        IServiceCollection services)
        {
            _logger = logger;
            _compiler = compiler;
            _services = services;
            _controllerFactory = controllerFactory;
            _apm = apm;
        }

        #region IService Interface
        public void Configure(IConfigurationSection config)
        {
        }

        public void Init()
        {
            Bootstrap(_compiler.GetTypes());
        }

        public void Release()
        {
            //Release all
        }
        #endregion

        public void Bootstrap(IEnumerable<Type> types)
        {
            var generatedControllers = new List<string>();
            foreach (var type in types)
            {
                var ti = type.GetTypeInfo();
                var attr = ti.GetCustomAttribute<ScriptUnitAttribute>();
                if (attr != null)
                {
                    Scripts.Add(type);
                    if (attr.AccessType == AccessType.Rest)
                        generatedControllers.Add(ControllerCreator.Create(type, attr));

                    if (attr.AccessType == AccessType.SignalR)
                        generatedControllers.Add(HubCreator.Create(type, attr));

                    switch (attr.LifecycleType)
                    {
                        case LifecycleType.Singleton:
                            using (var guard = new WriterGuard(_serviceLock))
                            {
                                _services.AddSingleton(type, type);
                                _servicesChanged = true;
                            }
                            break;
                        case LifecycleType.Transient:
                            using (var guard = new WriterGuard(_serviceLock))
                            {
                                _services.AddTransient(type, type);
                                _servicesChanged = true;
                            }
                            break;
                    }
                }
            }

            if (generatedControllers.Any())
            {
                var compiledType = _compiler.CompileFiles(generatedControllers, "Controllers");
                _apm.ApplicationParts.Add(new AssemblyPart(compiledType.Item1));
                foreach (var type in compiledType.Item2)
                    _services.AddTransient(type, type);
            }
        }
    }
}
