using System;

namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Atomic persistence store contract supporting validated fallback recovery.
    /// </summary>
    public interface IPersistenceStore
    {
        void Save(string fileName, string content);
        void Save(string fileName, string content, Func<string, bool> existingBackupValidator);
        bool TryLoad(string fileName, Func<string, bool> validator, out string content);
        bool Exists(string fileName);
    }
}
