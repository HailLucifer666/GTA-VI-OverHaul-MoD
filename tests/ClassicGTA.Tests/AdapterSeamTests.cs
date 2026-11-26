using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ClassicGTA.Adapter.GTAV;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Activities;
using ClassicGTA.Core.Economy;
using ClassicGTA.Core.Module1;
using ClassicGTA.Core.Module2;
using ClassicGTA.Infrastructure;

namespace ClassicGTA.Tests
{
    internal sealed class FakeSubmersibleAdapter : ISubmersibleAdapter
    {
        public bool InWater { get; set; }
        public int NextHandle { get; set; }
        public int SpawnCalls { get; private set; }

        public FakeSubmersibleAdapter()
        {
            InWater = true;
            NextHandle = 9001;
        }

        public bool IsSuitableWaterLocation() { return InWater; }

        public int SpawnKrakenNearPlayer()
        {
            SpawnCalls++;
            return NextHandle++;
        }
    }

    internal sealed class ToggleFailingPersistenceStore : IPersistenceStore
    {
        private readonly IPersistenceStore _inner;
        public bool FailSaves { get; set; }

        public ToggleFailingPersistenceStore(IPersistenceStore inner)
        {
            _inner = inner;
        }

        public void Save(string fileName, string content)
        {
            if (FailSaves) throw new IOException("Simulated disk write failure");
            _inner.Save(fileName, content);
        }

        public void Save(string fileName, string content, Func<string, bool> existingBackupValidator)
        {
            if (FailSaves) throw new IOException("Simulated disk write failure");
            _inner.Save(fileName, content, existingBackupValidator);
        }

        public bool TryLoad(string fileName, Func<string, bool> validator, out string content)
        {
            return _inner.TryLoad(fileName, validator, out content);
        }

        public bool Exists(string fileName)
        {
            return _inner.Exists(fileName);
        }
    }

    internal static class AdapterSeamTests
    {
        public static IEnumerable<KeyValuePair<string, Action>> GetAll()
        {
            yield return Pair("99_Host_TwoSlots_KeepSeparateWalletAndAssetFilesWithoutOverwrite", Host_TwoSlots_KeepSeparateWalletAndAssetFilesWithoutOverwrite);
            yield return Pair("100_Host_PurchaseAndReload_ReconcilesExactWalletAndOwnership", Host_PurchaseAndReload_ReconcilesExactWalletAndOwnership);
            yield return Pair("101_Host_LegacyV060Ledger_MigratesUnderProductionDefaultSlotAndSurvivesSaves", Host_LegacyV060Ledger_MigratesUnderProductionDefaultSlotAndSurvivesSaves);
            yield return Pair("102_Assets_CorruptLedgerOnDisk_NeverSilentlyOverwritten", Assets_CorruptLedgerOnDisk_NeverSilentlyOverwritten);
            yield return Pair("103_Host_UtcIncomeClock_PaysPurchaseToFirstCollectionCapsAt7dAndRejectsBackwardClock", Host_UtcIncomeClock_PaysPurchaseToFirstCollectionCapsAt7dAndRejectsBackwardClock);
            yield return Pair("104_Module1_OutdoorsFastTickNeverOverridesControls_AndF10NeverLeavesZoneFlagSet", Module1_OutdoorsFastTickNeverOverridesControls_AndF10NeverLeavesZoneFlagSet);
            yield return Pair("105_Module1_OnlineTransition_ClearsInteriorStateWithZeroOnlineNatives", Module1_OnlineTransition_ClearsInteriorStateWithZeroOnlineNatives);
            yield return Pair("106_Persistence_TruncatedBak1RejectedAndCompleteBak2Recovered_All7Engines", Persistence_TruncatedBak1RejectedAndCompleteBak2Recovered_All7Engines);
            yield return Pair("107_AtomicFileStore_CorruptPrimaryNeverRotatedIntoBak1", AtomicFileStore_CorruptPrimaryNeverRotatedIntoBak1);
            yield return Pair("108_Kraken_RejectsDryLandCapsAtOneEnforcesBudgetAndCleansUpOnAbortAndOnline", Kraken_RejectsDryLandCapsAtOneEnforcesBudgetAndCleansUpOnAbortAndOnline);
            yield return Pair("109_UiLoopGuard_RecoveryExceptionNeverEscapes_AndTripsCircuitBreaker", UiLoopGuard_RecoveryExceptionNeverEscapes_AndTripsCircuitBreaker);
            yield return Pair("110_Host_PassiveIncomePersistFailure_NeverShowsNoIncomeDue", Host_PassiveIncomePersistFailure_NeverShowsNoIncomeDue);
            yield return Pair("111_Dispatch_RoadSpawnRejectsBelowMin_AndFireEvaluatorHonestOutcome", Dispatch_RoadSpawnRejectsBelowMin_AndFireEvaluatorHonestOutcome);
            yield return Pair("112_Blackjack_OnlineVoidReturnsVoidedOnlineAndRefundsOnStoryResume", Blackjack_OnlineVoidReturnsVoidedOnlineAndRefundsOnStoryResume);
            yield return Pair("113_Taxi_BlockedPickupRetriesOnceAndTimesOutWithCleanup", Taxi_BlockedPickupRetriesOnceAndTimesOutWithCleanup);
            yield return Pair("114_Stocks_AdvanceMarketTick_PersistsPricesAndSharesAcrossRestart", Stocks_AdvanceMarketTick_PersistsPricesAndSharesAcrossRestart);
            yield return Pair("115_OnlineDetectionGuard_FailClosedLogsAndNotifiesOnceWithoutSpam", OnlineDetectionGuard_FailClosedLogsAndNotifiesOnceWithoutSpam);
            yield return Pair("116_DeploymentManifest_AndDocs_MatchActualCsprojAndTestCount", DeploymentManifest_AndDocs_MatchActualCsprojAndTestCount);
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

        private static ClassicGTAHostController CreateHost(
            string dir,
            string slotId,
            out FakeGameContext ctx,
            out FakeNotificationService notify,
            out FakeSubmersibleAdapter subAdapter,
            out FakeCleanupAdapter cleanup,
            Func<DateTime> utcNow = null,
            IPersistenceStore customStore = null)
        {
            ctx = new FakeGameContext();
            notify = new FakeNotificationService();
            subAdapter = new FakeSubmersibleAdapter();
            cleanup = new FakeCleanupAdapter();
            IPersistenceStore store = customStore ?? new AtomicFileStore(dir, notify);

            return new ClassicGTAHostController(
                ctx,
                new FakeWeaponService(),
                new PerHandleAliveDispatchAdapter(),
                new FakeTaxiAdapter(),
                new FakeSideJobAdapter(),
                new FakeCompanionAdapter(),
                new FakePetAdapter(),
                subAdapter,
                cleanup,
                store,
                notify,
                utcNow ?? (() => new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc)),
                slotId,
                500000);
        }

        // C-01 & C-02: Two different slots keep independent wallet + asset files without overwriting each other.
        private static void Host_TwoSlots_KeepSeparateWalletAndAssetFilesWithoutOverwrite()
        {
            string dir = CreateTempDir();
            try
            {
                FakeGameContext cA, cB; FakeNotificationService nA, nB; FakeSubmersibleAdapter sA, sB; FakeCleanupAdapter clA, clB;
                var hostA = CreateHost(dir, "slot_A", out cA, out nA, out sA, out clA);
                Assert(hostA.PurchaseAsset("safehouse.del_perro").Succeeded, "Slot A purchase must succeed.");
                Assert(hostA.Wallet.GetCash() == 450000, "Slot A wallet should be $450,000.");

                var hostB = CreateHost(dir, "slot_B", out cB, out nB, out sB, out clB);
                Assert(!hostB.AssetsEngine.IsOwned("safehouse.del_perro"), "Slot B must not inherit Slot A ownership.");
                Assert(hostB.Wallet.GetCash() == 500000, "Slot B wallet must start fresh at $500,000.");
                Assert(hostB.PurchaseAsset("sub.kraken").Succeeded, "Slot B purchase must succeed.");

                // Reload Slot A -> must still have Del Perro + $450,000 (Slot B did NOT overwrite Slot A's files).
                var hostAReloaded = CreateHost(dir, "slot_A", out cA, out nA, out sA, out clA);
                Assert(hostAReloaded.AssetsEngine.IsOwned("safehouse.del_perro") && !hostAReloaded.AssetsEngine.IsOwned("sub.kraken"),
                    "Slot A ownership must survive Slot B saves untouched.");
                Assert(hostAReloaded.Wallet.GetCash() == 450000,
                    "Slot A wallet must survive Slot B saves untouched, got $" + hostAReloaded.Wallet.GetCash());
            }
            finally { DeleteDir(dir); }
        }

        // C-01 & C-02: Purchasing an asset reconciles exact wallet cash and ownership across host restart.
        private static void Host_PurchaseAndReload_ReconcilesExactWalletAndOwnership()
        {
            string dir = CreateTempDir();
            try
            {
                FakeGameContext ctx; FakeNotificationService notify; FakeSubmersibleAdapter sub; FakeCleanupAdapter cleanup;
                var host1 = CreateHost(dir, ClassicGTAHostController.DefaultSlotId, out ctx, out notify, out sub, out cleanup);
                Assert(host1.PurchaseAsset("business.malibu_club").Succeeded, "Club purchase must succeed.");
                Assert(host1.Wallet.GetCash() == 380000, "Expected $380,000 after $120,000 club.");

                var host2 = CreateHost(dir, ClassicGTAHostController.DefaultSlotId, out ctx, out notify, out sub, out cleanup);
                Assert(host2.AssetsEngine.IsOwned("business.malibu_club"), "Reloaded host must own club.");
                Assert(host2.Wallet.GetCash() == 380000, "Reloaded host wallet must remain $380,000.");
            }
            finally { DeleteDir(dir); }
        }

        // I-02 & C-02: Footer-less, slot-less v0.6.0 ClassicGTA_Assets.dat migrates under production slot and survives subsequent saves.
        private static void Host_LegacyV060Ledger_MigratesUnderProductionDefaultSlotAndSurvivesSaves()
        {
            string dir = CreateTempDir();
            try
            {
                // Write exact v0.6.0 legacy file (no SLOT=, no RECORD_COUNT=, no END_OF_FILE)
                File.WriteAllText(
                    Path.Combine(dir, "ClassicGTA_Assets.dat"),
                    "SCHEMA=1\nLAST_DAY=10\nOWNED=sub.kraken\n",
                    new UTF8Encoding(false));

                FakeGameContext ctx; FakeNotificationService notify; FakeSubmersibleAdapter sub; FakeCleanupAdapter cleanup;
                var host1 = CreateHost(dir, ClassicGTAHostController.DefaultSlotId, out ctx, out notify, out sub, out cleanup);
                Assert(host1.AssetsEngine.IsOwned("sub.kraken"),
                    "Production host (gtav_story_default) MUST migrate legacy v0.6.0 ownership.");

                // Buy another asset so a subsequent save runs
                Assert(host1.PurchaseAsset("safehouse.del_perro").Succeeded, "Post-migration purchase must succeed.");

                // Reload production host -> BOTH migrated sub.kraken and new safehouse must survive!
                var host2 = CreateHost(dir, ClassicGTAHostController.DefaultSlotId, out ctx, out notify, out sub, out cleanup);
                Assert(host2.AssetsEngine.IsOwned("sub.kraken") && host2.AssetsEngine.IsOwned("safehouse.del_perro"),
                    "Both migrated legacy ownership and new purchase must survive reload.");
            }
            finally { DeleteDir(dir); }
        }

        // I-02 & I-04: Corrupt existing per-slot ledger is never silently overwritten.
        private static void Assets_CorruptLedgerOnDisk_NeverSilentlyOverwritten()
        {
            string dir = CreateTempDir();
            try
            {
                string targetFile = Path.Combine(dir, "ClassicGTA_Assets_gtav_story_default.dat");
                const string corruptPayload = "SCHEMA=1\nSLOT=gtav_story_default\nOWNED=sub.kraken\nTRUNCATED_GARBAGE";
                File.WriteAllText(targetFile, corruptPayload, new UTF8Encoding(false));

                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);
                var engine = new AssetOwnershipEngine(new FakeGameContext(), new FakeEconomyService(500000), store, notify, 7, "gtav_story_default");
                engine.RegisterAsset(new AssetDefinition("safehouse.del_perro", "Del Perro", AssetCategory.Safehouse, 50000));

                Assert(!engine.LoadState() && engine.LoadFailedCorrupt, "Corrupt ledger must fail LoadState and set LoadFailedCorrupt.");
                PurchaseResult pr = engine.TryPurchase("safehouse.del_perro", 10);
                Assert(!pr.Succeeded, "Must refuse to overwrite a corrupt ledger on purchase.");
                Assert(File.ReadAllText(targetFile, Encoding.UTF8) == corruptPayload,
                    "Corrupt ledger file on disk must remain untouched.");
            }
            finally { DeleteDir(dir); }
        }

        // I-01 & C-02: Pure UTC clock pays purchase-to-first-collection elapsed time, caps at 7d, rejects backward clock.
        private static void Host_UtcIncomeClock_PaysPurchaseToFirstCollectionCapsAt7dAndRejectsBackwardClock()
        {
            string dir = CreateTempDir();
            try
            {
                DateTime now = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
                FakeGameContext ctx; FakeNotificationService notify; FakeSubmersibleAdapter sub; FakeCleanupAdapter cleanup;
                var host = CreateHost(dir, "slot_utc", out ctx, out notify, out sub, out cleanup, () => now);

                // Purchase nightclub on June 1 -> baseline stamped at purchase!
                Assert(host.PurchaseAsset("business.malibu_club").Succeeded, "Club purchase must succeed.");

                // Same day -> $0 NothingDue
                PassiveIncomeResult sameDay = host.CollectPassiveIncome();
                Assert(sameDay.Status == PassiveIncomeStatus.NothingDue && sameDay.AmountPaid == 0,
                    "Same-day collection should return NothingDue.");

                // 3 real UTC days later -> pays 3 * $10,000 = $30,000 (purchase-to-first-collection NOT forfeited!)
                now = now.AddDays(3);
                PassiveIncomeResult threeDays = host.CollectPassiveIncome();
                Assert(threeDays.Status == PassiveIncomeStatus.Paid && threeDays.AmountPaid == 30000 && threeDays.ElapsedDays == 3,
                    "Expected $30,000 for 3 UTC days since purchase, got $" + threeDays.AmountPaid);

                // Clock jumps backward 5 days -> $0, ClockMovedBackward, does NOT corrupt baseline
                now = now.AddDays(-5);
                PassiveIncomeResult backward = host.CollectPassiveIncome();
                Assert(backward.Status == PassiveIncomeStatus.ClockMovedBackward && backward.AmountPaid == 0,
                    "Backward clock must return ClockMovedBackward with $0.");

                // 20 days after June 4 (June 24) -> caps at 7 days ($70,000)
                now = new DateTime(2026, 6, 24, 10, 0, 0, DateTimeKind.Utc);
                PassiveIncomeResult capped = host.CollectPassiveIncome();
                Assert(capped.Status == PassiveIncomeStatus.Paid && capped.AmountPaid == 70000 && capped.ElapsedDays == 7,
                    "20-day absence must cap at 7 days ($70,000), got $" + capped.AmountPaid);
            }
            finally { DeleteDir(dir); }
        }

        // I-03: Outdoors produces zero FastTick overrides; F10 outdoors never leaves SetCanEquipAllWeapons(true) set.
        private static void Module1_OutdoorsFastTickNeverOverridesControls_AndF10NeverLeavesZoneFlagSet()
        {
            var ctx = new FakeGameContext();
            var weapons = new FakeWeaponService();
            var notify = new FakeNotificationService();
            var mod = new WeaponAndZoneController(ctx, weapons, notify, new WeaponAndZoneConfig());
            mod.Start();

            weapons.InteriorId = 0; // Outdoors
            mod.SlowTick(1000);
            mod.FastTick();
            mod.FastTick();

            Assert(weapons.EnableCombatControlsCalls == 0 && weapons.DisableFiringFalseCalls == 0,
                "Outdoors FastTick MUST NOT touch combat controls.");

            int granted = mod.GrantPocketArsenal();
            Assert(granted == 3, "F10 outdoors should still grant all 3 unique weapons.");
            Assert(weapons.CanEquipAllCalls == 0,
                "F10 outdoors MUST NOT set SetCanEquipAllWeapons(true).");
        }

        // I-03: Online transition clears interior tracking without calling Online gameplay natives.
        private static void Module1_OnlineTransition_ClearsInteriorStateWithZeroOnlineNatives()
        {
            var ctx = new FakeGameContext();
            var weapons = new FakeWeaponService();
            var notify = new FakeNotificationService();
            var mod = new WeaponAndZoneController(ctx, weapons, notify, new WeaponAndZoneConfig());
            mod.Start();

            weapons.InteriorId = 42;
            mod.SlowTick(1000);
            int beforeOnlineEquipCalls = weapons.CanEquipAllCalls;
            int beforeOnlineControlCalls = weapons.EnableCombatControlsCalls;

            ctx.IsOnlineSession = true;
            mod.SlowTick(1500);
            mod.FastTick();

            Assert(weapons.CanEquipAllCalls == beforeOnlineEquipCalls && weapons.EnableCombatControlsCalls == beforeOnlineControlCalls,
                "Online transition must perform zero gameplay native calls.");

            // Return to SP outdoors -> interior flag was cleared, so FastTick must not override outdoors
            ctx.IsOnlineSession = false;
            weapons.InteriorId = 0;
            mod.FastTick();
            Assert(weapons.EnableCombatControlsCalls == beforeOnlineControlCalls,
                "Returning to SP outdoors after Online must not retain stale interior state.");
        }

        // I-04: Truncated .bak1 with valid header is rejected so complete .bak2 is recovered, for ALL 7 persisted engines.
        private static void Persistence_TruncatedBak1RejectedAndCompleteBak2Recovered_All7Engines()
        {
            string dir = CreateTempDir();
            try
            {
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);
                var ctx = new FakeGameContext();

                // Helper that writes a corrupt primary and a valid-header truncated .bak1 over a good .bak2
                Action<string, string> poisonPrimaryAndBak1 = (fileName, headerLine) =>
                {
                    string basePath = Path.Combine(dir, fileName);
                    string goodContent = File.ReadAllText(basePath, Encoding.UTF8);
                    File.WriteAllText(basePath + ".bak2", goodContent, new UTF8Encoding(false));
                    File.WriteAllText(basePath + ".bak1", headerLine + "\nPARTIAL_TRUNCATED_LINE\n", new UTF8Encoding(false));
                    File.WriteAllText(basePath, headerLine + "\n", new UTF8Encoding(false));
                };

                // 1. AssetOwnershipEngine
                var assets = new AssetOwnershipEngine(ctx, new FakeEconomyService(500000), store, notify, 7, "slot_t");
                assets.RegisterAsset(new AssetDefinition("sub.kraken", "Kraken", AssetCategory.Submarine, 250000));
                Assert(assets.TryPurchase("sub.kraken", 5).Succeeded, "Asset purchase failed.");
                poisonPrimaryAndBak1(assets.SaveFileName, "SCHEMA=1");
                var assets2 = new AssetOwnershipEngine(ctx, new FakeEconomyService(0), store, notify, 7, "slot_t");
                assets2.RegisterAsset(new AssetDefinition("sub.kraken", "Kraken", AssetCategory.Submarine, 250000));
                Assert(assets2.LoadState() && assets2.IsOwned("sub.kraken"), "Assets must reject truncated .bak1 and recover .bak2.");

                // 2. ModWalletEconomyService
                var wallet = new ModWalletEconomyService(store, notify, "slot_t", 1000);
                Assert(wallet.TrySpend(250, "Test"), "Wallet spend failed.");
                poisonPrimaryAndBak1(wallet.SaveFileName, "WALLET_SCHEMA=1");
                var wallet2 = new ModWalletEconomyService(store, notify, "slot_t", 0);
                Assert(wallet2.LoadState() && wallet2.GetCash() == 750, "Wallet must reject truncated .bak1 and recover .bak2.");

                // 3. PetCompanionController
                var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
                var pets = new PetCompanionController(ctx, new FakePetAdapter(), new FakeEconomyService(5000), reg, store, notify);
                pets.RegisterBreed("dog.rottweiler", 1500);
                Assert(pets.TryBuyBreed("dog.rottweiler").Succeeded, "Pet buy failed.");
                poisonPrimaryAndBak1("ClassicGTA_Pets.dat", "PETS_SCHEMA=1");
                var pets2 = new PetCompanionController(ctx, new FakePetAdapter(), new FakeEconomyService(0), reg, store, notify);
                pets2.RegisterBreed("dog.rottweiler", 1500);
                Assert(pets2.LoadState() && pets2.IsBreedOwned("dog.rottweiler"), "Pets must reject truncated .bak1 and recover .bak2.");

                // 4. StockMarketEngine
                var stocks = new StockMarketEngine(ctx, new FakeEconomyService(5000), store, notify);
                stocks.RegisterStock(new StockQuote("LCN1", "Cluckin", 100, 10, 500, 10));
                Assert(stocks.TryBuyShares("LCN1", 5).Succeeded, "Stock buy failed.");
                poisonPrimaryAndBak1("ClassicGTA_Stocks.dat", "STOCKS_SCHEMA=1");
                var stocks2 = new StockMarketEngine(ctx, new FakeEconomyService(0), store, notify);
                stocks2.RegisterStock(new StockQuote("LCN1", "Cluckin", 100, 10, 500, 10));
                Assert(stocks2.LoadState() && stocks2.GetQuote("LCN1").SharesOwned == 5, "Stocks must reject truncated .bak1 and recover .bak2.");

                // 5. RpgSkillEngine
                var rpg = new RpgSkillEngine(ctx, store);
                rpg.AddXp(RpgSkillType.Shooting, 500);
                rpg.SaveState();
                poisonPrimaryAndBak1("ClassicGTA_Skills.dat", "SKILLS_SCHEMA=1");
                var rpg2 = new RpgSkillEngine(ctx, store);
                Assert(rpg2.LoadState() && rpg2.GetLevel(RpgSkillType.Shooting) == 5, "RpgSkills must reject truncated .bak1 and recover .bak2.");

                // 6. RampageController
                var rampage = new RampageController(ctx, new FakeEconomyService(0), store, notify);
                rampage.StartRampage("rampage.1", 5, 60000, 1000, 1000);
                for (int k = 0; k < 5; k++) rampage.RecordKill(2000 + k * 100);
                rampage.FinishEarly(10000);
                poisonPrimaryAndBak1("ClassicGTA_Medals.dat", "MEDALS_SCHEMA=1");
                var rampage2 = new RampageController(ctx, new FakeEconomyService(0), store, notify);
                Assert(rampage2.LoadState() && rampage2.GetBestMedal("rampage.1") != MedalRating.None,
                    "Rampages must reject truncated .bak1 and recover .bak2.");

                // 7. SafehouseDecoratorEngine
                var deco = new SafehouseDecoratorEngine(ctx, new FakeEconomyService(10000), store, id => true);
                deco.RegisterSafehouseBounds("safehouse.del_perro", new WorldPoint(0f, 0f, 0f), 20f);
                deco.RegisterProp("prop.sofa", 500);
                string placedId;
                Assert(deco.TryPlaceFurniture("safehouse.del_perro", "prop.sofa", new WorldPoint(1f, 2f, 3f), 90f, out placedId).Succeeded, "Place furniture failed.");
                poisonPrimaryAndBak1("ClassicGTA_Furniture.dat", "FURNITURE_SCHEMA=1");
                var deco2 = new SafehouseDecoratorEngine(ctx, new FakeEconomyService(0), store, id => true);
                deco2.RegisterSafehouseBounds("safehouse.del_perro", new WorldPoint(0f, 0f, 0f), 20f);
                deco2.RegisterProp("prop.sofa", 500);
                Assert(deco2.LoadState() && deco2.GetPlacedForSafehouse("safehouse.del_perro").Count == 1,
                    "SafehouseProps must reject truncated .bak1 and recover .bak2.");
            }
            finally { DeleteDir(dir); }
        }

        // I-04: AtomicFileStore.Save(..., validator) must NOT rotate a corrupt primary into .bak1.
        private static void AtomicFileStore_CorruptPrimaryNeverRotatedIntoBak1()
        {
            string dir = CreateTempDir();
            try
            {
                var store = new AtomicFileStore(dir, new FakeNotificationService());
                Func<string, bool> validator = s => PersistenceCodec.ValidateStrict(s, "TEST=1", l => l.StartsWith("K="));
                string good1 = PersistenceCodec.Serialize("TEST=1", new List<string> { "K=1" });
                string good2 = PersistenceCodec.Serialize("TEST=1", new List<string> { "K=2" });

                store.Save("sample.dat", good1, validator);
                store.Save("sample.dat", good2, validator); // .bak1 now holds good1

                // Corrupt primary on disk
                string primaryPath = Path.Combine(dir, "sample.dat");
                File.WriteAllText(primaryPath, "TEST=1\nCORRUPT", new UTF8Encoding(false));

                // Save good3 with validator -> must NOT rotate CORRUPT into .bak1!
                string good3 = PersistenceCodec.Serialize("TEST=1", new List<string> { "K=3" });
                store.Save("sample.dat", good3, validator);

                string bak1 = File.ReadAllText(primaryPath + ".bak1", Encoding.UTF8);
                Assert(bak1 == good1, "Corrupt primary must never overwrite valid .bak1.");
            }
            finally { DeleteDir(dir); }
        }

        // I-05 & C-02: Kraken rejects dry land, caps at 1, enforces entity budget, and cleans up on Abort & Online.
        private static void Kraken_RejectsDryLandCapsAtOneEnforcesBudgetAndCleansUpOnAbortAndOnline()
        {
            string dir = CreateTempDir();
            try
            {
                FakeGameContext ctx; FakeNotificationService notify; FakeSubmersibleAdapter sub; FakeCleanupAdapter cleanup;
                var host = CreateHost(dir, "slot_kraken", out ctx, out notify, out sub, out cleanup);
                Assert(host.PurchaseAsset("sub.kraken").Succeeded, "Buy Kraken failed.");

                // Dry land rejected
                sub.InWater = false;
                Assert(!host.KrakenController.SummonOwnedSubmersible(), "Dry land summon must be rejected.");
                Assert(sub.SpawnCalls == 0, "Dry land must not spawn a vehicle.");

                // Open water -> spawns & tracks 1
                sub.InWater = true;
                Assert(host.KrakenController.SummonOwnedSubmersible(), "Water summon 1 must succeed.");
                int firstHandle = host.KrakenController.ActiveSubHandle;
                Assert(host.Registry.TrackedEntityCount == 1, "Expected 1 tracked Kraken.");

                // Second summon replaces first cleanly (cap = 1)
                Assert(host.KrakenController.SummonOwnedSubmersible(), "Water summon 2 must succeed.");
                Assert(host.Registry.TrackedEntityCount == 1 && cleanup.DeletedEntities.Contains(firstHandle),
                    "Second summon must replace previous Kraken and stay at cap 1.");

                // Fill entity budget to 20 -> refusal cleans up untracked spawn
                for (int i = 0; i < 19; i++) host.Registry.TryTrackEntity(5000 + i, "filler", EntityOwnership.Owned);
                // Note: replacing removes 1 then adds 1, so fill to 20 AFTER removing by using a fresh controller with cap=1
                var tightReg = new EntityOwnershipRegistry(cleanup, 1);
                tightReg.TryTrackEntity(8888, "other", EntityOwnership.Owned);
                var tightKraken = new KrakenDeliveryController(ctx, host.AssetsEngine, sub, tightReg, notify);
                int deletedBefore = cleanup.DeletedEntities.Count;
                Assert(!tightKraken.SummonOwnedSubmersible(), "Full budget must refuse Kraken summon.");
                Assert(cleanup.DeletedEntities.Count == deletedBefore + 1, "Refused Kraken must delete untracked spawn.");

                // Online abort -> drops without deleting
                int deletedBeforeOnline = cleanup.DeletedEntities.Count;
                ctx.IsOnlineSession = true;
                host.HandleAbort();
                Assert(host.KrakenController.ActiveSubHandle == 0 && cleanup.DeletedEntities.Count == deletedBeforeOnline,
                    "Online abort must drop Kraken without DeleteEntity.");

                // Story Mode abort -> deletes owned Kraken
                var host2 = CreateHost(dir, "slot_kraken", out ctx, out notify, out sub, out cleanup);
                Assert(host2.KrakenController.SummonOwnedSubmersible(), "Summon on host2 failed.");
                int handle2 = host2.KrakenController.ActiveSubHandle;
                host2.HandleAbort();
                Assert(cleanup.DeletedEntities.Contains(handle2), "Story Mode abort must delete owned Kraken.");
            }
            finally { DeleteDir(dir); }
        }

        // I-06 & C-02: UiLoopGuard catches throwing Process() AND throwing recovery actions without escaping, and trips circuit breaker.
        private static void UiLoopGuard_RecoveryExceptionNeverEscapes_AndTripsCircuitBreaker()
        {
            var guard = new UiLoopGuard(3);
            int processAttempts = 0;

            for (int i = 0; i < 5; i++)
            {
                guard.TryProcess(
                    () => { processAttempts++; throw new InvalidOperationException("UI crash"); },
                    () => { throw new InvalidOperationException("HideAll crash"); },
                    msg => { throw new IOException("Log crash"); },
                    msg => { throw new InvalidOperationException("Show crash"); });
            }

            Assert(guard.IsUiDisabled, "Circuit breaker must disable UI after 3 consecutive failures.");
            Assert(processAttempts == 3, "Must stop attempting Process() after tripping circuit breaker, got " + processAttempts);

            guard.Reset();
            Assert(!guard.IsUiDisabled && guard.ConsecutiveErrors == 0, "Reset (F7) must re-enable UI.");
        }

        // I-06 & C-02: Failed passive-income persistence displays persistence error, never "No new business income due yet."
        private static void Host_PassiveIncomePersistFailure_NeverShowsNoIncomeDue()
        {
            string dir = CreateTempDir();
            try
            {
                DateTime now = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
                var notify = new FakeNotificationService();
                var toggleStore = new ToggleFailingPersistenceStore(new AtomicFileStore(dir, notify));

                FakeGameContext ctx; FakeNotificationService hostNotify; FakeSubmersibleAdapter sub; FakeCleanupAdapter cleanup;
                var host = CreateHost(dir, "slot_err", out ctx, out hostNotify, out sub, out cleanup, () => now, toggleStore);
                Assert(host.PurchaseAsset("business.malibu_club").Succeeded, "Buy club failed.");

                now = now.AddDays(2);
                toggleStore.FailSaves = true;
                hostNotify.Notifications.Clear();

                PassiveIncomeResult res = host.CollectPassiveIncome();
                Assert(res.Status == PassiveIncomeStatus.PersistenceFailed, "Expected PersistenceFailed status.");
                Assert(!hostNotify.Notifications.Exists(m => m.Contains("No new business income due yet")),
                    "Persistence failure MUST NEVER display 'No new business income due yet.'");
                Assert(hostNotify.Notifications.Exists(m => m.Contains("save failed")),
                    "Persistence failure must show an explicit save-failure notification.");
            }
            finally { DeleteDir(dir); }
        }

        // I-07 & C-02: RoadSpawnValidator rejects < minDistance (0.5x bug) and FireExtinguishEvaluator returns false when no fires reduced.
        private static void Dispatch_RoadSpawnRejectsBelowMin_AndFireEvaluatorHonestOutcome()
        {
            var origin = new WorldPoint(0f, 0f, 0f);
            var halfMin = new WorldPoint(30f, 0f, 0f); // 30m < 60m min
            var valid = new WorldPoint(75f, 0f, 0f);   // 75m in [60, 140]
            var tooFar = new WorldPoint(150f, 0f, 0f); // 150m > 140m max

            Assert(!RoadSpawnValidator.IsWithinSpawnRange(halfMin, origin, 60f, 140f),
                "Road spawn at 0.5x MinSpawnDistance (30m < 60m) MUST be rejected.");
            Assert(RoadSpawnValidator.IsWithinSpawnRange(valid, origin, 60f, 140f),
                "Road spawn at 75m within [60, 140] must be accepted.");
            Assert(!RoadSpawnValidator.IsWithinSpawnRange(tooFar, origin, 60f, 140f),
                "Road spawn above MaxSpawnDistance (150m > 140m) must be rejected.");

            Assert(!FireExtinguishEvaluator.DidExtinguish(0, 0), "0 fires before must return false.");
            Assert(!FireExtinguishEvaluator.DidExtinguish(3, 3), "Unchanged fire count must return false.");
            Assert(FireExtinguishEvaluator.DidExtinguish(3, 0), "Reduced fire count must return true.");
        }

        // I-08: Online void returns VoidedOnline (not Push), never touches Online cash, and refunds on Story Mode resume.
        private static void Blackjack_OnlineVoidReturnsVoidedOnlineAndRefundsOnStoryResume()
        {
            var ctx = new FakeGameContext();
            var econ = new FakeEconomyService(1000);
            var bj = new BlackjackEngine(ctx, econ, () => 5);

            Assert(bj.StartHand(200), "StartHand should succeed.");
            Assert(econ.Cash == 800, "Bet should be deducted.");

            ctx.IsOnlineSession = true;
            BlackjackOutcome res = bj.Hit();
            Assert(res == BlackjackOutcome.VoidedOnline && res != BlackjackOutcome.Push,
                "Online void must return VoidedOnline, never Push.");
            Assert(econ.Cash == 800 && bj.PendingOnlineRefund == 200,
                "Online cash must remain untouched while refund is pending.");
            Assert(bj.ClaimPendingRefund() == 0, "Cannot claim refund while still Online.");

            // Resume Story Mode -> refund settles cleanly!
            ctx.IsOnlineSession = false;
            int refunded = bj.ClaimPendingRefund();
            Assert(refunded == 200 && econ.Cash == 1000 && bj.PendingOnlineRefund == 0,
                "Pending refund must return $200 once Story Mode resumes.");
        }

        // A-01: Blocked taxi pickup retries once at 10s and aborts cleanly at 30s.
        private static void Taxi_BlockedPickupRetriesOnceAndTimesOutWithCleanup()
        {
            var adapter = new FakeTaxiAdapter();
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var taxi = new TaxiMissionController(new FakeGameContext(), adapter, new FakeEconomyService(0), reg, new FakeNotificationService());
            taxi.Start();
            taxi.StartShift();
            taxi.SlowTick(1000); // Searching -> Pickup

            adapter.PlayerPosition = adapter.PickupPos;
            taxi.SlowTick(2000); // Issue enter task #1
            Assert(adapter.EnterTasks == 1, "Expected 1 initial enter task.");

            taxi.SlowTick(12000); // +10s -> bounded retry #2
            Assert(adapter.EnterTasks == 2 && taxi.CurrentState == TaxiState.Pickup,
                "Expected 1 retry at 10s, got EnterTasks=" + adapter.EnterTasks);

            taxi.SlowTick(32000); // +30s -> timeout abort + cleanup (borrowed taxi NOT deleted)
            Assert(taxi.CurrentState == TaxiState.Idle, "Blocked pickup must abort at 30s timeout.");
            Assert(cleanup.DeletedEntities.Contains(901) && !cleanup.DeletedEntities.Contains(adapter.TaxiHandle),
                "Timeout must delete owned fare ped (901) without deleting borrowed taxi.");
        }

        // A-02: StockMarketEngine.AdvanceMarketTick persists prices and shares coherently across restart.
        private static void Stocks_AdvanceMarketTick_PersistsPricesAndSharesAcrossRestart()
        {
            string dir = CreateTempDir();
            try
            {
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);
                var ctx = new FakeGameContext();

                var s1 = new StockMarketEngine(ctx, new FakeEconomyService(10000), store, notify, (min, max) => 10);
                s1.RegisterStock(new StockQuote("LCN1", "Cluckin", 100, 10, 500, 10));
                Assert(s1.TryBuyShares("LCN1", 5).Succeeded, "Buy shares failed.");
                s1.AdvanceMarketTick(); // 100 -> 110 and persisted!
                Assert(s1.GetQuote("LCN1").CurrentPrice == 110, "Price should advance to 110.");

                var s2 = new StockMarketEngine(ctx, new FakeEconomyService(0), store, notify);
                s2.RegisterStock(new StockQuote("LCN1", "Cluckin", 100, 10, 500, 10));
                Assert(s2.LoadState(), "Reload stocks failed.");
                Assert(s2.GetQuote("LCN1").CurrentPrice == 110 && s2.GetQuote("LCN1").SharesOwned == 5,
                    "Both advanced price ($110) and shares (5) must persist across restart.");
            }
            finally { DeleteDir(dir); }
        }

        // A-03 & C-02: OnlineDetectionGuard fails closed, logs once, notifies once, and resets latch on recovery.
        private static void OnlineDetectionGuard_FailClosedLogsAndNotifiesOnceWithoutSpam()
        {
            var notify = new FakeNotificationService();
            var guard = new OnlineDetectionGuard(notify);

            bool first = guard.Evaluate(() => { throw new InvalidOperationException("Native failure"); });
            bool second = guard.Evaluate(() => { throw new InvalidOperationException("Native failure"); });

            Assert(first && second, "OnlineDetectionGuard must fail closed (true) on exception.");
            Assert(notify.Logs.Count == 1 && notify.Notifications.Count == 1,
                "Must log and notify exactly once while failing, not spam every frame.");

            bool recovered = guard.Evaluate(() => false);
            Assert(!recovered && !guard.HasLoggedFailure, "Recovery must return false and reset failure latch.");
        }

        // C-02 & I-09: DeploymentManifest matches .csproj deploy target and docs have no stale test counts.
        private static void DeploymentManifest_AndDocs_MatchActualCsprojAndTestCount()
        {
            string repoRoot = FindRepoRoot();
            string csproj = File.ReadAllText(Path.Combine(repoRoot, "src", "ClassicGTA.Adapter.GTAV", "ClassicGTA.Adapter.GTAV.csproj"), Encoding.UTF8);

            for (int i = 0; i < DeploymentManifest.RequiredScriptFiles.Length; i++)
            {
                string required = DeploymentManifest.RequiredScriptFiles[i];
                Assert(csproj.IndexOf(required, StringComparison.OrdinalIgnoreCase) >= 0,
                    "Deployment target in .csproj must include " + required);
            }
            Assert(csproj.IndexOf("ScriptHookVDotNet3.dll\" DestinationFolder", StringComparison.OrdinalIgnoreCase) < 0,
                "Deployment target must never copy ScriptHookVDotNet3.dll to scripts/.");

            string readme = File.ReadAllText(Path.Combine(repoRoot, "README.md"), Encoding.UTF8);
            string tracker = File.ReadAllText(Path.Combine(repoRoot, "PROGRESS_TRACKER.md"), Encoding.UTF8);

            Assert(readme.Contains("116/116") && !readme.Contains("94 tests") && !readme.Contains("98/98"),
                "README.md must report current 116/116 test count with no stale 94/98 counts.");
            Assert(tracker.Contains("116/116") && !tracker.Contains("still **82/82 PASS**"),
                "PROGRESS_TRACKER.md must report current 116/116 test count with no stale 82/82 resume text.");
        }

        private static string FindRepoRoot()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            for (int i = 0; i < 8 && !string.IsNullOrEmpty(dir); i++)
            {
                if (File.Exists(Path.Combine(dir, "ClassicGTAOverhaul.sln")))
                {
                    return dir;
                }
                dir = Path.GetDirectoryName(dir);
            }
            throw new InvalidOperationException("Could not locate ClassicGTAOverhaul.sln from " + AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
