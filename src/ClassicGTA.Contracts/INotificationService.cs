namespace ClassicGTA.Contracts
{
    /// <summary>
    /// UI notification and structured diagnostic logging surface.
    /// </summary>
    public interface INotificationService
    {
        void Show(string message);
        void Log(string message);
    }
}
