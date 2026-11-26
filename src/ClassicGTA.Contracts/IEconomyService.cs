namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Integer-only cash service (eliminates floating-point currency drift).
    /// </summary>
    public interface IEconomyService
    {
        int GetCash();
        bool TrySpend(int amount, string reason);
        void Credit(int amount, string reason);
    }
}
