namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Three-way entity ownership model preventing accidental deletion of player/world entities.
    /// </summary>
    public enum EntityOwnership
    {
        /// <summary>Spawned by this mod; safe to delete or release on cleanup.</summary>
        Owned = 0,

        /// <summary>Player-owned or pre-existing world entity (e.g., player's taxi); NEVER delete.</summary>
        Borrowed = 1,

        /// <summary>Handed back to the game engine via MarkAsNoLongerNeeded; never force-delete.</summary>
        Detached = 2
    }
}
