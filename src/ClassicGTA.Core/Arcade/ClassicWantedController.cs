using System;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Arcade
{
    /// <summary>
    /// Phase 7 Restoration #2: Classic Wanted &amp; Criminal Profile Heat Reset.
    /// Restores classic line-of-sight wanted cooldown (wrap-around safe, pauses during cutscenes/menus)
    /// and Pay 'n' Spray / Bribe clearing via Mod Wallet.
    /// </summary>
    public sealed class ClassicWantedController : IModModule
    {
        public const int DefaultCooldownSeconds = 20;
        public const int DefaultMaxWantedStars = 6;
        public const int DefaultBribeCost = 500;

        public string ModuleId { get { return "arcade.classic_wanted"; } }
        public string DisplayName { get { return "Classic Wanted & Heat Reset"; } }
        public bool IsEnabled { get; set; }

        public bool ClassicCooldownEnabled { get; set; }

        private int _cooldownSeconds = DefaultCooldownSeconds;
        public int CooldownSeconds
        {
            get { return _cooldownSeconds; }
            set { _cooldownSeconds = value < 1 ? 1 : value; }
        }

        private int _maxWantedStars = DefaultMaxWantedStars;
        public int MaxWantedStars
        {
            get { return _maxWantedStars; }
            set { _maxWantedStars = value < 1 ? 1 : (value > 6 ? 6 : value); }
        }

        private int _bribeCost = DefaultBribeCost;
        public int BribeCost
        {
            get { return _bribeCost; }
            set { _bribeCost = value < 0 ? 0 : value; }
        }

        public int AccumulatedHiddenMs { get { return _accumulatedHiddenMs; } }

        private readonly IGameContext _context;
        private readonly IWantedSystemAdapter _adapter;
        private readonly IEconomyService _economy;
        private readonly INotificationService _notifications;

        private bool _started;
        private bool _disposed;
        private bool _hasLastTick;
        private int _lastTickMs;
        private int _accumulatedHiddenMs;

        public ClassicWantedController(
            IGameContext context,
            IWantedSystemAdapter adapter,
            IEconomyService economy,
            INotificationService notifications)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (economy == null) throw new ArgumentNullException("economy");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _adapter = adapter;
            _economy = economy;
            _notifications = notifications;

            IsEnabled = true;
            ClassicCooldownEnabled = true;
        }

        public void Start()
        {
            if (_disposed) return;
            _started = true;
            ResetTimer();
            if (!_context.IsOnlineSession && _context.IsPlayerReady)
            {
                _adapter.SetMaxWantedLevel(_maxWantedStars);
            }
        }

        public void FastTick()
        {
        }

        public void SlowTick(int gameTimeMs)
        {
            if (_context.IsOnlineSession)
            {
                ResetTimer();
                return;
            }

            if (!IsEnabled || !_started || _disposed || !_context.IsPlayerReady || _context.IsLoading)
            {
                return;
            }

            if (_context.IsInCutscene || _context.IsPaused)
            {
                // Pause hidden timer without resetting accumulated progress.
                _hasLastTick = false;
                return;
            }

            if (!ClassicCooldownEnabled)
            {
                ResetTimer();
                return;
            }

            int wanted = _adapter.GetWantedLevel();
            if (wanted <= 0)
            {
                ResetTimer();
                return;
            }

            if (_adapter.ArePoliceSpottingPlayer())
            {
                ResetTimer();
                return;
            }

            if (!_hasLastTick || gameTimeMs < _lastTickMs)
            {
                // First hidden tick or GameTimeMs wrap-around: re-anchor clock cleanly without freezing.
                _hasLastTick = true;
                _lastTickMs = gameTimeMs;
                return;
            }

            int delta = gameTimeMs - _lastTickMs;
            _lastTickMs = gameTimeMs;
            _accumulatedHiddenMs += delta;

            if (_accumulatedHiddenMs >= _cooldownSeconds * 1000)
            {
                _adapter.SetWantedLevel(0);
                _adapter.ClearCriminalProfileHeat();
                ResetTimer();
                _notifications.Show("Classic Wanted: Escaped police & cleared Heat.");
            }
        }

        public bool TryPayNSprayBribe()
        {
            if (_context.IsOnlineSession)
            {
                return false;
            }

            if (_disposed || !_context.IsPlayerReady)
            {
                return false;
            }

            if (_adapter.GetWantedLevel() <= 0)
            {
                _notifications.Show("No active wanted level to clear.");
                return false;
            }

            if (_bribeCost > 0 && (_economy.GetCash() < _bribeCost || !_economy.TrySpend(_bribeCost, "PayNSprayBribe")))
            {
                _notifications.Show("Insufficient Mod Wallet cash for Pay 'n' Spray bribe ($" + _bribeCost + ").");
                return false;
            }

            _adapter.SetWantedLevel(0);
            _adapter.ClearCriminalProfileHeat();
            ResetTimer();
            _notifications.Show("Pay 'n' Spray Bribe ($" + _bribeCost + "): Wanted & Heat cleared!");
            return true;
        }

        public void OnKeyPressed(GameKey key)
        {
        }

        public void Stop()
        {
            _started = false;
            ResetTimer();
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
        }

        private void ResetTimer()
        {
            _hasLastTick = false;
            _lastTickMs = 0;
            _accumulatedHiddenMs = 0;
        }
    }
}
