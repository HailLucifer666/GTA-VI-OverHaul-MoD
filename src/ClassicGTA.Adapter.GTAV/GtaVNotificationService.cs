using System;
using System.IO;
using ClassicGTA.Contracts;
using GTA.UI;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVNotificationService : INotificationService
    {
        private readonly string _logPath;

        public GtaVNotificationService(string logPath)
        {
            _logPath = logPath;
        }

        public void Show(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Notification.Show(message);
            }
        }

        public void Log(string message)
        {
            if (string.IsNullOrEmpty(_logPath) || string.IsNullOrEmpty(message)) return;
            try
            {
                string dir = Path.GetDirectoryName(_logPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(_logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + message + Environment.NewLine);
            }
            catch
            {
                // Logging must never crash the script thread.
            }
        }
    }
}
