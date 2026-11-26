namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Engine-agnostic logical keybindings used across Core modules.
    /// Adapters translate OS/game key codes into GameKey values.
    /// </summary>
    public enum GameKey
    {
        None = 0,
        F8 = 119,
        F9 = 120,
        F10 = 121,
        F11 = 122
    }
}
