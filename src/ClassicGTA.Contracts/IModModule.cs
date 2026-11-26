using System;

namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Standard lifecycle contract for every Classic GTA feature module.
    /// Zero dependency on any specific RAGE/ScriptHook runtime.
    /// </summary>
    public interface IModModule : IDisposable
    {
        string ModuleId { get; }
        string DisplayName { get; }
        bool IsEnabled { get; set; }

        void Start();
        void FastTick();
        void SlowTick(int gameTimeMs);
        void OnKeyPressed(GameKey key);
        void Stop();
    }
}
