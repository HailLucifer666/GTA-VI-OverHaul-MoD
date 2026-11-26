namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Behavioral adapter for Friend Hangouts / Companion AI (#24).
    /// </summary>
    public interface ICompanionAdapter
    {
        WorldPoint GetPlayerPosition();
        bool TrySpawnCompanion(string companionId, WorldPoint nearPlayer, out int pedHandle, out int blipHandle);
        bool IsEntityAlive(int handle);
        void TaskFollowPlayer(int pedHandle);
        void TaskAssistInCombat(int pedHandle);
        void TaskDismiss(int pedHandle);
    }
}
