using System;
using System.Collections.Generic;
using System.Text;

namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Strict line-oriented envelope codec for all ClassicGTA persisted state files (I-04).
    /// Format:
    ///   &lt;SCHEMA_HEADER&gt;
    ///   &lt;record lines...&gt;
    ///   RECORD_COUNT=&lt;N&gt;
    ///   END_OF_FILE
    /// Rejects truncated, missing-footer, mismatched-count, or malformed-record payloads so
    /// AtomicFileStore automatically falls back to the newest complete backup.
    /// </summary>
    public static class PersistenceCodec
    {
        public const string RecordCountPrefix = "RECORD_COUNT=";
        public const string Footer = "END_OF_FILE";

        public static string Serialize(string schemaHeader, IList<string> records)
        {
            if (string.IsNullOrWhiteSpace(schemaHeader)) throw new ArgumentException("Schema header required.", "schemaHeader");

            var sb = new StringBuilder();
            sb.AppendLine(schemaHeader);
            int count = 0;
            if (records != null)
            {
                for (int i = 0; i < records.Count; i++)
                {
                    string line = records[i];
                    if (line == null) continue;
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0) continue;
                    sb.AppendLine(trimmed);
                    count++;
                }
            }
            sb.Append(RecordCountPrefix).AppendLine(count.ToString());
            sb.AppendLine(Footer);
            return sb.ToString();
        }

        public static bool ValidateStrict(string raw, string expectedHeader, Func<string, bool> recordValidator)
        {
            List<string> records;
            return TryParseStrict(raw, expectedHeader, recordValidator, out records);
        }

        public static bool TryParseStrict(
            string raw,
            string expectedHeader,
            Func<string, bool> recordValidator,
            out List<string> records)
        {
            records = null;
            if (string.IsNullOrWhiteSpace(raw) || string.IsNullOrWhiteSpace(expectedHeader))
            {
                return false;
            }

            string[] lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 3)
            {
                return false;
            }

            if (!string.Equals(lines[0].Trim(), expectedHeader, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(lines[lines.Length - 1].Trim(), Footer, StringComparison.Ordinal))
            {
                return false;
            }

            string countLine = lines[lines.Length - 2].Trim();
            if (!countLine.StartsWith(RecordCountPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            int declaredCount;
            if (!int.TryParse(countLine.Substring(RecordCountPrefix.Length), out declaredCount) || declaredCount < 0)
            {
                return false;
            }

            int bodyCount = lines.Length - 3;
            if (bodyCount != declaredCount)
            {
                return false;
            }

            var parsed = new List<string>(bodyCount);
            for (int i = 1; i <= bodyCount; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0)
                {
                    return false;
                }
                if (recordValidator != null && !recordValidator(line))
                {
                    return false;
                }
                parsed.Add(line);
            }

            records = parsed;
            return true;
        }

        public static string SanitizeSlotId(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId)) return "default";
            string trimmed = slotId.Trim();
            var sb = new StringBuilder(trimmed.Length);
            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('_');
                }
            }
            return sb.Length == 0 ? "default" : sb.ToString();
        }
    }
}
