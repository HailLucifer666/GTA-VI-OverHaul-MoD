using System;
using System.IO;
using System.Text;
using ClassicGTA.Contracts;

namespace ClassicGTA.Infrastructure
{
    /// <summary>
    /// Crash-safe atomic persistence store:
    ///   1. Writes to a unique per-write temp file (avoids concurrent .tmp collisions).
    ///   2. Rotates up to 3 backups (.bak1, .bak2, .bak3) before replacing primary file.
    ///   3. On Load, validates payload and automatically falls back through backups if corrupted.
    /// </summary>
    public sealed class AtomicFileStore : IPersistenceStore
    {
        private const int MaxBackups = 3;
        private readonly string _baseDirectory;
        private readonly INotificationService _logger;
        private readonly object _sync = new object();

        public AtomicFileStore(string baseDirectory, INotificationService logger)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory)) throw new ArgumentException("Directory required.", "baseDirectory");
            if (logger == null) throw new ArgumentNullException("logger");

            _baseDirectory = baseDirectory;
            _logger = logger;
        }

        public void Save(string fileName, string content)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name required.", "fileName");
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Refusing to persist empty content.", "content");

            lock (_sync)
            {
                Directory.CreateDirectory(_baseDirectory);

                string targetPath = Path.Combine(_baseDirectory, Path.GetFileName(fileName));
                string tempPath = targetPath + "." + Guid.NewGuid().ToString("N") + ".tmp";

                try
                {
                    using (var fs = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    using (var writer = new StreamWriter(fs, new UTF8Encoding(false)))
                    {
                        writer.Write(content);
                        writer.Flush();
                        fs.Flush(true);
                    }

                    if (File.Exists(targetPath))
                    {
                        RotateBackups(targetPath);
                        File.Copy(targetPath, targetPath + ".bak1", true);
                        File.Replace(tempPath, targetPath, null, true);
                    }
                    else
                    {
                        File.Move(tempPath, targetPath);
                    }
                }
                finally
                {
                    if (File.Exists(tempPath))
                    {
                        try { File.Delete(tempPath); } catch { /* best effort cleanup */ }
                    }
                }
            }
        }

        public bool TryLoad(string fileName, Func<string, bool> validator, out string content)
        {
            content = null;
            if (string.IsNullOrWhiteSpace(fileName)) return false;

            lock (_sync)
            {
                string targetPath = Path.Combine(_baseDirectory, Path.GetFileName(fileName));
                string[] candidates = new string[MaxBackups + 1];
                candidates[0] = targetPath;
                for (int i = 1; i <= MaxBackups; i++)
                {
                    candidates[i] = targetPath + ".bak" + i;
                }

                for (int i = 0; i < candidates.Length; i++)
                {
                    string path = candidates[i];
                    if (!File.Exists(path)) continue;

                    try
                    {
                        string text = File.ReadAllText(path, Encoding.UTF8);
                        if (validator == null || validator(text))
                        {
                            if (i > 0)
                            {
                                _logger.Log("[AtomicFileStore] Recovered " + fileName + " from backup .bak" + i);
                            }
                            content = text;
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Log("[AtomicFileStore] Failed reading " + Path.GetFileName(path) + ": " + ex.Message);
                    }
                }

                return false;
            }
        }

        private static void RotateBackups(string targetPath)
        {
            for (int i = MaxBackups; i >= 2; i--)
            {
                string prev = targetPath + ".bak" + (i - 1);
                string next = targetPath + ".bak" + i;
                if (File.Exists(prev))
                {
                    File.Copy(prev, next, true);
                }
            }
        }
    }
}
