using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;

namespace ClassicGTA.Infrastructure
{
    /// <summary>
    /// 3-Tier Fault-Isolated Module Scheduler (0ms FastTick, 250ms SlowTick, 1000ms Heartbeat).
    /// Any module throwing an exception is isolated, stopped, and marked Faulted so healthy
    /// sibling modules keep running without crashing the script host.
    /// </summary>
    public sealed class ModuleScheduler : IDisposable
    {
        private sealed class Slot
        {
            public IModModule Module;
            public bool Started;
            public bool Faulted;
            public string LastError;
        }

        private readonly INotificationService _logger;
        private readonly int _slowIntervalMs;
        private readonly int _heartbeatIntervalMs;
        private readonly List<Slot> _slots = new List<Slot>();
        private readonly Dictionary<string, Slot> _byId = new Dictionary<string, Slot>(StringComparer.OrdinalIgnoreCase);

        private bool _hasSlowTicked;
        private int _lastSlowTickMs;
        private bool _hasHeartbeatTicked;
        private int _lastHeartbeatTickMs;
        private int _heartbeatCount;

        public int HeartbeatCount { get { return _heartbeatCount; } }

        public ModuleScheduler(
            INotificationService logger,
            int slowIntervalMs = 250,
            int heartbeatIntervalMs = 1000)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (slowIntervalMs < 50) throw new ArgumentOutOfRangeException("slowIntervalMs");
            if (heartbeatIntervalMs < slowIntervalMs) throw new ArgumentOutOfRangeException("heartbeatIntervalMs");

            _logger = logger;
            _slowIntervalMs = slowIntervalMs;
            _heartbeatIntervalMs = heartbeatIntervalMs;
        }

        public void Register(IModModule module)
        {
            if (module == null) throw new ArgumentNullException("module");
            if (string.IsNullOrWhiteSpace(module.ModuleId))
            {
                throw new ArgumentException("ModuleId cannot be empty.", "module");
            }
            if (_byId.ContainsKey(module.ModuleId))
            {
                throw new InvalidOperationException("Duplicate ModuleId registered: " + module.ModuleId);
            }

            var slot = new Slot { Module = module };
            _slots.Add(slot);
            _byId.Add(module.ModuleId, slot);
        }

        public bool IsFaulted(string moduleId)
        {
            Slot slot;
            return _byId.TryGetValue(moduleId, out slot) && slot.Faulted;
        }

        public string GetLastError(string moduleId)
        {
            Slot slot;
            return _byId.TryGetValue(moduleId, out slot) ? slot.LastError : null;
        }

        public void StartAll()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                Slot slot = _slots[i];
                if (slot.Faulted || slot.Started) continue;
                RunIsolated(slot, "Start", delegate { slot.Module.Start(); slot.Started = true; });
            }
        }

        public void Tick(int gameTimeMs)
        {
            // Tier 1: FastTick (0ms, every frame)
            for (int i = 0; i < _slots.Count; i++)
            {
                Slot slot = _slots[i];
                if (slot.Faulted || !slot.Started || !slot.Module.IsEnabled) continue;
                RunIsolated(slot, "FastTick", delegate { slot.Module.FastTick(); });
            }

            // Tier 2: SlowTick (250ms default, wrap-around safe)
            if (IsIntervalElapsed(gameTimeMs, ref _hasSlowTicked, ref _lastSlowTickMs, _slowIntervalMs))
            {
                for (int i = 0; i < _slots.Count; i++)
                {
                    Slot slot = _slots[i];
                    if (slot.Faulted || !slot.Started || !slot.Module.IsEnabled) continue;
                    RunIsolated(slot, "SlowTick", delegate { slot.Module.SlowTick(gameTimeMs); });
                }
            }

            // Tier 3: Heartbeat (1000ms default, wrap-around safe)
            if (IsIntervalElapsed(gameTimeMs, ref _hasHeartbeatTicked, ref _lastHeartbeatTickMs, _heartbeatIntervalMs))
            {
                _heartbeatCount++;
            }
        }

        public void DispatchKey(GameKey key)
        {
            if (key == GameKey.None) return;

            for (int i = 0; i < _slots.Count; i++)
            {
                Slot slot = _slots[i];
                if (slot.Faulted || !slot.Started || !slot.Module.IsEnabled) continue;
                RunIsolated(slot, "OnKeyPressed", delegate { slot.Module.OnKeyPressed(key); });
            }
        }

        public void StopAll()
        {
            // Stop in reverse dependency/registration order
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                Slot slot = _slots[i];
                if (!slot.Started) continue;
                try
                {
                    slot.Module.Stop();
                }
                catch (Exception ex)
                {
                    _logger.Log("[Scheduler] Stop failed for " + slot.Module.ModuleId + ": " + ex.Message);
                }
                finally
                {
                    slot.Started = false;
                }
            }
        }

        public void Dispose()
        {
            StopAll();
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                try
                {
                    _slots[i].Module.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Log("[Scheduler] Dispose failed for " + _slots[i].Module.ModuleId + ": " + ex.Message);
                }
            }
        }

        private static bool IsIntervalElapsed(int nowMs, ref bool hasRun, ref int lastMs, int intervalMs)
        {
            if (!hasRun || nowMs < lastMs || (nowMs - lastMs) >= intervalMs)
            {
                hasRun = true;
                lastMs = nowMs;
                return true;
            }
            return false;
        }

        private void RunIsolated(Slot slot, string phase, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                slot.Faulted = true;
                slot.LastError = phase + ": " + ex.Message;
                _logger.Log("[Scheduler] FAULT in " + slot.Module.ModuleId + " during " + phase + ": " + ex.Message);
                try { slot.Module.Stop(); } catch { /* already faulted */ }
                slot.Started = false;
            }
        }
    }
}
