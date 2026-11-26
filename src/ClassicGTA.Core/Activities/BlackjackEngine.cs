using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Activities
{
    public enum BlackjackOutcome
    {
        InProgress = 0,
        PlayerBlackjack = 1,
        PlayerWin = 2,
        DealerWin = 3,
        Push = 4,
        PlayerBust = 5,
        DealerBust = 6,
        VoidedOnline = 7
    }

    /// <summary>
    /// Rules-complete Virtual-Cash Blackjack Engine (Cut Feature #15 — zero real-money gambling).
    /// Supports Hit, Stand, Double Down, soft-Ace valuation, dealer stands on 17, and 3:2 Natural Blackjack.
    /// </summary>
    public sealed class BlackjackEngine
    {
        private readonly IGameContext _context;
        private readonly IEconomyService _economy;
        private readonly Func<int> _drawCardRank; // returns 1..13 (1=Ace, 11..13=J/Q/K)
        private readonly int _minBet;
        private readonly int _maxBet;

        private readonly List<int> _playerHand = new List<int>();
        private readonly List<int> _dealerHand = new List<int>();

        private int _currentBet;
        private BlackjackOutcome _outcome = BlackjackOutcome.Push;
        private bool _roundActive;

        public bool IsRoundActive { get { return _roundActive; } }
        public BlackjackOutcome Outcome { get { return _outcome; } }
        public int PlayerTotal { get { return CalculateHandValue(_playerHand); } }
        public int DealerTotal { get { return CalculateHandValue(_dealerHand); } }
        public int DealerVisibleTotal
        {
            get
            {
                if (_dealerHand.Count == 0) return 0;
                if (!_roundActive) return DealerTotal;
                return CalculateHandValue(new[] { _dealerHand[0] });
            }
        }
        public int CurrentBet { get { return _currentBet; } }
        public int PendingOnlineRefund { get; private set; }

        public int ClaimPendingRefund()
        {
            if (_context.IsOnlineSession || PendingOnlineRefund <= 0)
            {
                return 0;
            }

            int refund = PendingOnlineRefund;
            PendingOnlineRefund = 0;
            _economy.Credit(refund, "Blackjack:OnlineRefund");
            return refund;
        }

        public BlackjackEngine(
            IGameContext context,
            IEconomyService economy,
            Func<int> drawCardRank = null,
            int minBet = 10,
            int maxBet = 50000)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (economy == null) throw new ArgumentNullException("economy");
            if (minBet < 1 || maxBet < minBet) throw new ArgumentOutOfRangeException("minBet");

            _context = context;
            _economy = economy;
            _minBet = minBet;
            _maxBet = maxBet;

            if (drawCardRank != null)
            {
                _drawCardRank = drawCardRank;
            }
            else
            {
                var rng = new Random();
                _drawCardRank = () => rng.Next(1, 14);
            }
        }

        public bool StartHand(int bet)
        {
            if (_context.IsOnlineSession || _roundActive) return false;
            ClaimPendingRefund();
            if (bet < _minBet || bet > _maxBet) return false;
            if (_economy.GetCash() < bet || !_economy.TrySpend(bet, "BlackjackBet")) return false;

            _playerHand.Clear();
            _dealerHand.Clear();
            _currentBet = bet;
            _roundActive = true;
            _outcome = BlackjackOutcome.InProgress;

            _playerHand.Add(DrawRank());
            _dealerHand.Add(DrawRank());
            _playerHand.Add(DrawRank());
            _dealerHand.Add(DrawRank());

            // Check natural 21
            int pTotal = PlayerTotal;
            int dTotal = DealerTotal;
            if (pTotal == 21 && dTotal == 21)
            {
                Settle(BlackjackOutcome.Push, _currentBet);
            }
            else if (pTotal == 21)
            {
                int payout = _currentBet + ((_currentBet * 3) / 2);
                Settle(BlackjackOutcome.PlayerBlackjack, payout);
            }

            return true;
        }

        public BlackjackOutcome Hit()
        {
            if (!_roundActive) return _outcome;
            if (_context.IsOnlineSession)
            {
                // V-18 Fix: Void round cleanly on SP->Online transition instead of freezing _roundActive forever.
                return VoidForOnline();
            }

            _playerHand.Add(DrawRank());
            if (PlayerTotal > 21)
            {
                Settle(BlackjackOutcome.PlayerBust, 0);
            }
            return _outcome;
        }

        public BlackjackOutcome DoubleDown()
        {
            if (!_roundActive || _playerHand.Count != 2) return _outcome;
            if (_context.IsOnlineSession)
            {
                return VoidForOnline();
            }
            if (_economy.GetCash() < _currentBet || !_economy.TrySpend(_currentBet, "BlackjackDouble"))
            {
                return _outcome;
            }

            _currentBet *= 2;
            _playerHand.Add(DrawRank());
            if (PlayerTotal > 21)
            {
                Settle(BlackjackOutcome.PlayerBust, 0);
                return _outcome;
            }

            return Stand();
        }

        public BlackjackOutcome Stand()
        {
            if (!_roundActive) return _outcome;
            if (_context.IsOnlineSession)
            {
                return VoidForOnline();
            }

            while (DealerTotal < 17)
            {
                _dealerHand.Add(DrawRank());
            }

            int pTotal = PlayerTotal;
            int dTotal = DealerTotal;

            if (dTotal > 21)
            {
                Settle(BlackjackOutcome.DealerBust, _currentBet * 2);
            }
            else if (pTotal > dTotal)
            {
                Settle(BlackjackOutcome.PlayerWin, _currentBet * 2);
            }
            else if (pTotal < dTotal)
            {
                Settle(BlackjackOutcome.DealerWin, 0);
            }
            else
            {
                Settle(BlackjackOutcome.Push, _currentBet);
            }

            return _outcome;
        }

        public static int CalculateHandValue(IList<int> ranks)
        {
            int total = 0;
            int aces = 0;

            for (int i = 0; i < ranks.Count; i++)
            {
                int r = ranks[i];
                if (r == 1)
                {
                    aces++;
                    total += 11;
                }
                else if (r >= 10)
                {
                    total += 10;
                }
                else
                {
                    total += r;
                }
            }

            // Soft-Ace adjustment: downgrade Aces from 11 to 1 while over 21
            while (total > 21 && aces > 0)
            {
                total -= 10;
                aces--;
            }

            return total;
        }

        private int DrawRank()
        {
            int rank = _drawCardRank();
            if (rank < 1 || rank > 13)
            {
                throw new InvalidOperationException("Card rank out of range (1..13): " + rank);
            }
            return rank;
        }

        private BlackjackOutcome VoidForOnline()
        {
            // I-08 Fix: Record explicit VoidedOnline outcome and queue a Story Mode refund without touching Online cash.
            if (_currentBet > 0)
            {
                PendingOnlineRefund += _currentBet;
            }
            _outcome = BlackjackOutcome.VoidedOnline;
            _roundActive = false;
            _currentBet = 0;
            return _outcome;
        }

        private void Settle(BlackjackOutcome outcome, int payout)
        {
            _outcome = outcome;
            _roundActive = false;
            if (payout > 0)
            {
                _economy.Credit(payout, "Blackjack:" + outcome);
            }
        }
    }
}
