using System;
using ClassicGTA.Contracts;
using GTA;
using GTA.Math;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVEmergencySideJobAdapter : IEmergencySideJobAdapter
    {
        private readonly Random _rng = new Random();

        public bool IsPlayerInEmergencyVehicle(out EmergencyServiceType jobType, out int vehicleHandle)
        {
            jobType = EmergencyServiceType.Police;
            vehicleHandle = 0;

            Ped player = Game.Player.Character;
            if (player == null || !player.Exists() || !player.IsInVehicle()) return false;

            Vehicle veh = player.CurrentVehicle;
            if (veh == null || !veh.Exists()) return false;

            Model model = veh.Model;
            if (model == VehicleHash.Ambulance)
            {
                jobType = EmergencyServiceType.Ambulance;
                vehicleHandle = veh.Handle;
                return true;
            }
            if (model == VehicleHash.FireTruck)
            {
                jobType = EmergencyServiceType.FireDepartment;
                vehicleHandle = veh.Handle;
                return true;
            }
            if (model == VehicleHash.Police || model == VehicleHash.Police2 ||
                model == VehicleHash.Police3 || model == VehicleHash.Police4 ||
                model == VehicleHash.Sheriff || model == VehicleHash.Sheriff2 ||
                model == VehicleHash.FBI || model == VehicleHash.FBI2 ||
                model == VehicleHash.Policeb)
            {
                jobType = EmergencyServiceType.Police;
                vehicleHandle = veh.Handle;
                return true;
            }

            return false;
        }

        public WorldPoint GetPlayerPosition()
        {
            Vector3 p = Game.Player.Character.Position;
            return new WorldPoint(p.X, p.Y, p.Z);
        }

        public bool TrySpawnWaveTarget(
            EmergencyServiceType jobType,
            int level,
            out int targetHandle,
            out WorldPoint location,
            out int blipHandle)
        {
            targetHandle = 0;
            location = new WorldPoint();
            blipHandle = 0;

            Vector3 p = Game.Player.Character.Position;
            float angle = (float)(_rng.NextDouble() * Math.PI * 2.0);
            float dist = 140f + (level * 15f);
            Vector3 raw = new Vector3(p.X + (float)Math.Cos(angle) * dist, p.Y + (float)Math.Sin(angle) * dist, p.Z);
            Vector3 streetPos = World.GetNextPositionOnStreet(raw);

            PedHash modelHash = jobType == EmergencyServiceType.Police
                ? PedHash.BallaOrig01GMY
                : PedHash.Business01AMM;

            var model = new Model(modelHash);
            Ped target = World.CreatePed(model, streetPos);
            model.MarkAsNoLongerNeeded();
            if (target == null || !target.Exists()) return false;

            if (jobType == EmergencyServiceType.Police)
            {
                target.Weapons.Give(WeaponHash.Pistol, 120, true, true);
                target.Task.FightAgainst(Game.Player.Character);
            }

            Blip blip = target.AddBlip();
            if (blip != null && blip.Exists())
            {
                blip.Color = jobType == EmergencyServiceType.Police ? BlipColor.Red : BlipColor.Green;
                blip.ShowRoute = true;
                blip.Name = jobType + " Target L" + level;
                blipHandle = blip.Handle;
            }

            targetHandle = target.Handle;
            location = new WorldPoint(streetPos.X, streetPos.Y, streetPos.Z);
            return true;
        }

        public bool IsTargetResolved(EmergencyServiceType jobType, int targetHandle, WorldPoint playerPosition)
        {
            Entity target = Entity.FromHandle(targetHandle);
            if (target == null || !target.Exists()) return true;

            if (jobType == EmergencyServiceType.Police)
            {
                return target.IsDead;
            }

            Vector3 tPos = target.Position;
            var tPoint = new WorldPoint(tPos.X, tPos.Y, tPos.Z);
            return tPoint.DistanceTo(playerPosition) <= 15f;
        }

        public bool IsEntityAlive(int handle)
        {
            Entity e = Entity.FromHandle(handle);
            return e != null && e.Exists() && e.IsAlive;
        }
    }
}
