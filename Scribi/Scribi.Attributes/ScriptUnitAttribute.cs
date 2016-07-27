using System;

namespace Scribi.Attributes
{
    public enum AccessType { Local, Rest, SignalR, Remote };
    public enum LifecycleType { Singleton, Transient, };

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ScriptUnitAttribute : Attribute
    {
        public string Name { get; private set; }
        public AccessType AccessType { get; private set; }

        public LifecycleType LifecycleType { get; private set; }

        public Type ClientInterface { get; private set; }

        public ScriptUnitAttribute(string name, 
                                   LifecycleType lifecycleType = LifecycleType.Transient, 
                                   AccessType accessType = AccessType.Local,
                                   Type clientInterface = null)
        {
            Name = name ?? string.Empty;
            LifecycleType = lifecycleType;
            AccessType = accessType;
            ClientInterface = clientInterface;
        }

        public ScriptUnitAttribute(string name,
                                   AccessType accessType,
                                   Type clientInterface = null)
        {
            Name = name ?? string.Empty;
            LifecycleType = LifecycleType.Transient;
            AccessType = accessType;
            ClientInterface = clientInterface;
        }

    }
}
