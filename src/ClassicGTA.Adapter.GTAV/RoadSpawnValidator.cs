using ClassicGTA.Contracts;

namespace ClassicGTA.Adapter.GTAV
{
    /// <summary>
    /// Enforces the documented 911 road-spawn distance contract [minDistance, maxDistance] (I-07).
    /// </summary>
    public static class RoadSpawnValidator
    {
        public static bool IsWithinSpawnRange(WorldPoint candidate, WorldPoint target, float minDistance, float maxDistance)
        {
            if (minDistance <= 0f || maxDistance < minDistance) return false;
            float actualDist = candidate.DistanceTo(target);
            return actualDist >= minDistance && actualDist <= maxDistance;
        }
    }
}
