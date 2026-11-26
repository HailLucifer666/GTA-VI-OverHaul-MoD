namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Behavioral adapter for Purchasable &amp; Walkable Pets / Guard Dogs (#26).
    /// </summary>
    public interface IPetAdapter
    {
        WorldPoint GetPlayerPosition();
        bool TrySpawnPet(string breedId, WorldPoint nearPlayer, out int petHandle, out int blipHandle);
        bool IsEntityAlive(int handle);
        void IssueCommand(int petHandle, PetCommand command);
    }
}
