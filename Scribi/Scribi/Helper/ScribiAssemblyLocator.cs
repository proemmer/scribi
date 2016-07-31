using Microsoft.AspNetCore.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;

namespace Scribi.Helper
{
    public class ScribiAssemblyLocator : IAssemblyLocator
    {
        private static readonly string AssemblyRoot = typeof(Hub).GetTypeInfo().Assembly.GetName().Name;
        private readonly Assembly _entryAssembly;
        private static readonly IList<Assembly> _additionalReferences = new List<Assembly>();
        private DependencyContext _dependencyContext;

        public ScribiAssemblyLocator(IHostingEnvironment environment)
        {
            _entryAssembly = Assembly.Load(new AssemblyName(environment.ApplicationName));
            _dependencyContext = DependencyContext.Load(_entryAssembly);
        }

        public void AddAssemblyReference(Assembly asm)
        {
            _additionalReferences.Add(asm);
        }

        public virtual IList<Assembly> GetAssemblies()
        {
            if (_dependencyContext == null)
            {
                // Use the entry assembly as the sole candidate.
                return new[] { _entryAssembly };
            }

            var result = _dependencyContext
                .RuntimeLibraries
                .Where(IsCandidateLibrary)
                .SelectMany(l => l.GetDefaultAssemblyNames(_dependencyContext))
                .Select(assembly => Assembly.Load(new AssemblyName(assembly.Name))).ToList();
            return result.Concat(_additionalReferences.ToArray()).ToList();
        }

        private bool IsCandidateLibrary(RuntimeLibrary library)
        {
            return library.Dependencies.Any(dependency => string.Equals(AssemblyRoot, dependency.Name, StringComparison.Ordinal));
        }

    }
}
