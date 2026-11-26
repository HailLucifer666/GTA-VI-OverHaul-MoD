using System;
using System.Collections.Generic;
using System.Text;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Activities
{
    /// <summary>
    /// RPG Character Skill Bars (Cut Feature #20 — Stamina, Driving, Flying, Shooting, LungCapacity).
    /// Levels 0..100 with 100 XP per level, persisted atomically and exposing tangible perk multipliers.
    /// </summary>
    public sealed class RpgSkillEngine
    {
        private const string SaveFileName = "ClassicGTA_Skills.dat";
        private const string SchemaHeader = "SKILLS_SCHEMA=1";
        private const int MaxLevel = 100;
        private const int XpPerLevel = 100;

        private readonly IGameContext _context;
        private readonly IPersistenceStore _store;
        private readonly Dictionary<RpgSkillType, int> _totalXp = new Dictionary<RpgSkillType, int>();

        public RpgSkillEngine(IGameContext context, IPersistenceStore store)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (store == null) throw new ArgumentNullException("store");

            _context = context;
            _store = store;

            foreach (RpgSkillType skill in Enum.GetValues(typeof(RpgSkillType)))
            {
                _totalXp[skill] = 0;
            }
        }

        public int GetLevel(RpgSkillType skill)
        {
            int level = _totalXp[skill] / XpPerLevel;
            return level > MaxLevel ? MaxLevel : level;
        }

        public int AddXp(RpgSkillType skill, int xpAmount)
        {
            if (_context.IsOnlineSession) return GetLevel(skill);
            if (xpAmount <= 0) throw new ArgumentOutOfRangeException("xpAmount", "XP must be positive.");

            int maxTotalXp = MaxLevel * XpPerLevel;
            long next = (long)_totalXp[skill] + xpAmount;
            _totalXp[skill] = next > maxTotalXp ? maxTotalXp : (int)next;
            return GetLevel(skill);
        }

        /// <summary>Returns 1.0x at level 0 up to 2.0x at level 100.</summary>
        public float GetStaminaRegenMultiplier()
        {
            return 1.0f + (GetLevel(RpgSkillType.Stamina) / 100.0f);
        }

        /// <summary>Returns 0% recoil reduction at level 0 up to 35% at level 100.</summary>
        public int GetWeaponRecoilReductionPercent()
        {
            return (GetLevel(RpgSkillType.Shooting) * 35) / 100;
        }

        public void SaveState()
        {
            var records = new List<string>(_totalXp.Count);
            foreach (var kv in _totalXp)
            {
                records.Add(((int)kv.Key) + "|" + kv.Value);
            }
            _store.Save(SaveFileName, PersistenceCodec.Serialize(SchemaHeader, records), ValidatePayload);
        }

        public bool LoadState()
        {
            string raw;
            if (!_store.TryLoad(SaveFileName, ValidatePayload, out raw))
            {
                return false;
            }

            List<string> records;
            if (!PersistenceCodec.TryParseStrict(raw, SchemaHeader, IsValidRecord, out records))
            {
                return false;
            }

            for (int i = 0; i < records.Count; i++)
            {
                string[] parts = records[i].Split('|');
                int skillInt; int xp;
                if (int.TryParse(parts[0], out skillInt) && int.TryParse(parts[1], out xp) && Enum.IsDefined(typeof(RpgSkillType), skillInt))
                {
                    int maxXp = MaxLevel * XpPerLevel;
                    _totalXp[(RpgSkillType)skillInt] = xp < 0 ? 0 : (xp > maxXp ? maxXp : xp);
                }
            }
            return true;
        }

        private static bool ValidatePayload(string text)
        {
            return PersistenceCodec.ValidateStrict(text, SchemaHeader, IsValidRecord);
        }

        private static bool IsValidRecord(string line)
        {
            string[] parts = line.Split('|');
            int skillInt; int xp;
            return parts.Length == 2 &&
                   int.TryParse(parts[0], out skillInt) &&
                   Enum.IsDefined(typeof(RpgSkillType), skillInt) &&
                   int.TryParse(parts[1], out xp) && xp >= 0;
        }
    }
}
