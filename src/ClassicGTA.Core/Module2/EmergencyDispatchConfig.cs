using System;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Module2
{
    public sealed class EmergencyDispatchConfig
    {
        public bool Enabled { get; set; }
        public GameKey QuickDispatchKey { get; set; }
        public float MinSpawnDistance { get; set; }
        public float MaxSpawnDistance { get; set; }
        public float ArrivalRadiusMeters { get; set; }
        public float RetaskDistanceThreshold { get; set; }
        public float FireExtinguishRadius { get; set; }
        public int ArrivalTimeoutMs { get; set; }
        public int CooldownMs { get; set; }

        public EmergencyDispatchConfig()
        {
            Enabled = true;
            QuickDispatchKey = GameKey.F9;
            MinSpawnDistance = 60f;
            MaxSpawnDistance = 140f;
            ArrivalRadiusMeters = 15f;
            RetaskDistanceThreshold = 25f;
            FireExtinguishRadius = 25f;
            ArrivalTimeoutMs = 60000;
            CooldownMs = 10000;
        }

        public void Validate()
        {
            if (MinSpawnDistance <= 0f || MaxSpawnDistance <= MinSpawnDistance)
            {
                throw new ArgumentOutOfRangeException("MinSpawnDistance", "Spawn distance range is invalid.");
            }
            if (ArrivalRadiusMeters <= 0f)
            {
                throw new ArgumentOutOfRangeException("ArrivalRadiusMeters");
            }
            if (RetaskDistanceThreshold <= 0f)
            {
                throw new ArgumentOutOfRangeException("RetaskDistanceThreshold");
            }
            if (ArrivalTimeoutMs < 5000)
            {
                throw new ArgumentOutOfRangeException("ArrivalTimeoutMs");
            }
            if (CooldownMs < 0)
            {
                throw new ArgumentOutOfRangeException("CooldownMs");
            }
        }
    }
}
