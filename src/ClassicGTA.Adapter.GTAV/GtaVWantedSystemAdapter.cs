using ClassicGTA.Contracts;
using GTA;
using GTA.Native;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVWantedSystemAdapter : IWantedSystemAdapter
    {
        public int GetWantedLevel()
        {
            return Game.Player.WantedLevel;
        }

        public void SetWantedLevel(int level)
        {
            int clamped = level < 0 ? 0 : (level > 6 ? 6 : level);
            Game.Player.WantedLevel = clamped;
        }

        public bool ArePoliceSpottingPlayer()
        {
            try
            {
                Ped player = Game.Player.Character;
                if (player == null || !player.Exists()) return false;
                if (Game.Player.WantedLevel <= 0) return false;

                int copGroup = Game.GenerateHash("COP");
                Ped[] nearby = World.GetNearbyPeds(player.Position, 80f);
                if (nearby == null) return false;

                for (int i = 0; i < nearby.Length; i++)
                {
                    Ped ped = nearby[i];
                    if (ped == null || !ped.Exists() || !ped.IsAlive || ped == player) continue;

                    bool isPolice = ped.RelationshipGroup.Hash == copGroup || ped.IsInCombatAgainst(player);
                    if (!isPolice) continue;

                    if (Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, ped.Handle, player.Handle, 17))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Safe zero-throw fallback: treat as spotted if native probe fails.
                return true;
            }

            return false;
        }

        public void ClearCriminalProfileHeat()
        {
            // GTA V Legacy has no Criminal Profile Heat system; this seam is implemented natively
            // in ClassicGTA.Adapter.GTAVI on GTA VI PC Day 1 with zero Core changes.
            if (Game.Player.WantedLevel > 0)
            {
                Game.Player.WantedLevel = 0;
            }
        }

        public void SetMaxWantedLevel(int maxLevel)
        {
            int clamped = maxLevel < 1 ? 1 : (maxLevel > 6 ? 6 : maxLevel);
            Game.MaxWantedLevel = clamped;
        }
    }
}
