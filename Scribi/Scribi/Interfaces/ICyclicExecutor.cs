using System;

namespace Scribi.Interfaces
{
    public interface ICyclicExecutorService
    {
        void Add(string name, string label, double milliseconds, Action action, bool enabled = false, bool ownThread = false, bool executeOnlyOnce = false);
        bool Contains(string aName);
        bool Enabled(string name);
        void Enabled(string name, bool state, bool startImmediately = false);
        void Shutdown();
        void Update(string name, double milliseconds);
    }
}