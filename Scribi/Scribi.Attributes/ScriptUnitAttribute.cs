using System;

namespace Scribi.Attributes
{
    public enum AccessType { Local, Rest, SignalR };

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ScriptUnitAttribute : Attribute
    {
        public string Name { get; private set; }
        public AccessType AccessType { get; private set; }

        public ScriptUnitAttribute(string name, AccessType accessType = AccessType.Local)
        {
            Name = name ?? string.Empty;
            AccessType = accessType;
        }
    }
}
