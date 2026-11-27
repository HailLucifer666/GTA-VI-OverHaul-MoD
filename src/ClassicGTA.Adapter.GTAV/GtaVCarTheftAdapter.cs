using ClassicGTA.Contracts;
using GTA;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVCarTheftAdapter : ICarTheftAdapter
    {
        public int GetTargetVehicleHandle()
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return 0;

            Vehicle target = player.VehicleTryingToEnter;
            if (target != null && target.Exists())
            {
                return target.Handle;
            }

            Vehicle[] nearby = World.GetNearbyVehicles(player.Position, 4.5f);
            if (nearby != null)
            {
                for (int i = 0; i < nearby.Length; i++)
                {
                    Vehicle v = nearby[i];
                    if (v != null && v.Exists() && v != player.CurrentVehicle && IsLockedStatus(v.LockStatus))
                    {
                        return v.Handle;
                    }
                }
            }

            return 0;
        }

        public bool IsVehicleLocked(int vehicleHandle)
        {
            Vehicle v = Entity.FromHandle(vehicleHandle) as Vehicle;
            return v != null && v.Exists() && IsLockedStatus(v.LockStatus);
        }

        public bool UnlockVehicle(int vehicleHandle)
        {
            Vehicle v = Entity.FromHandle(vehicleHandle) as Vehicle;
            if (v == null || !v.Exists()) return false;
            v.LockStatus = VehicleLockStatus.Unlocked;
            return true;
        }

        public void DisableAlarm(int vehicleHandle)
        {
            Vehicle v = Entity.FromHandle(vehicleHandle) as Vehicle;
            if (v != null && v.Exists())
            {
                v.IsAlarmSet = false;
            }
        }

        public void PacifyDriver(int vehicleHandle)
        {
            Ped player = Game.Player.Character;
            Vehicle v = Entity.FromHandle(vehicleHandle) as Vehicle;
            if (player == null || !player.Exists() || v == null || !v.Exists()) return;

            Ped driver = v.Driver;
            if (driver != null && driver.Exists() && driver != player && driver.IsAlive)
            {
                driver.Task.FleeFrom(player);
            }
        }

        private static bool IsLockedStatus(VehicleLockStatus status)
        {
            return status == VehicleLockStatus.CannotEnter ||
                   status == VehicleLockStatus.PlayerCannotEnter ||
                   status == VehicleLockStatus.PlayerCannotLeaveCanBeBrokenIntoPersist;
        }
    }
}
