using System;

namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Atomic persistence store contract supporting validated fallback recovery.
    /// </summary>
    public interface IPersistenceStore
    {
        void Save(string fileName, string content);
        bool TryLoad(string fileName, Func<string, bool> validator, out string content);
    }
}
