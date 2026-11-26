namespace ClassicGTA.Adapter.GTAV
{
    /// <summary>
    /// Evaluates whether STOP_FIRE_IN_RANGE actually reduced active fires (I-07).
    /// Prevents unconditional true returns so Core's failure/no-fire branch is genuinely reachable.
    /// </summary>
    public static class FireExtinguishEvaluator
    {
        public static bool DidExtinguish(int firesBefore, int firesAfter)
        {
            return firesBefore > 0 && firesAfter < firesBefore;
        }
    }
}
