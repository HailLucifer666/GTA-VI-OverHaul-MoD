using System;
using ClassicGTA.Contracts;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVSandboxCheatAdapter : ISandboxCheatAdapter
    {
        public void RefillHealthAndArmor()
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return;

            player.Health = player.MaxHealth;
            player.Armor = 100;
        }

        public void ClearWantedLevel()
        {
            Game.Player.WantedLevel = 0;
        }

        public void SetNeverWanted(bool enabled)
        {
            if (enabled)
            {
                Game.Player.WantedLevel = 0;
                Game.MaxWantedLevel = 0;
            }
            else if (Game.MaxWantedLevel == 0)
            {
                Game.MaxWantedLevel = 5;
            }
        }

        public void ApplySuperJumpFrame()
        {
            Function.Call(Hash.SET_SUPER_JUMP_THIS_FRAME, Game.Player);
        }

        public bool RepairCurrentVehicle()
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return false;

            Vehicle v = player.CurrentVehicle;
            if (v == null || !v.Exists()) return false;

            v.Repair();
            return true;
        }

        public int SpawnClassicVehicle(string modelId)
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists() || string.IsNullOrWhiteSpace(modelId)) return 0;

            Vector3 spawnPos = player.Position + (player.ForwardVector * 5.5f);
            var model = new Model(modelId.Trim());
            if (!model.IsInCdImage || !model.IsValid || !model.IsVehicle) return 0;
            Vehicle v = World.CreateVehicle(model, spawnPos, player.Heading);
            model.MarkAsNoLongerNeeded();
            if (v == null || !v.Exists()) return 0;
            return v.Handle;
        }

        public void SetWeather(string weatherId)
        {
            if (string.IsNullOrWhiteSpace(weatherId)) return;

            Weather weather;
            if (TryMapWeather(weatherId.Trim().ToUpperInvariant(), out weather))
            {
                World.Weather = weather;
            }
        }

        public void AdvanceTimeHours(int hours)
        {
            if (hours <= 0) return;
            World.CurrentTimeOfDay = World.CurrentTimeOfDay.Add(TimeSpan.FromHours(hours));
        }

        private static bool TryMapWeather(string id, out Weather weather)
        {
            switch (id)
            {
                case "EXTRASUNNY": weather = Weather.ExtraSunny; return true;
                case "CLEAR": weather = Weather.Clear; return true;
                case "CLOUDS": weather = Weather.Clouds; return true;
                case "OVERCAST": weather = Weather.Overcast; return true;
                case "RAIN": weather = Weather.Raining; return true;
                case "THUNDER": weather = Weather.ThunderStorm; return true;
                case "CLEARING": weather = Weather.Clearing; return true;
                case "FOGGY": weather = Weather.Foggy; return true;
                case "XMAS": weather = Weather.Christmas; return true;
                default: weather = Weather.Clear; return false;
            }
        }
    }
}
