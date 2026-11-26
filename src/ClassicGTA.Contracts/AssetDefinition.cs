using System;

namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Stable ContentId-keyed catalog entry for safehouses, businesses, boats, aircraft, and submarines.
    /// </summary>
    public sealed class AssetDefinition
    {
        public string ContentId { get; private set; }
        public string DisplayName { get; private set; }
        public AssetCategory Category { get; private set; }
        public int Price { get; private set; }
        public int DailyPassiveIncome { get; private set; }

        public AssetDefinition(
            string contentId,
            string displayName,
            AssetCategory category,
            int price,
            int dailyPassiveIncome = 0)
        {
            if (string.IsNullOrWhiteSpace(contentId)) throw new ArgumentException("ContentId required.", "contentId");
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("DisplayName required.", "displayName");
            if (price < 0) throw new ArgumentOutOfRangeException("price");
            if (dailyPassiveIncome < 0) throw new ArgumentOutOfRangeException("dailyPassiveIncome");

            ContentId = contentId.Trim();
            DisplayName = displayName.Trim();
            Category = category;
            Price = price;
            DailyPassiveIncome = dailyPassiveIncome;
        }
    }
}
