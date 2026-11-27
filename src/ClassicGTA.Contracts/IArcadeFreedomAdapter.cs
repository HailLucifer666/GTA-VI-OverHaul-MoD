namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Engine-agnostic adapter for Arcade Freedom (No Refuelling, No Survival Chores, No Vignette).
    /// </summary>
    public interface IArcadeFreedomAdapter
    {
        void TopUpVehicleFuelAndHealth();
        void RestorePlayerStaminaAndVitals();
        void SetVignetteSuppressed(bool suppressed);
    }
}
