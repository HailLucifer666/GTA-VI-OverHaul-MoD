using System;
using ClassicGTA.Contracts;
using GTA;
using GTA.Native;

namespace ClassicGTA.Adapter.GTAV
{
    /// <summary>
    /// Engine-independent fail-closed online detection evaluator (A-03).
    /// Logs and notifies once on native exception while returning true (Online / blocked),
    /// and resets the warning latch if a subsequent probe succeeds.
    /// </summary>
    public sealed class OnlineDetectionGuard
    {
        private readonly INotificationService _notifications;
        private bool _loggedFailure;

        public bool HasLoggedFailure { get { return _loggedFailure; } }

        public OnlineDetectionGuard(INotificationService notifications = null)
        {
            _notifications = notifications;
        }

        public bool Evaluate(Func<bool> nativeProbe)
        {
            try
            {
                bool online = nativeProbe();
                _loggedFailure = false;
                return online;
            }
            catch (Exception ex)
            {
                if (!_loggedFailure)
                {
                    _loggedFailure = true;
                    if (_notifications != null)
                    {
                        _notifications.Log("[GameContext] Online detection native failed (fail-closed -> Online): " + ex.Message);
                        _notifications.Show("~y~Classic GTA paused:~s~ session check failed (see ClassicGTA.log).");
                    }
                }
                return true;
            }
        }
    }

    internal sealed class GtaVGameContext : IGameContext
    {
        private const ulong HashNetworkIsSessionStarted = 0x9DE624D2FC4B603FUL;
        private const ulong HashNetworkIsInSession = 0xCA97246103B63917UL;

        private readonly OnlineDetectionGuard _onlineGuard;

        public GtaVGameContext(INotificationService notifications = null)
        {
            _onlineGuard = new OnlineDetectionGuard(notifications);
        }

        public bool IsPlayerReady
        {
            get
            {
                Ped ped = Game.Player.Character;
                return ped != null && ped.Exists() && ped.IsAlive;
            }
        }

        public int PlayerHandle
        {
            get
            {
                Ped ped = Game.Player.Character;
                return ped != null && ped.Exists() ? ped.Handle : 0;
            }
        }

        public bool IsInCutscene { get { return Game.IsCutsceneActive; } }
        public bool IsPaused { get { return Game.IsPaused; } }
        public bool IsLoading { get { return Game.IsLoading; } }

        public bool IsOnlineSession
        {
            get
            {
                return _onlineGuard.Evaluate(() =>
                    Function.Call<bool>((Hash)HashNetworkIsSessionStarted) ||
                    Function.Call<bool>((Hash)HashNetworkIsInSession));
            }
        }

        public bool IsInVehicle
        {
            get
            {
                Ped ped = Game.Player.Character;
                return ped != null && ped.Exists() && ped.IsInVehicle();
            }
        }

        public bool IsScreenFadedOut { get { return GTA.UI.Screen.IsFadedOut; } }
        public int GameTimeMs { get { return Game.GameTime; } }
    }
}
