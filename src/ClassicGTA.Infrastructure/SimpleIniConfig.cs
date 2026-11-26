using System;
using System.Collections.Generic;
using System.Text;
using ClassicGTA.Contracts;

namespace ClassicGTA.Infrastructure
{
    /// <summary>
    /// Simple INI config parser/writer with throttled flushing (zero per-tick disk I/O).
    /// </summary>
    public sealed class SimpleIniConfig
    {
        private readonly Dictionary<string, Dictionary<string, string>> _sections =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        private readonly IPersistenceStore _store;
        private readonly string _fileName;
        private readonly int _flushIntervalMs;

        private bool _isDirty;
        private bool _hasFlushed;
        private int _lastFlushMs;

        public bool IsDirty { get { return _isDirty; } }

        public SimpleIniConfig(IPersistenceStore store, string fileName, int flushIntervalMs = 5000)
        {
            if (store == null) throw new ArgumentNullException("store");
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name required.", "fileName");
            if (flushIntervalMs < 100) throw new ArgumentOutOfRangeException("flushIntervalMs");

            _store = store;
            _fileName = fileName;
            _flushIntervalMs = flushIntervalMs;
        }

        public bool Load()
        {
            string raw;
            if (!_store.TryLoad(_fileName, text => !string.IsNullOrWhiteSpace(text), out raw))
            {
                return false;
            }

            _sections.Clear();
            string currentSection = "General";
            string[] lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#")) continue;

                if (line.StartsWith("[") && line.EndsWith("]") && line.Length > 2)
                {
                    currentSection = line.Substring(1, line.Length - 2).Trim();
                    continue;
                }

                int eq = line.IndexOf('=');
                if (eq > 0)
                {
                    string key = line.Substring(0, eq).Trim();
                    string val = line.Substring(eq + 1).Trim();
                    SetInternal(currentSection, key, val);
                }
            }

            _isDirty = false;
            return true;
        }

        public string GetString(string section, string key, string defaultValue)
        {
            Dictionary<string, string> map;
            string value;
            if (_sections.TryGetValue(section, out map) && map.TryGetValue(key, out value))
            {
                return value;
            }
            return defaultValue;
        }

        public bool GetBool(string section, string key, bool defaultValue)
        {
            bool parsed;
            return bool.TryParse(GetString(section, key, string.Empty), out parsed) ? parsed : defaultValue;
        }

        public void Set(string section, string key, string value)
        {
            string existing = GetString(section, key, null);
            if (string.Equals(existing, value, StringComparison.Ordinal))
            {
                return;
            }

            SetInternal(section, key, value ?? string.Empty);
            _isDirty = true;
        }

        public bool FlushIfDirty(int gameTimeMs)
        {
            if (!_isDirty) return false;
            if (_hasFlushed && gameTimeMs >= _lastFlushMs && (gameTimeMs - _lastFlushMs) < _flushIntervalMs)
            {
                return false;
            }

            ForceFlush(gameTimeMs);
            return true;
        }

        public void ForceFlush(int gameTimeMs)
        {
            if (!_isDirty) return;

            var sb = new StringBuilder();
            foreach (var section in _sections)
            {
                sb.Append('[').Append(section.Key).AppendLine("]");
                foreach (var kv in section.Value)
                {
                    sb.Append(kv.Key).Append('=').AppendLine(kv.Value);
                }
                sb.AppendLine();
            }

            _store.Save(_fileName, sb.ToString());
            _isDirty = false;
            _hasFlushed = true;
            _lastFlushMs = gameTimeMs;
        }

        private void SetInternal(string section, string key, string value)
        {
            Dictionary<string, string> map;
            if (!_sections.TryGetValue(section, out map))
            {
                map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _sections[section] = map;
            }
            map[key] = value;
        }
    }
}
