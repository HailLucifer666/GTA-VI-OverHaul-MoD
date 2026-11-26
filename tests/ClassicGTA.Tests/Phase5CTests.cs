using System;
using System.Collections.Generic;
using System.IO;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Activities;
using ClassicGTA.Core.Economy;
using ClassicGTA.Infrastructure;

namespace ClassicGTA.Tests
{
    internal sealed class FakePetAdapter : IPetAdapter
    {
        public bool PetAlive { get; set; }
        public List<PetCommand> IssuedCommands { get; private set; }
        private int _nextHandle = 2100;

        public FakePetAdapter()
        {
            PetAlive = true;
            IssuedCommands = new List<PetCommand>();
        }

        public WorldPoint GetPlayerPosition() { return new WorldPoint(0f, 0f, 0f); }

        public bool TrySpawnPet(string breedId, WorldPoint nearPlayer, out int petHandle, out int blipHandle)
        {
            petHandle = _nextHandle++;
            blipHandle = _nextHandle++;
            return true;
        }

        public bool IsEntityAlive(int handle) { return PetAlive; }
        public void IssueCommand(int petHandle, PetCommand command) { IssuedCommands.Add(command); }
    }

    internal static class Phase5CTests
    {
        public static IEnumerable<KeyValuePair<string, Action>> GetAll()
        {
            yield return Pair("71_Pet_CannotSummonUnownedBreed_BuyThenSummonAndCommand", Pet_CannotSummonUnownedBreed_BuyThenSummonAndCommand);
            yield return Pair("72_Pet_DeathCleansUpEntityAndBlip", Pet_DeathCleansUpEntityAndBlip);
            yield return Pair("73_Pet_BuyPersistFailureRefundsAndRollsBack", Pet_BuyPersistFailureRefundsAndRollsBack);
            yield return Pair("74_Pet_PersistsOwnedBreedsAcrossSessions", Pet_PersistsOwnedBreedsAcrossSessions);
            yield return Pair("75_Decorator_PlacesPersistsReloadsAndRemovesFurniture", Decorator_PlacesPersistsReloadsAndRemovesFurniture);
            yield return Pair("76_Decorator_RejectsOutOfBoundsAndUnownedSafehouse", Decorator_RejectsOutOfBoundsAndUnownedSafehouse);
            yield return Pair("77_Decorator_PersistFailureRefundsAndRollsBack", Decorator_PersistFailureRefundsAndRollsBack);
            yield return Pair("78_Decorator_EnforcesMaxPropsPerSafehouseCap", Decorator_EnforcesMaxPropsPerSafehouseCap);
            yield return Pair("79_Decorator_BlockedInOnlineSession", Decorator_BlockedInOnlineSession);
            yield return Pair("80_ClassicNet_OpenNavigateBackAndHomeHistory", ClassicNet_OpenNavigateBackAndHomeHistory);
            yield return Pair("81_ClassicNet_BlockedInOnlineAndCutscene", ClassicNet_BlockedInOnlineAndCutscene);
            yield return Pair("82_ClassicNet_ClosedBrowserIgnoresNavigation", ClassicNet_ClosedBrowserIgnoresNavigation);
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

        private static void Pet_CannotSummonUnownedBreed_BuyThenSummonAndCommand()
        {
            string dir = CreateTempDir();
            try
            {
                var adapter = new FakePetAdapter();
                var econ = new FakeEconomyService(5000);
                var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
                var notify = new FakeNotificationService();
                var pets = new PetCompanionController(new FakeGameContext(), adapter, econ, reg, new AtomicFileStore(dir, notify), notify);
                pets.RegisterBreed("dog.rottweiler", 1500);
                pets.Start();

                Assert(!pets.SummonPet("dog.rottweiler"), "Must NOT summon an unowned pet breed.");
                Assert(pets.TryBuyBreed("dog.rottweiler").Succeeded && econ.Cash == 3500, "Buy should deduct $1500.");
                Assert(pets.SummonPet("dog.rottweiler") && pets.HasActivePet, "Summon owned breed should succeed.");
                Assert(pets.GiveCommand(PetCommand.Guard) && pets.CurrentCommand == PetCommand.Guard, "Guard command should apply.");
                Assert(adapter.IssuedCommands.Contains(PetCommand.Follow) && adapter.IssuedCommands.Contains(PetCommand.Guard),
                    "Expected Follow on spawn and Guard on command.");
            }
            finally { DeleteDir(dir); }
        }

        private static void Pet_DeathCleansUpEntityAndBlip()
        {
            string dir = CreateTempDir();
            try
            {
                var adapter = new FakePetAdapter();
                var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
                var notify = new FakeNotificationService();
                var pets = new PetCompanionController(new FakeGameContext(), adapter, new FakeEconomyService(5000), reg, new AtomicFileStore(dir, notify), notify);
                pets.RegisterBreed("dog.husky", 1000);
                pets.Start();
                pets.TryBuyBreed("dog.husky");
                pets.SummonPet("dog.husky");

                adapter.PetAlive = false;
                pets.SlowTick(2000);
                Assert(!pets.HasActivePet && reg.TrackedEntityCount == 0 && reg.TrackedBlipCount == 0,
                    "Pet death must clean up entity and blip.");
            }
            finally { DeleteDir(dir); }
        }

        private static void Pet_BuyPersistFailureRefundsAndRollsBack()
        {
            var econ = new FakeEconomyService(3000);
            var pets = new PetCompanionController(new FakeGameContext(), new FakePetAdapter(), econ, new EntityOwnershipRegistry(new FakeCleanupAdapter()), new FailingPersistenceStore(), new FakeNotificationService());
            pets.RegisterBreed("dog.shepherd", 2000);
            pets.Start();

            PurchaseResult res = pets.TryBuyBreed("dog.shepherd");
            Assert(res.Status == PurchaseStatus.PersistenceFailedAndRefunded, "Expected PersistenceFailedAndRefunded.");
            Assert(econ.Cash == 3000 && !pets.IsBreedOwned("dog.shepherd"), "Cash and ownership must roll back.");
        }

        private static void Pet_PersistsOwnedBreedsAcrossSessions()
        {
            string dir = CreateTempDir();
            try
            {
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);
                var p1 = new PetCompanionController(new FakeGameContext(), new FakePetAdapter(), new FakeEconomyService(5000), new EntityOwnershipRegistry(new FakeCleanupAdapter()), store, notify);
                p1.RegisterBreed("dog.rottweiler", 1500);
                p1.TryBuyBreed("dog.rottweiler");

                var p2 = new PetCompanionController(new FakeGameContext(), new FakePetAdapter(), new FakeEconomyService(0), new EntityOwnershipRegistry(new FakeCleanupAdapter()), store, notify);
                p2.RegisterBreed("dog.rottweiler", 1500);
                Assert(p2.LoadState() && p2.IsBreedOwned("dog.rottweiler"), "Owned breed must reload across sessions.");
            }
            finally { DeleteDir(dir); }
        }

        private static void Decorator_PlacesPersistsReloadsAndRemovesFurniture()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(10000);
                var store = new AtomicFileStore(dir, new FakeNotificationService());
                var dec = new SafehouseDecoratorEngine(new FakeGameContext(), econ, store, id => id == "home.condo", 10);
                dec.RegisterProp("prop.sofa_modern", 1200);
                dec.RegisterSafehouseBounds("home.condo", new WorldPoint(10f, 10f, 5f), 20f);

                string placementId;
                PurchaseResult res = dec.TryPlaceFurniture("home.condo", "prop.sofa_modern", new WorldPoint(12.5f, 11.25f, 5f), 90f, out placementId);
                Assert(res.Succeeded && !string.IsNullOrEmpty(placementId) && econ.Cash == 8800, "Placement should succeed and cost $1200.");

                var reloaded = new SafehouseDecoratorEngine(new FakeGameContext(), econ, store, id => id == "home.condo", 10);
                Assert(reloaded.LoadState() && reloaded.GetPlacedForSafehouse("home.condo").Count == 1, "Placed prop must persist.");
                Assert(Math.Abs(reloaded.GetPlacedForSafehouse("home.condo")[0].Position.X - 12.5f) < 0.001f, "Coordinates must round-trip.");

                Assert(reloaded.RemoveFurniture(placementId) && reloaded.TotalPlacedCount == 0, "RemoveFurniture should remove prop.");
            }
            finally { DeleteDir(dir); }
        }

        private static void Decorator_RejectsOutOfBoundsAndUnownedSafehouse()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(10000);
                var store = new AtomicFileStore(dir, new FakeNotificationService());
                var dec = new SafehouseDecoratorEngine(new FakeGameContext(), econ, store, id => id == "home.owned", 10);
                dec.RegisterProp("prop.tv", 500);
                dec.RegisterSafehouseBounds("home.owned", new WorldPoint(0f, 0f, 0f), 15f);
                dec.RegisterSafehouseBounds("home.unowned", new WorldPoint(100f, 0f, 0f), 15f);

                string id1;
                Assert(!dec.TryPlaceFurniture("home.unowned", "prop.tv", new WorldPoint(100f, 0f, 0f), 0f, out id1).Succeeded,
                    "Must reject placement in unowned safehouse.");

                string id2;
                Assert(!dec.TryPlaceFurniture("home.owned", "prop.tv", new WorldPoint(50f, 0f, 0f), 0f, out id2).Succeeded,
                    "Must reject placement outside safehouse radius.");
                Assert(econ.Cash == 10000 && dec.TotalPlacedCount == 0, "No cash spent or prop placed on rejection.");
            }
            finally { DeleteDir(dir); }
        }

        private static void Decorator_PersistFailureRefundsAndRollsBack()
        {
            var econ = new FakeEconomyService(5000);
            var dec = new SafehouseDecoratorEngine(new FakeGameContext(), econ, new FailingPersistenceStore(), s => true, 10);
            dec.RegisterProp("prop.pool_table", 2500);
            dec.RegisterSafehouseBounds("home.owned", new WorldPoint(0f, 0f, 0f), 20f);

            string id;
            PurchaseResult res = dec.TryPlaceFurniture("home.owned", "prop.pool_table", new WorldPoint(2f, 2f, 0f), 0f, out id);
            Assert(res.Status == PurchaseStatus.PersistenceFailedAndRefunded, "Expected PersistenceFailedAndRefunded.");
            Assert(econ.Cash == 5000 && dec.TotalPlacedCount == 0, "Cash and placement MUST roll back on persist failure.");
        }

        private static void Decorator_EnforcesMaxPropsPerSafehouseCap()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(10000);
                var dec = new SafehouseDecoratorEngine(new FakeGameContext(), econ, new AtomicFileStore(dir, new FakeNotificationService()), s => true, 2);
                dec.RegisterProp("prop.lamp", 100);
                dec.RegisterSafehouseBounds("home.owned", new WorldPoint(0f, 0f, 0f), 20f);

                string id;
                Assert(dec.TryPlaceFurniture("home.owned", "prop.lamp", new WorldPoint(1f, 0f, 0f), 0f, out id).Succeeded, "1st prop ok.");
                Assert(dec.TryPlaceFurniture("home.owned", "prop.lamp", new WorldPoint(2f, 0f, 0f), 0f, out id).Succeeded, "2nd prop ok.");
                Assert(!dec.TryPlaceFurniture("home.owned", "prop.lamp", new WorldPoint(3f, 0f, 0f), 0f, out id).Succeeded, "3rd prop must hit cap.");
            }
            finally { DeleteDir(dir); }
        }

        private static void Decorator_BlockedInOnlineSession()
        {
            var ctx = new FakeGameContext { IsOnlineSession = true };
            var dec = new SafehouseDecoratorEngine(ctx, new FakeEconomyService(5000), new FailingPersistenceStore(), s => true, 10);
            dec.RegisterProp("prop.lamp", 100);
            dec.RegisterSafehouseBounds("home.owned", new WorldPoint(0f, 0f, 0f), 20f);

            string id;
            Assert(dec.TryPlaceFurniture("home.owned", "prop.lamp", new WorldPoint(0f, 0f, 0f), 0f, out id).Status == PurchaseStatus.BlockedInOnline,
                "Decorator must be blocked in Online.");
        }

        private static void ClassicNet_OpenNavigateBackAndHomeHistory()
        {
            var hub = new ClassicNetHub(new FakeGameContext());
            Assert(hub.Open() && hub.CurrentSite == ClassicNetSite.Home, "Expected Home on open.");
            Assert(hub.NavigateTo(ClassicNetSite.DynastyRealEstate), "Navigate to Dynasty.");
            Assert(hub.NavigateTo(ClassicNetSite.LcnStockExchange), "Navigate to LCN.");
            Assert(hub.HistoryDepth == 2, "Expected 2 history entries.");

            Assert(hub.GoBack() && hub.CurrentSite == ClassicNetSite.DynastyRealEstate, "Back -> Dynasty.");
            hub.GoHome();
            Assert(hub.CurrentSite == ClassicNetSite.Home, "GoHome -> Home.");
        }

        private static void ClassicNet_BlockedInOnlineAndCutscene()
        {
            var ctx = new FakeGameContext { IsOnlineSession = true };
            var hub = new ClassicNetHub(ctx);
            Assert(!hub.Open(), "ClassicNet must be blocked in Online.");

            ctx.IsOnlineSession = false;
            ctx.IsInCutscene = true;
            Assert(!hub.Open(), "ClassicNet must be blocked in cutscenes.");
        }

        private static void ClassicNet_ClosedBrowserIgnoresNavigation()
        {
            var hub = new ClassicNetHub(new FakeGameContext());
            Assert(!hub.NavigateTo(ClassicNetSite.PetKennel), "Closed browser must reject navigation.");
            Assert(!hub.GoBack(), "Closed browser must reject GoBack.");
        }
    }
}
