using Microsoft.AspNetCore.SignalR.Hubs;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;

namespace Scribi.CodeGeneration
{
    public class ScribiAssemblyLocator : DefaultAssemblyLocator
    {
        private static readonly IList<Assembly> _additionalReferences = new List<Assembly>();

        public ScribiAssemblyLocator(IHostingEnvironment environment) : base(environment)
        {
        }

        public void AddAssemblyReference(Assembly asm)
        {
            _additionalReferences.Add(asm);
        }

        public override IList<Assembly> GetAssemblies()
        {
            return base.GetAssemblies().Concat(_additionalReferences.ToArray()).ToList();
        }
    }
}
