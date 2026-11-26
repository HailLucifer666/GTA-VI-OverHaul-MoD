using System;

namespace ClassicGTA.Adapter.GTAV
{
    /// <summary>
    /// Non-throwing UI loop wrapper with consecutive-failure circuit breaker (I-06).
    /// Guarantees that neither UI processing nor recovery actions (HideAll / Log / Show)
    /// can ever throw out of OnTick and abort SHVDN3, and prevents per-frame error spam.
    /// </summary>
    public sealed class UiLoopGuard
    {
        private readonly int _maxConsecutiveErrors;
        private int _consecutiveErrors;
        private bool _uiDisabled;

        public bool IsUiDisabled { get { return _uiDisabled; } }
        public int ConsecutiveErrors { get { return _consecutiveErrors; } }

        public UiLoopGuard(int maxConsecutiveErrors = 3)
        {
            if (maxConsecutiveErrors < 1) throw new ArgumentOutOfRangeException("maxConsecutiveErrors");
            _maxConsecutiveErrors = maxConsecutiveErrors;
        }

        public bool TryProcess(Action processAction, Action hideAllAction, Action<string> logAction, Action<string> showAction)
        {
            if (_uiDisabled || processAction == null)
            {
                return false;
            }

            try
            {
                processAction();
                _consecutiveErrors = 0;
                return true;
            }
            catch (Exception ex)
            {
                _consecutiveErrors++;
                SafeInvoke(hideAllAction);
                SafeInvoke(() =>
                {
                    if (logAction != null) logAction("[Host] UI Process error (" + _consecutiveErrors + "/" + _maxConsecutiveErrors + "): " + ex.Message);
                });

                if (_consecutiveErrors >= _maxConsecutiveErrors)
                {
                    _uiDisabled = true;
                    SafeInvoke(() =>
                    {
                        if (logAction != null) logAction("[Host] UI disabled after " + _consecutiveErrors + " consecutive errors. Press F7 to retry.");
                    });
                    SafeInvoke(() =>
                    {
                        if (showAction != null) showAction("~r~Classic GTA UI paused after repeated errors.~s~ Press F7 to retry.");
                    });
                }
                else
                {
                    SafeInvoke(() =>
                    {
                        if (showAction != null) showAction("~r~Classic GTA UI error:~s~ check ClassicGTA.log");
                    });
                }
                return false;
            }
        }

        public void Reset()
        {
            _consecutiveErrors = 0;
            _uiDisabled = false;
        }

        private static void SafeInvoke(Action action)
        {
            if (action == null) return;
            try
            {
                action();
            }
            catch
            {
                // Recovery actions must never escape and abort the SHVDN3 script host.
            }
        }
    }
}
