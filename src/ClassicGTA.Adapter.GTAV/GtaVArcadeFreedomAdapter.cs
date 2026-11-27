using ClassicGTA.Contracts;
using GTA;
using GTA.Native;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVArcadeFreedomAdapter : IArcadeFreedomAdapter
    {
        public void TopUpVehicleFuelAndHealth()
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return;

            Vehicle v = player.CurrentVehicle;
            if (v != null && v.Exists())
            {
                if (v.FuelLevel < 65f) v.FuelLevel = 65f;
                if (v.EngineHealth < 900f) v.EngineHealth = 1000f;
            }
        }

        public void RestorePlayerStaminaAndVitals()
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return;

            Function.Call(Hash.RESTORE_PLAYER_STAMINA, Game.Player, 100f);
        }

        public void SetVignetteSuppressed(bool suppressed)
        {
            if (suppressed)
            {
                Function.Call(Hash.ANIMPOSTFX_STOP_ALL);
            }
        }
    }
}
