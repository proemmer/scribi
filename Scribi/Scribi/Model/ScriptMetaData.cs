using Scribi.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scribi.Model
{
    public class ScriptMetaData
    {
        public string Name { get; internal set; }
        public IEnumerable<string> Methods { get; internal set; }
        public IEnumerable<string> Properties { get; internal set; }
        public LifecycleType LifecycleType { get; internal set; }
    }
}
