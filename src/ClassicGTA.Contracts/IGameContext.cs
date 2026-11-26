namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Read-only snapshot of live game state used to gate module execution safely.
    /// </summary>
    public interface IGameContext
    {
        bool IsPlayerReady { get; }
        int PlayerHandle { get; }
        bool IsInCutscene { get; }
        bool IsPaused { get; }
        bool IsLoading { get; }
        bool IsOnlineSession { get; }
        bool IsInVehicle { get; }
        bool IsScreenFadedOut { get; }
        int GameTimeMs { get; }
    }
}
