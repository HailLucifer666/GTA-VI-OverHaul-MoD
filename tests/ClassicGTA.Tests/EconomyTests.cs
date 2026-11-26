using System;
using System.Collections.Generic;
using System.IO;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Economy;
using ClassicGTA.Infrastructure;

namespace ClassicGTA.Tests
{
    internal sealed class FakeEconomyService : IEconomyService
    {
        public int Cash { get; set; }
        public int SpendCalls { get; private set; }
        public int CreditCalls { get; private set; }

        public FakeEconomyService(int startingCash)
        {
            Cash = startingCash;
        }

        public int GetCash() { return Cash; }

        public bool TrySpend(int amount, string reason)
        {
            SpendCalls++;
            if (amount < 0 || Cash < amount) return false;
            Cash -= amount;
            return true;
        }

        public void Credit(int amount, string reason)
        {
            CreditCalls++;
            Cash += amount;
        }
    }

    internal sealed class FailingPersistenceStore : IPersistenceStore
    {
        public void Save(string fileName, string content)
        {
            throw new IOException("Simulated disk full failure");
        }

        public bool TryLoad(string fileName, Func<string, bool> validator, out string content)
        {
            content = null;
            return false;
        }
    }

    internal static class EconomyTests
    {
        public static IEnumerable<KeyValuePair<string, Action>> GetAll()
        {
            yield return Pair("35_AssetEngine_PurchaseSucceeds_PersistsAndReloadsAcrossSessions", AssetEngine_PurchaseSucceeds_PersistsAndReloadsAcrossSessions);
            yield return Pair("36_AssetEngine_DuplicatePurchase_RejectedWithZeroDoubleCharge", AssetEngine_DuplicatePurchase_RejectedWithZeroDoubleCharge);
            yield return Pair("37_AssetEngine_InsufficientFunds_RejectedWithoutStateChange", AssetEngine_InsufficientFunds_RejectedWithoutStateChange);
            yield return Pair("38_AssetEngine_PersistenceFailure_RefundsCashAndRollsBackOwnership", AssetEngine_PersistenceFailure_RefundsCashAndRollsBackOwnership);
            yield return Pair("39_AssetEngine_OnlineSession_BlocksPurchaseAndPassiveIncome", AssetEngine_OnlineSession_BlocksPurchaseAndPassiveIncome);
            yield return Pair("40_AssetEngine_PassiveIncome_CapsAtOfflineCapDaysAndIsIdempotent", AssetEngine_PassiveIncome_CapsAtOfflineCapDaysAndIsIdempotent);
            yield return Pair("41_AssetEngine_PassiveIncome_IgnoresBackwardClockJump", AssetEngine_PassiveIncome_IgnoresBackwardClockJump);
            yield return Pair("42_StockMarket_BuyAndSell_UpdatesPortfolioAndCashAccurately", StockMarket_BuyAndSell_UpdatesPortfolioAndCashAccurately);
            yield return Pair("43_StockMarket_SellMoreThanOwned_Rejected", StockMarket_SellMoreThanOwned_Rejected);
            yield return Pair("44_StockMarket_BuyPersistenceFailure_RefundsCashAndRollsBackShares", StockMarket_BuyPersistenceFailure_RefundsCashAndRollsBackShares);
            yield return Pair("45_StockMarket_ContractShock_CrashesTargetAndSurgesRivalWithinBounds", StockMarket_ContractShock_CrashesTargetAndSurgesRivalWithinBounds);
            yield return Pair("46_StockMarket_PersistsAndReloadsPortfolioState", StockMarket_PersistsAndReloadsPortfolioState);
        }

        private static KeyValuePair<string, Action> Pair(string name, Action body)
        {
            return new KeyValuePair<string, Action>(name, body);
        }

        private static void Assert(bool cond, string msg)
        {
            if (!cond) throw new InvalidOperationException(msg);
        }

        private static string CreateTempDir()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestScratch", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void DeleteDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }

        private static void SeedCatalog(AssetOwnershipEngine engine)
        {
            engine.RegisterAsset(new AssetDefinition("safehouse.vice_beach_condo", "Vice Beach Condo", AssetCategory.Safehouse, 50000));
            engine.RegisterAsset(new AssetDefinition("business.malibu_club", "Malibu Nightclub", AssetCategory.Business, 120000, 10000));
            engine.RegisterAsset(new AssetDefinition("sub.kraken_classic", "Classic Submersible", AssetCategory.Submarine, 250000));
        }

        private static void AssetEngine_PurchaseSucceeds_PersistsAndReloadsAcrossSessions()
        {
            string dir = CreateTempDir();
            try
            {
                var ctx = new FakeGameContext();
                var econ = new FakeEconomyService(300000);
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);

                var engine = new AssetOwnershipEngine(ctx, econ, store, notify);
                SeedCatalog(engine);

                PurchaseResult r1 = engine.TryPurchase("safehouse.vice_beach_condo");
                PurchaseResult r2 = engine.TryPurchase("sub.kraken_classic");

                Assert(r1.Succeeded && r2.Succeeded, "Expected both purchases to succeed.");
                Assert(econ.Cash == 0, "Expected exact $300,000 spent, remaining=" + econ.Cash);

                // Reload in fresh engine instance
                var reloaded = new AssetOwnershipEngine(ctx, econ, store, notify);
                SeedCatalog(reloaded);
                Assert(reloaded.LoadState(), "Expected LoadState to succeed.");
                Assert(reloaded.IsOwned("safehouse.vice_beach_condo") && reloaded.IsOwned("sub.kraken_classic"),
                    "Owned assets must persist across sessions.");
            }
            finally { DeleteDir(dir); }
        }

        private static void AssetEngine_DuplicatePurchase_RejectedWithZeroDoubleCharge()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(200000);
                var notify = new FakeNotificationService();
                var engine = new AssetOwnershipEngine(new FakeGameContext(), econ, new AtomicFileStore(dir, notify), notify);
                SeedCatalog(engine);

                Assert(engine.TryPurchase("safehouse.vice_beach_condo").Succeeded, "First purchase should succeed.");
                PurchaseResult dup = engine.TryPurchase("safehouse.vice_beach_condo");

                Assert(dup.Status == PurchaseStatus.AlreadyOwned, "Duplicate purchase must return AlreadyOwned.");
                Assert(econ.Cash == 150000 && econ.SpendCalls == 1, "Must NEVER double-charge on duplicate purchase.");
            }
            finally { DeleteDir(dir); }
        }

        private static void AssetEngine_InsufficientFunds_RejectedWithoutStateChange()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(10000);
                var notify = new FakeNotificationService();
                var engine = new AssetOwnershipEngine(new FakeGameContext(), econ, new AtomicFileStore(dir, notify), notify);
                SeedCatalog(engine);

                PurchaseResult res = engine.TryPurchase("safehouse.vice_beach_condo");
                Assert(res.Status == PurchaseStatus.InsufficientFunds, "Expected InsufficientFunds.");
                Assert(!engine.IsOwned("safehouse.vice_beach_condo") && econ.Cash == 10000, "State and cash must not change.");
            }
            finally { DeleteDir(dir); }
        }

        private static void AssetEngine_PersistenceFailure_RefundsCashAndRollsBackOwnership()
        {
            var econ = new FakeEconomyService(100000);
            var notify = new FakeNotificationService();
            var engine = new AssetOwnershipEngine(new FakeGameContext(), econ, new FailingPersistenceStore(), notify);
            SeedCatalog(engine);

            PurchaseResult res = engine.TryPurchase("safehouse.vice_beach_condo");

            Assert(res.Status == PurchaseStatus.PersistenceFailedAndRefunded, "Expected PersistenceFailedAndRefunded.");
            Assert(econ.Cash == 100000, "Cash MUST be refunded when persistence fails, got " + econ.Cash);
            Assert(!engine.IsOwned("safehouse.vice_beach_condo"), "Ownership MUST be rolled back when persistence fails.");
        }

        private static void AssetEngine_OnlineSession_BlocksPurchaseAndPassiveIncome()
        {
            string dir = CreateTempDir();
            try
            {
                var ctx = new FakeGameContext { IsOnlineSession = true };
                var econ = new FakeEconomyService(500000);
                var notify = new FakeNotificationService();
                var engine = new AssetOwnershipEngine(ctx, econ, new AtomicFileStore(dir, notify), notify);
                SeedCatalog(engine);

                Assert(engine.TryPurchase("business.malibu_club").Status == PurchaseStatus.BlockedInOnline,
                    "Purchase must be blocked in Online.");
                Assert(engine.CollectPassiveIncome(10) == 0, "Passive income must be blocked in Online.");
            }
            finally { DeleteDir(dir); }
        }

        private static void AssetEngine_PassiveIncome_CapsAtOfflineCapDaysAndIsIdempotent()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(200000);
                var notify = new FakeNotificationService();
                var engine = new AssetOwnershipEngine(new FakeGameContext(), econ, new AtomicFileStore(dir, notify), notify, 7);
                SeedCatalog(engine);

                Assert(engine.TryPurchase("business.malibu_club").Succeeded, "Business purchase should succeed.");

                // Initialize baseline day 10 -> 0 payout
                Assert(engine.CollectPassiveIncome(10) == 0, "First day baseline must pay 0.");

                // Jump 30 days ahead -> must cap at 7 days * $10,000 = $70,000
                int payout = engine.CollectPassiveIncome(40);
                Assert(payout == 70000, "Expected 7-day cap payout of $70,000, got $" + payout);
                Assert(econ.Cash == 150000, "Expected cash $80,000 + $70,000 = $150,000, got $" + econ.Cash);

                // Calling again on day 40 must be idempotent (0 payout)
                Assert(engine.CollectPassiveIncome(40) == 0, "Repeat call on same in-game day must pay 0.");
            }
            finally { DeleteDir(dir); }
        }

        private static void AssetEngine_PassiveIncome_IgnoresBackwardClockJump()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(200000);
                var notify = new FakeNotificationService();
                var engine = new AssetOwnershipEngine(new FakeGameContext(), econ, new AtomicFileStore(dir, notify), notify);
                SeedCatalog(engine);
                engine.TryPurchase("business.malibu_club");

                engine.CollectPassiveIncome(20);
                Assert(engine.CollectPassiveIncome(15) == 0, "Backward clock jump must pay 0.");
            }
            finally { DeleteDir(dir); }
        }

        private static void StockMarket_BuyAndSell_UpdatesPortfolioAndCashAccurately()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(10000);
                var notify = new FakeNotificationService();
                var market = new StockMarketEngine(new FakeGameContext(), econ, new AtomicFileStore(dir, notify), notify, (a, b) => 0);
                market.RegisterStock(new StockQuote("LCN1", "Cluckin Bell", 100, 10, 500, 10));

                Assert(market.TryBuyShares("LCN1", 25).Succeeded, "Buy 25 shares at $100 should succeed.");
                Assert(econ.Cash == 7500 && market.GetQuote("LCN1").SharesOwned == 25, "Expected $7500 cash and 25 shares.");

                market.GetQuote("LCN1").CurrentPrice = 140;
                Assert(market.TrySellShares("LCN1", 10).Succeeded, "Sell 10 shares at $140 should succeed.");
                Assert(econ.Cash == 8900 && market.GetQuote("LCN1").SharesOwned == 15, "Expected $8900 cash and 15 shares.");
            }
            finally { DeleteDir(dir); }
        }

        private static void StockMarket_SellMoreThanOwned_Rejected()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(10000);
                var notify = new FakeNotificationService();
                var market = new StockMarketEngine(new FakeGameContext(), econ, new AtomicFileStore(dir, notify), notify);
                market.RegisterStock(new StockQuote("BWSQ", "Ammu-Nation", 100, 10, 500, 10));

                PurchaseResult res = market.TrySellShares("BWSQ", 5);
                Assert(res.Status == PurchaseStatus.InvalidQuantity, "Selling unowned shares must be rejected.");
                Assert(econ.Cash == 10000, "Cash must remain unchanged.");
            }
            finally { DeleteDir(dir); }
        }

        private static void StockMarket_BuyPersistenceFailure_RefundsCashAndRollsBackShares()
        {
            var econ = new FakeEconomyService(5000);
            var notify = new FakeNotificationService();
            var market = new StockMarketEngine(new FakeGameContext(), econ, new FailingPersistenceStore(), notify);
            market.RegisterStock(new StockQuote("RIVL", "Redwood", 100, 10, 500, 10));

            PurchaseResult res = market.TryBuyShares("RIVL", 10);
            Assert(res.Status == PurchaseStatus.PersistenceFailedAndRefunded, "Expected PersistenceFailedAndRefunded.");
            Assert(econ.Cash == 5000 && market.GetQuote("RIVL").SharesOwned == 0, "Cash and shares must roll back.");
        }

        private static void StockMarket_ContractShock_CrashesTargetAndSurgesRivalWithinBounds()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(10000);
                var notify = new FakeNotificationService();
                var market = new StockMarketEngine(new FakeGameContext(), econ, new AtomicFileStore(dir, notify), notify);
                market.RegisterStock(new StockQuote("REDW", "Redwood Cigarettes", 100, 20, 300, 10));
                market.RegisterStock(new StockQuote("DEBO", "Debonaire", 100, 20, 300, 10));

                market.ApplyContractShock("REDW", -40, "DEBO", 55);

                Assert(market.GetQuote("REDW").CurrentPrice == 60, "Expected REDW to drop to $60.");
                Assert(market.GetQuote("DEBO").CurrentPrice == 155, "Expected DEBO to surge to $155.");

                // Verify bounds clamp on extreme shock
                market.ApplyContractShock("REDW", -95, "DEBO", 500);
                Assert(market.GetQuote("REDW").CurrentPrice == 20, "Target must clamp at MinPrice ($20).");
                Assert(market.GetQuote("DEBO").CurrentPrice == 300, "Rival must clamp at MaxPrice ($300).");
            }
            finally { DeleteDir(dir); }
        }

        private static void StockMarket_PersistsAndReloadsPortfolioState()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(10000);
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);
                var market = new StockMarketEngine(new FakeGameContext(), econ, store, notify);
                market.RegisterStock(new StockQuote("LIFE", "Lifeinvader", 100, 10, 500, 10));

                market.TryBuyShares("LIFE", 12);
                market.ApplyContractShock("LIFE", -25, null, 0);

                var reloaded = new StockMarketEngine(new FakeGameContext(), econ, store, notify);
                reloaded.RegisterStock(new StockQuote("LIFE", "Lifeinvader", 100, 10, 500, 10));
                Assert(reloaded.LoadState(), "Expected stock portfolio LoadState to succeed.");
                Assert(reloaded.GetQuote("LIFE").SharesOwned == 12 && reloaded.GetQuote("LIFE").CurrentPrice == 75,
                    "Expected 12 shares at $75 after reload.");
            }
            finally { DeleteDir(dir); }
        }
    }
}
