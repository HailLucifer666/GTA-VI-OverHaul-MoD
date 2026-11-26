using ClassicGTA.Contracts;
using GTA;
using GTA.Math;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVPetAdapter : IPetAdapter
    {
        public WorldPoint GetPlayerPosition()
        {
            Vector3 p = Game.Player.Character.Position;
            return new WorldPoint(p.X, p.Y, p.Z);
        }

        public bool TrySpawnPet(string breedId, WorldPoint nearPlayer, out int petHandle, out int blipHandle)
        {
            petHandle = 0;
            blipHandle = 0;

            PedHash hash = PedHash.Rottweiler;
            if (breedId == "dog.shepherd") hash = PedHash.Shepherd;
            else if (breedId == "dog.husky") hash = PedHash.Husky;

            Vector3 spawnPos = new Vector3(nearPlayer.X + 2.5f, nearPlayer.Y + 2.5f, nearPlayer.Z);
            var model = new Model(hash);
            Ped dog = World.CreatePed(model, spawnPos);
            model.MarkAsNoLongerNeeded();
            if (dog == null || !dog.Exists()) return false;

            Game.Player.Character.PedGroup.Add(dog, false);

            Blip blip = dog.AddBlip();
            if (blip != null && blip.Exists())
            {
                blip.Color = BlipColor.Orange;
                blip.Name = "Guard Dog";
                blipHandle = blip.Handle;
            }

            petHandle = dog.Handle;
            return true;
        }

        public bool IsEntityAlive(int handle)
        {
            Entity e = Entity.FromHandle(handle);
            return e != null && e.Exists() && e.IsAlive;
        }

        public void IssueCommand(int petHandle, PetCommand command)
        {
            Ped dog = Entity.FromHandle(petHandle) as Ped;
            if (dog == null || !dog.Exists()) return;

            if (command == PetCommand.Follow)
            {
                dog.Task.FollowToOffsetFromEntity(Game.Player.Character, new Vector3(1.5f, -1.5f, 0f), 3.0f, -1, 1.5f, true);
            }
            else if (command == PetCommand.Sit)
            {
                dog.Task.StandStill(-1);
            }
            else if (command == PetCommand.Guard || command == PetCommand.AttackThreat)
            {
                dog.Task.FightAgainstHatedTargets(35f);
            }
            else if (command == PetCommand.Fetch)
            {
                dog.Task.RunTo(Game.Player.Character.Position);
            }
        }
    }
}
