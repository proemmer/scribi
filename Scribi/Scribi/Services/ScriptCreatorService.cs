using Microsoft.Extensions.Logging;
using Scribi.Attributes;
using Scribi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Threading;
using Scribi.Helper;

namespace Scribi.Services
{
    public class ScriptCreatorService : IScriptCreatorService
    {
        private const string ControllerTemplate =
        @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Scribi.Interfaces;

namespace Scribi.Controllers
{{
    [Route(""api/[controller]"")]
    public class {0}Controller : Controller
    {{
        private readonly {2} _obj;

        public {0}Controller(IScriptCreatorService ccs)
        {{
            //To get the selfe registred services, because asp.net 
            //service provider did not update at runtime
            _obj = ccs.ServiceProvider.GetRequiredService(typeof({2})) as {2};
        }}

{1}
    }}
}}
        ";

        private const string ControllerMethod =
        @"
        {0}
        public {1} {2} ({3})
        {{
            return _obj.{2}({4});
        }}
        ";

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
                        generatedControllers.Add(CreateController(type, attr));

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

        private string CreateController(Type type, ScriptUnitAttribute attr)
        {
            var sb = new StringBuilder();
            var methods = type.GetMethods();
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes<RestMethodAttribute>();
                if (attributes.Any())
                {
                    foreach (var attribute in attributes)
                    {
                        var parameters = method.GetParameters();
                        sb.Append(string.Format(ControllerMethod,
                                                HttpMethodToAttribute(attribute),
                                                method.ReturnType,
                                                method.Name,
                                                parameters.Any() ? ParametersToParameters(parameters) : string.Empty,
                                                parameters.Any() ? ParametersToCallParams(parameters) : string.Empty));
                        sb.AppendLine();
                    }
                }
            }
            return string.Format(ControllerTemplate, attr.Name, sb.ToString(), type);
        }

        private string HttpMethodToAttribute(RestMethodAttribute attr)
        {
            switch (attr.HttpMethod.ToUpper())
            {
                case "GET":
                    return string.IsNullOrWhiteSpace(attr.RouteTemplate) ? "[HttpGet]" : $"[HttpGet(\"{attr.RouteTemplate}\")]";
                case "POST":
                    return string.IsNullOrWhiteSpace(attr.RouteTemplate) ? "[HttpPost]" : $"[HttpPost(\"{attr.RouteTemplate}\")]";
                case "PATCH":
                    return string.IsNullOrWhiteSpace(attr.RouteTemplate) ? "[HttpPatch]" : $"[HttpPatch(\"{attr.RouteTemplate}\")]";
                case "PUT":
                    return string.IsNullOrWhiteSpace(attr.RouteTemplate) ? "[HttpPut]" : $"[HttpPut(\"{attr.RouteTemplate}\")]";
                case "DELETE":
                    return string.IsNullOrWhiteSpace(attr.RouteTemplate) ? "[HttpPut]" : $"[HttpPut(\"{attr.RouteTemplate}\")]";
            }
            return string.Empty;
        }

        private string ParametersToParameters(ParameterInfo[] parameters)
        {
            var sb = new StringBuilder();
            foreach (var param in parameters)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(param.ParameterType);
                sb.Append(" ");
                sb.Append(param.Name);
            }
            return sb.ToString();
        }

        private string ParametersToCallParams(ParameterInfo[] parameters)
        {
            var sb = new StringBuilder();
            foreach (var param in parameters)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(param.Name);
            }
            return sb.ToString();
        }

    }
}
