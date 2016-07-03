using System;

namespace Scribi.Interfaces
{
    public interface IControllerCreatorService : IService
    {
        IServiceProvider ServiceProvider { get; }
    }
}