using System;
using ClassicGTA.Contracts;
using GTA;
using GTA.Math;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVTaxiAdapter : ITaxiAdapter
    {
        private readonly Random _rng = new Random();

        public bool IsPlayerInTaxi(out int vehicleHandle)
        {
            vehicleHandle = 0;
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists() || !player.IsInVehicle()) return false;

            Vehicle veh = player.CurrentVehicle;
            if (veh == null || !veh.Exists()) return false;

            if (veh.Model == VehicleHash.Taxi)
            {
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

        public bool TrySpawnFare(
            WorldPoint nearPlayer,
            out int farePedHandle,
            out WorldPoint pickupPosition,
            out WorldPoint dropoffPosition,
            out int blipHandle)
        {
            farePedHandle = 0;
            pickupPosition = new WorldPoint();
            dropoffPosition = new WorldPoint();
            blipHandle = 0;

            float angle1 = (float)(_rng.NextDouble() * Math.PI * 2.0);
            Vector3 pickupRaw = new Vector3(
                nearPlayer.X + (float)Math.Cos(angle1) * 90f,
                nearPlayer.Y + (float)Math.Sin(angle1) * 90f,
                nearPlayer.Z);
            Vector3 pickupPos = World.GetNextPositionOnSidewalk(pickupRaw);
            if (pickupPos.LengthSquared() < 1f) pickupPos = World.GetNextPositionOnStreet(pickupRaw);

            float angle2 = (float)(_rng.NextDouble() * Math.PI * 2.0);
            Vector3 dropRaw = new Vector3(
                pickupPos.X + (float)Math.Cos(angle2) * 650f,
                pickupPos.Y + (float)Math.Sin(angle2) * 650f,
                pickupPos.Z);
            Vector3 dropPos = World.GetNextPositionOnStreet(dropRaw);
            if (pickupPos.LengthSquared() <= 1f || dropPos.LengthSquared() <= 1f) return false;

            var pedModel = new Model(PedHash.Business01AMM);
            Ped fare = World.CreatePed(pedModel, pickupPos);
            pedModel.MarkAsNoLongerNeeded();
            if (fare == null || !fare.Exists()) return false;
            fare.IsPersistent = true;

            Blip blip = World.CreateBlip(pickupPos);
            if (blip != null && blip.Exists())
            {
                blip.Color = BlipColor.Yellow;
                blip.ShowRoute = true;
                blip.Name = "Taxi Fare";
                blipHandle = blip.Handle;
            }

            farePedHandle = fare.Handle;
            pickupPosition = new WorldPoint(pickupPos.X, pickupPos.Y, pickupPos.Z);
            dropoffPosition = new WorldPoint(dropPos.X, dropPos.Y, dropPos.Z);
            return true;
        }

        public bool IsFareInVehicle(int farePedHandle, int vehicleHandle)
        {
            Ped ped = Entity.FromHandle(farePedHandle) as Ped;
            Vehicle veh = Entity.FromHandle(vehicleHandle) as Vehicle;
            return ped != null && ped.Exists() && veh != null && veh.Exists() && ped.IsInVehicle(veh);
        }

        public bool IsEntityAlive(int handle)
        {
            Entity e = Entity.FromHandle(handle);
            return e != null && e.Exists() && e.IsAlive;
        }

        public void TaskFareEnterVehicle(int farePedHandle, int vehicleHandle)
        {
            Ped ped = Entity.FromHandle(farePedHandle) as Ped;
            Vehicle veh = Entity.FromHandle(vehicleHandle) as Vehicle;
            if (ped != null && ped.Exists() && veh != null && veh.Exists())
            {
                ped.Task.EnterVehicle(veh, VehicleSeat.RightRear);
            }
        }

        public void TaskFareLeaveVehicle(int farePedHandle, int vehicleHandle)
        {
            Ped ped = Entity.FromHandle(farePedHandle) as Ped;
            Vehicle veh = Entity.FromHandle(vehicleHandle) as Vehicle;
            if (ped != null && ped.Exists() && veh != null && veh.Exists())
            {
                ped.Task.LeaveVehicle(veh, LeaveVehicleFlags.None);
            }
        }

        public void SetRouteBlip(int blipHandle, WorldPoint destination)
        {
            var blip = new Blip(blipHandle);
            if (blip.Exists())
            {
                blip.Position = new Vector3(destination.X, destination.Y, destination.Z);
                blip.ShowRoute = true;
            }
        }
    }
}
