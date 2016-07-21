using Scribi.Model;
using System.Collections.Generic;

namespace Scribi.Interfaces
{
    public interface IScriptFactoryService : IService
    {
        IEnumerable<string> GetScriptNames();
        ScriptMetaData GetScriptMetaInfo(string script);
    }
}
