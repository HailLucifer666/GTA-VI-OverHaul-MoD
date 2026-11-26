using ClassicGTA.Contracts;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVEmergencyDispatchAdapter : IEmergencyDispatchAdapter
    {
        private const ulong HashStopFireInRange = 0x056A8A219B8E829FUL;

        public WorldPoint GetPlayerPosition()
        {
            Vector3 p = Game.Player.Character.Position;
            return new WorldPoint(p.X, p.Y, p.Z);
        }

        public bool TryFindRoadSpawnPoint(WorldPoint target, float minDistance, float maxDistance, out WorldPoint spawnPoint)
        {
            Vector3 offset = new Vector3(target.X + minDistance, target.Y + (minDistance * 0.5f), target.Z);
            Vector3 roadPos = World.GetNextPositionOnStreet(offset);
            spawnPoint = new WorldPoint(roadPos.X, roadPos.Y, roadPos.Z);
            return roadPos.LengthSquared() > 1f;
        }

        public DispatchSpawnResult SpawnUnit(EmergencyServiceType service, WorldPoint spawnPoint)
        {
            VehicleHash vehHash;
            PedHash pedHash;
            BlipColor blipColor;

            if (service == EmergencyServiceType.Ambulance)
            {
                vehHash = VehicleHash.Ambulance;
                pedHash = PedHash.Paramedic01SMM;
                blipColor = BlipColor.Green;
            }
            else if (service == EmergencyServiceType.FireDepartment)
            {
                vehHash = VehicleHash.FireTruck;
                pedHash = PedHash.Fireman01SMY;
                blipColor = BlipColor.Red;
            }
            else
            {
                vehHash = VehicleHash.Police;
                pedHash = PedHash.Cop01SMY;
                blipColor = BlipColor.Blue;
            }

            var vehModel = new Model(vehHash);
            var pedModel = new Model(pedHash);

            Vehicle veh = World.CreateVehicle(vehModel, new Vector3(spawnPoint.X, spawnPoint.Y, spawnPoint.Z));
            vehModel.MarkAsNoLongerNeeded();
            if (veh == null || !veh.Exists())
            {
                pedModel.MarkAsNoLongerNeeded();
                return DispatchSpawnResult.Fail("Failed to spawn vehicle " + vehHash);
            }

            veh.IsSirenActive = true;
            Ped ped = veh.CreatePedOnSeat(VehicleSeat.Driver, pedModel);
            pedModel.MarkAsNoLongerNeeded();
            if (ped == null || !ped.Exists())
            {
                veh.Delete();
                return DispatchSpawnResult.Fail("Failed to spawn responder ped " + pedHash);
            }

            Blip blip = veh.AddBlip();
            int blipHandle = 0;
            if (blip != null && blip.Exists())
            {
                blip.Color = blipColor;
                blip.Name = "911 " + service;
                blipHandle = blip.Handle;
            }

            return DispatchSpawnResult.Success(veh.Handle, ped.Handle, blipHandle);
        }

        public void TaskDriveTo(int responderPedHandle, int vehicleHandle, WorldPoint destination)
        {
            Ped ped = Entity.FromHandle(responderPedHandle) as Ped;
            Vehicle veh = Entity.FromHandle(vehicleHandle) as Vehicle;
            if (ped != null && ped.Exists() && veh != null && veh.Exists())
            {
                ped.Task.DriveTo(veh, new Vector3(destination.X, destination.Y, destination.Z), 12f, 28f, DrivingStyle.Rushed);
            }
        }

        public bool TryGetEntityPosition(int handle, out WorldPoint position)
        {
            Entity entity = Entity.FromHandle(handle);
            if (entity == null || !entity.Exists())
            {
                position = new WorldPoint();
                return false;
            }
            Vector3 p = entity.Position;
            position = new WorldPoint(p.X, p.Y, p.Z);
            return true;
        }

        public bool IsEntityAlive(int handle)
        {
            Entity entity = Entity.FromHandle(handle);
            return entity != null && entity.Exists() && entity.IsAlive;
        }

        public void HealPlayerToFull()
        {
            Ped player = Game.Player.Character;
            if (player != null && player.Exists() && player.IsAlive)
            {
                player.Health = player.MaxHealth;
            }
        }

        public void TaskEngageNearbyHostiles(int responderPedHandle)
        {
            Ped ped = Entity.FromHandle(responderPedHandle) as Ped;
            if (ped != null && ped.Exists())
            {
                ped.Task.FightAgainstHatedTargets(40f);
            }
        }

        public bool TryExtinguishNearbyFires(WorldPoint center, float radius)
        {
            Function.Call((Hash)HashStopFireInRange, center.X, center.Y, center.Z, radius);
            return true;
        }

        public void TaskDepart(int responderPedHandle, int vehicleHandle)
        {
            Ped ped = Entity.FromHandle(responderPedHandle) as Ped;
            Vehicle veh = Entity.FromHandle(vehicleHandle) as Vehicle;
            if (ped != null && ped.Exists() && veh != null && veh.Exists())
            {
                veh.IsSirenActive = false;
                ped.Task.CruiseWithVehicle(veh, 18f, DrivingStyle.Normal);
            }
        }
    }
}
