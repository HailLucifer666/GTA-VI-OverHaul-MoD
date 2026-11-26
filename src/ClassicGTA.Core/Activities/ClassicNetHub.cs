using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Activities
{
    public enum ClassicNetSite
    {
        Home = 0,
        DynastyRealEstate = 1,
        DockTeaseWatercraft = 2,
        ElitasAircraft = 3,
        LcnStockExchange = 4,
        Emergency911Directory = 5,
        PetKennel = 6,
        HomeFurnishings = 7
    }

    /// <summary>
    /// Classic In-Game Web Portal Hub View-Model (Cut Feature #6 [SANDBOX SUBSTITUTE]).
    /// Provides browser-style navigation history tying Properties, Dealerships, Stocks,
    /// 911 Directory, Pets, and Furnishings into one unified UI model.
    /// </summary>
    public sealed class ClassicNetHub
    {
        private readonly IGameContext _context;
        private readonly Stack<ClassicNetSite> _history = new Stack<ClassicNetSite>();

        public bool IsOpen { get; private set; }
        public ClassicNetSite CurrentSite { get; private set; }
        public int HistoryDepth { get { return _history.Count; } }

        public ClassicNetHub(IGameContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            _context = context;
            CurrentSite = ClassicNetSite.Home;
        }

        public bool Open()
        {
            if (_context.IsOnlineSession || !_context.IsPlayerReady || _context.IsInCutscene)
            {
                return false;
            }
            IsOpen = true;
            return true;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public bool NavigateTo(ClassicNetSite destination)
        {
            if (!IsOpen || _context.IsOnlineSession) return false;
            if (destination == CurrentSite) return true;

            _history.Push(CurrentSite);
            CurrentSite = destination;
            return true;
        }

        public bool GoBack()
        {
            if (!IsOpen || _history.Count == 0) return false;
            CurrentSite = _history.Pop();
            return true;
        }

        public void GoHome()
        {
            if (!IsOpen) return;
            if (CurrentSite != ClassicNetSite.Home)
            {
                _history.Push(CurrentSite);
                CurrentSite = ClassicNetSite.Home;
            }
        }
    }
}
