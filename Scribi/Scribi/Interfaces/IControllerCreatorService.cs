using System;
using System.Collections.Generic;

namespace Scribi.Interfaces
{
    public interface IScriptCreatorService : IService
    {
        IServiceProvider ServiceProvider { get; }
        List<Type> Scripts { get; }
    }
}