using ClassicGTA.Contracts;
using GTA;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVEconomyService : IEconomyService
    {
        public int GetCash()
        {
            return Game.Player.Money;
        }

        public bool TrySpend(int amount, string reason)
        {
            if (amount < 0 || Game.Player.Money < amount) return false;
            Game.Player.Money -= amount;
            return true;
        }

        public void Credit(int amount, string reason)
        {
            if (amount <= 0) return;
            long next = (long)Game.Player.Money + amount;
            Game.Player.Money = next > int.MaxValue ? int.MaxValue : (int)next;
        }
    }
}
