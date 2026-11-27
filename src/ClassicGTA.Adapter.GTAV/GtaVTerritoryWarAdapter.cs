using System;
using ClassicGTA.Contracts;
using GTA;
using GTA.Math;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVTerritoryWarAdapter : ITerritoryWarAdapter
    {
        private readonly Random _rng = new Random();

        public int SpawnWaveEnemy(string zoneId, int waveNumber)
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return 0;

            double angle = _rng.NextDouble() * Math.PI * 2.0;
            float dist = 25f + (float)(_rng.NextDouble() * 20.0);
            Vector3 offset = new Vector3((float)(Math.Cos(angle) * dist), (float)(Math.Sin(angle) * dist), 0f);
            Vector3 spawnPos = World.GetNextPositionOnStreet(player.Position + offset);

            PedHash hash = waveNumber == 1 ? PedHash.BallaOrig01GMY :
                           (waveNumber == 2 ? PedHash.BallaEast01GMY : PedHash.BallaSout01GMY);

            var model = new Model(hash);
            if (!model.IsInCdImage || !model.IsValid) return 0;
            Ped enemy = World.CreatePed(model, spawnPos);
            model.MarkAsNoLongerNeeded();
            if (enemy == null || !enemy.Exists()) return 0;

            RelationshipGroup enemyGroup = World.AddRelationshipGroup("CLASSIC_TURF_ENEMY");
            enemy.RelationshipGroup = enemyGroup;
            enemyGroup.SetRelationshipBetweenGroups(player.RelationshipGroup, Relationship.Hate, true);

            WeaponHash weapon = waveNumber >= 3 ? WeaponHash.CarbineRifle :
                                (waveNumber == 2 ? WeaponHash.MicroSMG : WeaponHash.Pistol);
            enemy.Weapons.Give(weapon, 250, true, true);
            enemy.Task.FightAgainst(player);

            return enemy.Handle;
        }

        public bool IsEntityAlive(int entityHandle)
        {
            Entity e = Entity.FromHandle(entityHandle);
            return e != null && e.Exists() && e.IsAlive;
        }

        public int CreateZoneBlip(string zoneId)
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return 0;

            Blip blip = World.CreateBlip(player.Position);
            if (blip == null || !blip.Exists()) return 0;
            blip.Color = BlipColor.Red;
            blip.Name = "Turf War: " + (zoneId ?? "Zone");
            return blip.Handle;
        }
    }
}
