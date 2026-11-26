using ClassicGTA.Contracts;
using GTA;
using GTA.Math;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVCompanionAdapter : ICompanionAdapter
    {
        public WorldPoint GetPlayerPosition()
        {
            Vector3 p = Game.Player.Character.Position;
            return new WorldPoint(p.X, p.Y, p.Z);
        }

        public bool TrySpawnCompanion(string companionId, WorldPoint nearPlayer, out int pedHandle, out int blipHandle)
        {
            pedHandle = 0;
            blipHandle = 0;

            PedHash hash = PedHash.LamarDavis;
            if (companionId == "buddy.bodyguard") hash = PedHash.Cop01SMY;
            else if (companionId == "buddy.chop_handler") hash = PedHash.Franklin;

            Vector3 spawnPos = new Vector3(nearPlayer.X + 3f, nearPlayer.Y + 3f, nearPlayer.Z);
            var model = new Model(hash);
            Ped buddy = World.CreatePed(model, spawnPos);
            model.MarkAsNoLongerNeeded();
            if (buddy == null || !buddy.Exists()) return false;

            buddy.Weapons.Give(WeaponHash.CombatPistol, 200, false, true);
            Game.Player.Character.PedGroup.Add(buddy, false);

            Blip blip = buddy.AddBlip();
            if (blip != null && blip.Exists())
            {
                blip.Color = BlipColor.Blue;
                blip.Name = "Buddy: " + companionId;
                blipHandle = blip.Handle;
            }

            pedHandle = buddy.Handle;
            return true;
        }

        public bool IsEntityAlive(int handle)
        {
            Entity e = Entity.FromHandle(handle);
            return e != null && e.Exists() && e.IsAlive;
        }

        public void TaskFollowPlayer(int pedHandle)
        {
            Ped ped = Entity.FromHandle(pedHandle) as Ped;
            if (ped != null && ped.Exists())
            {
                ped.Task.FollowToOffsetFromEntity(Game.Player.Character, new Vector3(2f, -2f, 0f), 2.5f, -1, 2f, true);
            }
        }

        public void TaskAssistInCombat(int pedHandle)
        {
            Ped ped = Entity.FromHandle(pedHandle) as Ped;
            if (ped != null && ped.Exists())
            {
                ped.Task.FightAgainstHatedTargets(40f);
            }
        }

        public void TaskDismiss(int pedHandle)
        {
            Ped ped = Entity.FromHandle(pedHandle) as Ped;
            if (ped != null && ped.Exists())
            {
                ped.LeaveGroup();
                ped.Task.WanderAround();
            }
        }
    }
}
