using System;
using System.Collections.Generic;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Economy
{
    /// <summary>
    /// Dedicated per-slot atomic mod wallet (C-01 fix).
    /// Keeps mod cash on the exact same atomic persistence plane as mod assets so reloading an
    /// older GTA Story Mode save can never restore spent cash while retaining owned properties.
    /// </summary>
    public sealed class ModWalletEconomyService : IEconomyService
    {
        public const int DefaultInitialBalance = 500000;
        private const string SchemaHeader = "WALLET_SCHEMA=1";

        private readonly IPersistenceStore _store;
        private readonly INotificationService _notifications;
        private readonly string _slotFingerprint;
        private readonly string _saveFileName;
        private readonly int _initialBalance;

        private int _balance;
        private bool _loadFailedCorrupt;

        public string SlotFingerprint { get { return _slotFingerprint; } }
        public string SaveFileName { get { return _saveFileName; } }
        public bool LoadFailedCorrupt { get { return _loadFailedCorrupt; } }

        public ModWalletEconomyService(
            IPersistenceStore store,
            INotificationService notifications,
            string slotFingerprint = "gtav_story_default",
            int initialBalance = DefaultInitialBalance)
        {
            if (store == null) throw new ArgumentNullException("store");
            if (notifications == null) throw new ArgumentNullException("notifications");
            if (initialBalance < 0) throw new ArgumentOutOfRangeException("initialBalance");

            _store = store;
            _notifications = notifications;
            _slotFingerprint = string.IsNullOrWhiteSpace(slotFingerprint) ? "gtav_story_default" : slotFingerprint.Trim();
            _saveFileName = "ClassicGTA_Wallet_" + PersistenceCodec.SanitizeSlotId(_slotFingerprint) + ".dat";
            _initialBalance = initialBalance;
            _balance = initialBalance;
        }

        public bool LoadState()
        {
            string raw;
            if (!_store.TryLoad(_saveFileName, ValidatePayload, out raw))
            {
                if (_store.Exists(_saveFileName))
                {
                    _loadFailedCorrupt = true;
                    _notifications.Log("[Wallet] Corrupt or mismatched wallet file on disk (" + _saveFileName + "); refusing to overwrite.");
                }
                return false;
            }

            List<string> records;
            if (!PersistenceCodec.TryParseStrict(raw, SchemaHeader, IsValidRecord, out records))
            {
                return false;
            }

            string loadedSlot = null;
            int loadedBalance = -1;
            for (int i = 0; i < records.Count; i++)
            {
                string line = records[i];
                if (line.StartsWith("SLOT=", StringComparison.Ordinal))
                {
                    loadedSlot = line.Substring("SLOT=".Length).Trim();
                }
                else if (line.StartsWith("BALANCE=", StringComparison.Ordinal))
                {
                    int val;
                    if (int.TryParse(line.Substring("BALANCE=".Length), out val) && val >= 0)
                    {
                        loadedBalance = val;
                    }
                }
            }

            if (!string.Equals(loadedSlot, _slotFingerprint, StringComparison.OrdinalIgnoreCase) || loadedBalance < 0)
            {
                _loadFailedCorrupt = true;
                _notifications.Log("[Wallet] Save-slot mismatch or invalid balance in " + _saveFileName + "; refusing to overwrite.");
                return false;
            }

            _balance = loadedBalance;
            _loadFailedCorrupt = false;
            return true;
        }

        public int GetCash()
        {
            return _balance;
        }

        public bool TrySpend(int amount, string reason)
        {
            if (_loadFailedCorrupt) return false;
            if (amount < 0 || _balance < amount) return false;
            if (amount == 0) return true;

            int previous = _balance;
            _balance -= amount;
            try
            {
                PersistState();
                return true;
            }
            catch (Exception ex)
            {
                _balance = previous;
                _notifications.Log("[Wallet] Spend persist failed (" + reason + "), rolled back: " + ex.Message);
                return false;
            }
        }

        public void Credit(int amount, string reason)
        {
            if (_loadFailedCorrupt || amount <= 0) return;

            int previous = _balance;
            long next = (long)_balance + amount;
            _balance = next > int.MaxValue ? int.MaxValue : (int)next;
            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                _balance = previous;
                _notifications.Log("[Wallet] Credit persist failed (" + reason + "), rolled back: " + ex.Message);
            }
        }

        private void PersistState()
        {
            var records = new List<string>
            {
                "SLOT=" + _slotFingerprint,
                "BALANCE=" + _balance
            };
            _store.Save(_saveFileName, PersistenceCodec.Serialize(SchemaHeader, records), ValidatePayload);
        }

        private static bool ValidatePayload(string text)
        {
            List<string> records;
            if (!PersistenceCodec.TryParseStrict(text, SchemaHeader, IsValidRecord, out records))
            {
                return false;
            }
            bool hasSlot = false;
            bool hasBalance = false;
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].StartsWith("SLOT=", StringComparison.Ordinal)) hasSlot = true;
                else if (records[i].StartsWith("BALANCE=", StringComparison.Ordinal)) hasBalance = true;
            }
            return hasSlot && hasBalance;
        }

        private static bool IsValidRecord(string line)
        {
            if (line.StartsWith("SLOT=", StringComparison.Ordinal))
            {
                return line.Length > "SLOT=".Length;
            }
            if (line.StartsWith("BALANCE=", StringComparison.Ordinal))
            {
                int val;
                return int.TryParse(line.Substring("BALANCE=".Length), out val) && val >= 0;
            }
            return false;
        }
    }
}
