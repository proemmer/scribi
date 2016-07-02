using System;
using System.Collections.Generic;
using System.Reflection;

namespace Scribi.Interfaces
{
    /// <summary>
    /// Interface for runtime compiler service to get the dynamic compiled types
    /// </summary>
    public interface IRuntimeCompilerService : IService
    {
        /// <summary>
        /// Return all dynamic compiled types
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetTypes();

        Tuple<Assembly, IEnumerable<Type>> CompileFiles(IEnumerable<string> files, string assemblyName);

        Tuple<Assembly, IEnumerable<Type>> CompileFilesFromLocation(string assemblyName);
    }
}