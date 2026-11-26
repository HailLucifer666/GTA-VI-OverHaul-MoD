using System;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Activities
{
    public enum HangoutState
    {
        Idle = 0,
        Following = 1,
        CombatAssist = 2
    }

    /// <summary>
    /// Classic Friend Hangouts / Companion AI Controller (Cut Feature #24).
    /// Supports calling a buddy, following, combat assistance, natural dismissal, and leak-free cleanup.
    /// </summary>
    public sealed class CompanionHangoutController : IModModule
    {
        private readonly IGameContext _context;
        private readonly ICompanionAdapter _adapter;
        private readonly IEntityRegistry _registry;
        private readonly INotificationService _notifications;

        private bool _isRunning;
        private bool _isEnabled = true;
        private HangoutState _state = HangoutState.Idle;
        private string _activeCompanionId;
        private int _companionPedHandle;

        public string ModuleId { get { return "classic.social.hangout"; } }
        public string DisplayName { get { return "Friend Hangouts (Companion AI)"; } }
        public HangoutState CurrentState { get { return _state; } }
        public string ActiveCompanionId { get { return _activeCompanionId; } }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public CompanionHangoutController(
            IGameContext context,
            ICompanionAdapter adapter,
            IEntityRegistry registry,
            INotificationService notifications)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (registry == null) throw new ArgumentNullException("registry");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _adapter = adapter;
            _registry = registry;
            _notifications = notifications;
        }

        public void Start()
        {
            _isRunning = true;
            _state = HangoutState.Idle;
        }

        public void FastTick() { }

        public bool CallCompanion(string companionId)
        {
            if (!_isRunning || !_isEnabled || _state != HangoutState.Idle) return false;
            if (_context.IsOnlineSession || !_context.IsPlayerReady || string.IsNullOrWhiteSpace(companionId)) return false;

            int pedHandle;
            int blipHandle;
            if (!_adapter.TrySpawnCompanion(companionId.Trim(), _adapter.GetPlayerPosition(), out pedHandle, out blipHandle))
            {
                return false;
            }

            if (!_registry.TryTrackEntity(pedHandle, ModuleId, EntityOwnership.Owned))
            {
                // V-03 Fix: Delete untracked ped and blip immediately on budget refusal.
                _registry.DeleteUntrackedEntity(pedHandle);
                _registry.DeleteUntrackedBlip(blipHandle);
                _registry.CleanupOwner(ModuleId, false);
                return false;
            }

            if (blipHandle > 0)
            {
                _registry.TrackBlip(blipHandle, ModuleId);
            }

            _activeCompanionId = companionId.Trim();
            _companionPedHandle = pedHandle;
            _state = HangoutState.Following;
            _adapter.TaskFollowPlayer(_companionPedHandle);
            _notifications.Show("Hangout started with " + _activeCompanionId + ".");
            return true;
        }

        public bool CommandCombatAssist()
        {
            if (!_isRunning || _state == HangoutState.Idle) return false;
            if (!_adapter.IsEntityAlive(_companionPedHandle))
            {
                EndHangout("CompanionDied", false);
                return false;
            }

            _state = HangoutState.CombatAssist;
            _adapter.TaskAssistInCombat(_companionPedHandle);
            return true;
        }

        public void DismissCompanion()
        {
            if (_state == HangoutState.Idle) return;
            if (_adapter.IsEntityAlive(_companionPedHandle))
            {
                _adapter.TaskDismiss(_companionPedHandle);
            }
            // Release naturally so companion walks off instead of vanishing.
            EndHangout("Dismissed", true);
        }

        public void SlowTick(int gameTimeMs)
        {
            if (!_isRunning || !_isEnabled || _state == HangoutState.Idle) return;
            if (_context.IsOnlineSession)
            {
                // V-04 Fix: Drop only this owner's handles without calling DeleteEntity.
                _registry.DropOwnerWithoutDeleting(ModuleId);
                _companionPedHandle = 0;
                _activeCompanionId = null;
                _state = HangoutState.Idle;
                return;
            }

            if (!_adapter.IsEntityAlive(_companionPedHandle))
            {
                _notifications.Show(_activeCompanionId + " was injured! Hangout ended.");
                EndHangout("CompanionDied", false);
            }
        }

        public void OnKeyPressed(GameKey key) { }

        public void Stop()
        {
            _isRunning = false;
            if (_state != HangoutState.Idle)
            {
                EndHangout("Stopped", false);
            }
        }

        public void Dispose() { Stop(); }

        private void EndHangout(string reason, bool releaseInsteadOfDelete)
        {
            _registry.CleanupOwner(ModuleId, releaseInsteadOfDelete);
            _companionPedHandle = 0;
            _activeCompanionId = null;
            _state = HangoutState.Idle;
            _notifications.Log("[Hangout] ENDED reason=" + reason);
        }
    }
}
