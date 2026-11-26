using GTA;
using GTA.Math;
using GTA.Native;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVSubmersibleAdapter : ISubmersibleAdapter
    {
        private const ulong HashTestVerticalProbeAgainstAllWater = 0x2B3451FA1E3142E2UL;

        public bool IsSuitableWaterLocation()
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return false;
            if (player.IsInWater || player.IsSwimming) return true;

            Vector3 spawnPos = player.Position + (player.ForwardVector * 8f);
            var waterHeightArg = new OutputArgument();
            bool hasWater = Function.Call<bool>(
                (Hash)HashTestVerticalProbeAgainstAllWater,
                spawnPos.X, spawnPos.Y, spawnPos.Z + 10f,
                0,
                waterHeightArg);
            return hasWater;
        }

        public int SpawnKrakenNearPlayer()
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return 0;

            Vector3 spawnPos = player.Position + (player.ForwardVector * 8f);
            var model = new Model(VehicleHash.Submersible2);
            Vehicle sub = World.CreateVehicle(model, spawnPos, player.Heading);
            model.MarkAsNoLongerNeeded();
            if (sub == null || !sub.Exists()) return 0;
            return sub.Handle;
        }
    }
}
