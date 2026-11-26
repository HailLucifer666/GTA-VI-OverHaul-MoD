using System;
using System.Collections.Generic;
using System.Text;
using ClassicGTA.Contracts;

namespace ClassicGTA.Core.Economy
{
    public sealed class StockQuote
    {
        public string Symbol { get; private set; }
        public string CompanyName { get; private set; }
        public int CurrentPrice { get; set; }
        public int MinPrice { get; private set; }
        public int MaxPrice { get; private set; }
        public int MaxTickPercentMove { get; private set; }
        public int SharesOwned { get; set; }

        public StockQuote(
            string symbol,
            string companyName,
            int initialPrice,
            int minPrice,
            int maxPrice,
            int maxTickPercentMove)
        {
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol required.", "symbol");
            if (minPrice < 1 || maxPrice <= minPrice) throw new ArgumentOutOfRangeException("minPrice");
            if (initialPrice < minPrice || initialPrice > maxPrice) throw new ArgumentOutOfRangeException("initialPrice");
            if (maxTickPercentMove < 1 || maxTickPercentMove > 50) throw new ArgumentOutOfRangeException("maxTickPercentMove");

            Symbol = symbol.Trim().ToUpperInvariant();
            CompanyName = companyName ?? Symbol;
            CurrentPrice = initialPrice;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            MaxTickPercentMove = maxTickPercentMove;
        }
    }

    /// <summary>
    /// Classic Stock Exchange Simulator (Cut Feature #5 — LCN/BAWSAQ style).
    /// Supports deterministic market ticks, assassination/contract rival shocks,
    /// refund-safe share purchases, and atomic persistence.
    /// </summary>
    public sealed class StockMarketEngine
    {
        private const string SaveFileName = "ClassicGTA_Stocks.dat";
        private const string SchemaHeader = "STOCKS_SCHEMA=1";

        private readonly IGameContext _context;
        private readonly IEconomyService _economy;
        private readonly IPersistenceStore _store;
        private readonly INotificationService _notifications;
        private readonly Func<int, int, int> _nextRandom;

        private readonly Dictionary<string, StockQuote> _stocks =
            new Dictionary<string, StockQuote>(StringComparer.OrdinalIgnoreCase);

        public StockMarketEngine(
            IGameContext context,
            IEconomyService economy,
            IPersistenceStore store,
            INotificationService notifications,
            Func<int, int, int> randomRange = null)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (economy == null) throw new ArgumentNullException("economy");
            if (store == null) throw new ArgumentNullException("store");
            if (notifications == null) throw new ArgumentNullException("notifications");

            _context = context;
            _economy = economy;
            _store = store;
            _notifications = notifications;

            if (randomRange != null)
            {
                _nextRandom = randomRange;
            }
            else
            {
                var rng = new Random();
                _nextRandom = (min, max) => rng.Next(min, max + 1);
            }
        }

        public void RegisterStock(StockQuote quote)
        {
            if (quote == null) throw new ArgumentNullException("quote");
            if (_stocks.ContainsKey(quote.Symbol))
            {
                throw new InvalidOperationException("Duplicate stock symbol: " + quote.Symbol);
            }
            _stocks.Add(quote.Symbol, quote);
        }

        public StockQuote GetQuote(string symbol)
        {
            StockQuote quote;
            return !string.IsNullOrWhiteSpace(symbol) && _stocks.TryGetValue(symbol, out quote) ? quote : null;
        }

        public void AdvanceMarketTick()
        {
            if (_context.IsOnlineSession) return;

            foreach (var kv in _stocks)
            {
                StockQuote stock = kv.Value;
                int pct = _nextRandom(-stock.MaxTickPercentMove, stock.MaxTickPercentMove);
                ApplyPercentDelta(stock, pct);
            }

            // A-02 Fix: Persist market prices and share counts together so prices cannot revert on restart.
            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                _notifications.Log("[Stocks] AdvanceMarketTick persist failed: " + ex.Message);
            }
        }

        public void ApplyContractShock(string targetSymbol, int targetPercentDelta, string rivalSymbol, int rivalPercentDelta)
        {
            if (_context.IsOnlineSession) return;

            StockQuote target = GetQuote(targetSymbol);
            if (target != null)
            {
                ApplyPercentDelta(target, targetPercentDelta);
            }

            StockQuote rival = GetQuote(rivalSymbol);
            if (rival != null)
            {
                ApplyPercentDelta(rival, rivalPercentDelta);
            }

            PersistState();
            _notifications.Log("[Stocks] CONTRACT_SHOCK target=" + targetSymbol + "(" + targetPercentDelta + "%) rival=" + rivalSymbol + "(" + rivalPercentDelta + "%)");
        }

        public PurchaseResult TryBuyShares(string symbol, int quantity)
        {
            if (_context.IsOnlineSession)
            {
                return new PurchaseResult(PurchaseStatus.BlockedInOnline, "Blocked in Online session.");
            }
            if (quantity <= 0)
            {
                return new PurchaseResult(PurchaseStatus.InvalidQuantity, "Quantity must be positive.");
            }

            StockQuote stock = GetQuote(symbol);
            if (stock == null)
            {
                return new PurchaseResult(PurchaseStatus.UnknownAsset, "Unknown stock: " + symbol);
            }

            long totalLong = (long)stock.CurrentPrice * quantity;
            if (totalLong > int.MaxValue || totalLong <= 0)
            {
                return new PurchaseResult(PurchaseStatus.InvalidQuantity, "Order value out of range.");
            }

            int totalCost = (int)totalLong;
            if (_economy.GetCash() < totalCost || !_economy.TrySpend(totalCost, "BuyStock:" + stock.Symbol))
            {
                return new PurchaseResult(PurchaseStatus.InsufficientFunds, "Insufficient cash.");
            }

            stock.SharesOwned += quantity;
            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                stock.SharesOwned -= quantity;
                _economy.Credit(totalCost, "RefundStock:" + stock.Symbol);
                _notifications.Log("[Stocks] Buy persist failed, refunded: " + ex.Message);
                return new PurchaseResult(PurchaseStatus.PersistenceFailedAndRefunded, "Save failed; cash refunded.");
            }

            return new PurchaseResult(PurchaseStatus.Succeeded, "Bought " + quantity + " " + stock.Symbol + ".");
        }

        public PurchaseResult TrySellShares(string symbol, int quantity)
        {
            if (_context.IsOnlineSession)
            {
                return new PurchaseResult(PurchaseStatus.BlockedInOnline, "Blocked in Online session.");
            }
            if (quantity <= 0)
            {
                return new PurchaseResult(PurchaseStatus.InvalidQuantity, "Quantity must be positive.");
            }

            StockQuote stock = GetQuote(symbol);
            if (stock == null)
            {
                return new PurchaseResult(PurchaseStatus.UnknownAsset, "Unknown stock: " + symbol);
            }
            if (stock.SharesOwned < quantity)
            {
                return new PurchaseResult(PurchaseStatus.InvalidQuantity, "Not enough shares owned.");
            }

            long proceedsLong = (long)stock.CurrentPrice * quantity;
            int proceeds = proceedsLong > int.MaxValue ? int.MaxValue : (int)proceedsLong;

            stock.SharesOwned -= quantity;
            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                stock.SharesOwned += quantity;
                _notifications.Log("[Stocks] Sell persist failed: " + ex.Message);
                return new PurchaseResult(PurchaseStatus.PersistenceFailedAndRefunded, "Save failed; shares restored.");
            }

            _economy.Credit(proceeds, "SellStock:" + stock.Symbol);
            return new PurchaseResult(PurchaseStatus.Succeeded, "Sold " + quantity + " " + stock.Symbol + " for $" + proceeds + ".");
        }

        public bool LoadState()
        {
            string raw;
            if (!_store.TryLoad(SaveFileName, ValidatePayload, out raw))
            {
                return false;
            }

            List<string> records;
            if (!PersistenceCodec.TryParseStrict(raw, SchemaHeader, IsValidRecord, out records))
            {
                return false;
            }

            for (int i = 0; i < records.Count; i++)
            {
                string[] parts = records[i].Split('|');
                StockQuote quote = GetQuote(parts[0]);
                int price;
                int shares;
                if (quote != null && int.TryParse(parts[1], out price) && int.TryParse(parts[2], out shares))
                {
                    quote.CurrentPrice = Clamp(price, quote.MinPrice, quote.MaxPrice);
                    quote.SharesOwned = shares < 0 ? 0 : shares;
                }
            }

            return true;
        }

        private void PersistState()
        {
            var records = new List<string>(_stocks.Count);
            foreach (var kv in _stocks)
            {
                StockQuote q = kv.Value;
                records.Add(q.Symbol + "|" + q.CurrentPrice + "|" + q.SharesOwned);
            }
            _store.Save(SaveFileName, PersistenceCodec.Serialize(SchemaHeader, records), ValidatePayload);
        }

        private static bool ValidatePayload(string text)
        {
            return PersistenceCodec.ValidateStrict(text, SchemaHeader, IsValidRecord);
        }

        private static bool IsValidRecord(string line)
        {
            string[] parts = line.Split('|');
            if (parts.Length != 3 || string.IsNullOrWhiteSpace(parts[0])) return false;
            int price;
            int shares;
            return int.TryParse(parts[1], out price) && price > 0 &&
                   int.TryParse(parts[2], out shares) && shares >= 0;
        }

        private static void ApplyPercentDelta(StockQuote stock, int percentDelta)
        {
            long delta = ((long)stock.CurrentPrice * percentDelta) / 100L;
            if (delta == 0 && percentDelta != 0)
            {
                delta = percentDelta > 0 ? 1 : -1;
            }
            long next = stock.CurrentPrice + delta;
            stock.CurrentPrice = Clamp((int)Math.Max(int.MinValue, Math.Min(int.MaxValue, next)), stock.MinPrice, stock.MaxPrice);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
