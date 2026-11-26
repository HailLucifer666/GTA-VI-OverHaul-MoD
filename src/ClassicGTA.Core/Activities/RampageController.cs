using System;
using System.Collections.Generic;
using System.Text;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Activities
{
    /// <summary>
    /// Timed Weapon-Specific Rampage &amp; Sandbox Medal Tracker (Cut Features #10 &amp; #17 [SANDBOX SUBSTITUTE]).
    /// Persists highest Bronze/Silver/Gold medal per challenge ContentId and never downgrades earned medals.
    /// </summary>
    public sealed class RampageController
    {
        private const string SaveFileName = "ClassicGTA_Medals.dat";
        private const string SchemaHeader = "MEDALS_SCHEMA=1";

        private readonly IGameContext _context;
        private readonly IEconomyService _economy;
        private readonly IPersistenceStore _store;
        private readonly INotificationService _notifications;

        private readonly Dictionary<string, MedalRating> _bestMedals =
            new Dictionary<string, MedalRating>(StringComparer.OrdinalIgnoreCase);

        private bool _isActive;
        private bool _settled;
        private string _activeId;
        private int _targetKills;
        private int _currentKills;
        private int _startTimeMs;
        private int _durationMs;
        private int _rewardCash;

        public bool IsActive { get { return _isActive; } }
        public int CurrentKills { get { return _currentKills; } }
        public MedalRating LastEarnedMedal { get; private set; }

        public RampageController(
            IGameContext context,
            IEconomyService economy,
            IPersistenceStore store,
            INotificationService notifications)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (economy == null) throw new ArgumentNullException("economy");
            if (store == null) throw new ArgumentNullException("store");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _economy = economy;
            _store = store;
            _notifications = notifications;
        }

        public bool StartRampage(string contentId, int targetKills, int durationMs, int rewardCash, int gameTimeMs)
        {
            if (_context.IsOnlineSession || _isActive) return false;
            if (string.IsNullOrWhiteSpace(contentId) || targetKills < 1 || durationMs < 5000 || rewardCash < 0)
            {
                return false;
            }

            _activeId = contentId.Trim();
            _targetKills = targetKills;
            _currentKills = 0;
            _startTimeMs = gameTimeMs;
            _durationMs = durationMs;
            _rewardCash = rewardCash;
            _settled = false;
            _isActive = true;
            LastEarnedMedal = MedalRating.None;

            _notifications.Show("RAMPAGE STARTED: Eliminate " + targetKills + " hostiles!");
            return true;
        }

        public void RecordKill(int gameTimeMs)
        {
            if (!_isActive || _context.IsOnlineSession) return;
            if (IsExpired(gameTimeMs))
            {
                EvaluateCompletion(gameTimeMs);
                return;
            }
            _currentKills++;
        }

        public void Tick(int gameTimeMs)
        {
            if (!_isActive || _context.IsOnlineSession) return;
            if (IsExpired(gameTimeMs))
            {
                EvaluateCompletion(gameTimeMs);
            }
        }

        public MedalRating FinishEarly(int gameTimeMs)
        {
            if (!_isActive) return MedalRating.None;
            return EvaluateCompletion(gameTimeMs);
        }

        public MedalRating GetBestMedal(string contentId)
        {
            MedalRating rating;
            return !string.IsNullOrWhiteSpace(contentId) && _bestMedals.TryGetValue(contentId, out rating)
                ? rating
                : MedalRating.None;
        }

        public bool LoadState()
        {
            string raw;
            if (!_store.TryLoad(SaveFileName, s => !string.IsNullOrWhiteSpace(s) && s.StartsWith(SchemaHeader, StringComparison.Ordinal), out raw))
            {
                return false;
            }

            _bestMedals.Clear();
            string[] lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Trim().Split('|');
                int ratingInt;
                if (parts.Length == 2 && int.TryParse(parts[1], out ratingInt) && Enum.IsDefined(typeof(MedalRating), ratingInt))
                {
                    _bestMedals[parts[0]] = (MedalRating)ratingInt;
                }
            }
            return true;
        }

        private bool IsExpired(int gameTimeMs)
        {
            return gameTimeMs >= _startTimeMs && (gameTimeMs - _startTimeMs) >= _durationMs;
        }

        private MedalRating EvaluateCompletion(int gameTimeMs)
        {
            if (_settled) return LastEarnedMedal;
            _settled = true;
            _isActive = false;

            MedalRating earned = MedalRating.None;
            int elapsed = gameTimeMs >= _startTimeMs ? (gameTimeMs - _startTimeMs) : _durationMs;

            if (_currentKills >= _targetKills)
            {
                int goldKills = (_targetKills * 3 + 1) / 2; // 1.5x rounded up
                int silverKills = (_targetKills * 5 + 3) / 4; // 1.25x rounded up

                if (_currentKills >= goldKills || elapsed <= (_durationMs * 60) / 100)
                {
                    earned = MedalRating.Gold;
                }
                else if (_currentKills >= silverKills)
                {
                    earned = MedalRating.Silver;
                }
                else
                {
                    earned = MedalRating.Bronze;
                }
            }

            LastEarnedMedal = earned;

            if (earned != MedalRating.None)
            {
                MedalRating existing = GetBestMedal(_activeId);
                if (earned > existing)
                {
                    _bestMedals[_activeId] = earned;
                    PersistState();
                }

                int bonusMultiplier = earned == MedalRating.Gold ? 2 : 1;
                int totalReward = _rewardCash * bonusMultiplier;
                _economy.Credit(totalReward, "Rampage:" + _activeId + ":" + earned);
                _notifications.Show("RAMPAGE PASSED! Medal: " + earned + " (+$" + totalReward + ")");
            }
            else
            {
                _notifications.Show("RAMPAGE FAILED (" + _currentKills + "/" + _targetKills + " kills).");
            }

            return earned;
        }

        private void PersistState()
        {
            var sb = new StringBuilder();
            sb.AppendLine(SchemaHeader);
            foreach (var kv in _bestMedals)
            {
                sb.Append(kv.Key).Append('|').AppendLine(((int)kv.Value).ToString());
            }
            _store.Save(SaveFileName, sb.ToString());
        }
    }
}
