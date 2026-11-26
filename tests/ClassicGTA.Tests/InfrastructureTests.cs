using System;
using System.Collections.Generic;
using System.IO;
using ClassicGTA.Contracts;
using ClassicGTA.Infrastructure;

namespace ClassicGTA.Tests
{
    internal sealed class SpyModule : IModModule
    {
        private readonly List<string> _stopOrder;
        public string ModuleId { get; private set; }
        public string DisplayName { get { return ModuleId; } }
        public bool IsEnabled { get; set; }
        public bool ThrowOnFastTick { get; set; }
        public int FastTicks { get; private set; }
        public int SlowTicks { get; private set; }
        public bool Stopped { get; private set; }

        public SpyModule(string id, List<string> stopOrder = null)
        {
            ModuleId = id;
            IsEnabled = true;
            _stopOrder = stopOrder;
        }

        public void Start() { }

        public void FastTick()
        {
            if (ThrowOnFastTick) throw new InvalidOperationException("Simulated module crash");
            FastTicks++;
        }

        public void SlowTick(int gameTimeMs) { SlowTicks++; }
        public void OnKeyPressed(GameKey key) { }

        public void Stop()
        {
            Stopped = true;
            if (_stopOrder != null) _stopOrder.Add(ModuleId);
        }

        public void Dispose() { }
    }

    internal sealed class FakeCleanupAdapter : IEntityCleanupAdapter
    {
        public List<string> CallLog { get; private set; }
        public List<int> DeletedEntities { get; private set; }
        public List<int> ReleasedEntities { get; private set; }
        public List<int> DeletedBlips { get; private set; }
        public List<int> DeletedCameras { get; private set; }

        public FakeCleanupAdapter()
        {
            CallLog = new List<string>();
            DeletedEntities = new List<int>();
            ReleasedEntities = new List<int>();
            DeletedBlips = new List<int>();
            DeletedCameras = new List<int>();
        }

        public void DeleteEntity(int entityHandle)
        {
            DeletedEntities.Add(entityHandle);
            CallLog.Add("DeleteEntity:" + entityHandle);
        }

        public void MarkEntityNoLongerNeeded(int entityHandle)
        {
            ReleasedEntities.Add(entityHandle);
            CallLog.Add("ReleaseEntity:" + entityHandle);
        }

        public void DeleteBlip(int blipHandle)
        {
            DeletedBlips.Add(blipHandle);
            CallLog.Add("DeleteBlip:" + blipHandle);
        }

        public void RestoreRenderingCamera()
        {
            CallLog.Add("RestoreRenderingCamera");
        }

        public void DeleteCamera(int cameraHandle)
        {
            DeletedCameras.Add(cameraHandle);
            CallLog.Add("DeleteCamera:" + cameraHandle);
        }
    }

    internal static class InfrastructureTests
    {
        public static IEnumerable<KeyValuePair<string, Action>> GetAll()
        {
            yield return Pair("13_Scheduler_IsolatesFaultingModuleAndKeepsSiblingRunning", Scheduler_IsolatesFaultingModuleAndKeepsSiblingRunning);
            yield return Pair("14_Scheduler_StopsModulesInReverseOrder", Scheduler_StopsModulesInReverseOrder);
            yield return Pair("15_Scheduler_RejectsDuplicateModuleId", Scheduler_RejectsDuplicateModuleId);
            yield return Pair("16_Scheduler_ThrottlesSlowAndHeartbeatAndHandlesWrap", Scheduler_ThrottlesSlowAndHeartbeatAndHandlesWrap);
            yield return Pair("17_EntityRegistry_OwnedDeleted_BorrowedNeverTouched_DetachedReleased", EntityRegistry_OwnedDeleted_BorrowedNeverTouched_DetachedReleased);
            yield return Pair("18_EntityRegistry_EnforcesMaxOwnedEntityCap", EntityRegistry_EnforcesMaxOwnedEntityCap);
            yield return Pair("19_EntityRegistry_RestoresCameraBeforeDeletingCamera", EntityRegistry_RestoresCameraBeforeDeletingCamera);
            yield return Pair("20_EntityRegistry_TracksBlipsSeparatelyFromEntities", EntityRegistry_TracksBlipsSeparatelyFromEntities);
            yield return Pair("21_AtomicFileStore_SavesLoadsAndRotatesThreeBackups", AtomicFileStore_SavesLoadsAndRotatesThreeBackups);
            yield return Pair("22_AtomicFileStore_RecoversFromBackupWhenPrimaryCorrupted", AtomicFileStore_RecoversFromBackupWhenPrimaryCorrupted);
            yield return Pair("23_AtomicFileStore_RejectsEmptyPayloadWithoutHarmingPrimary", AtomicFileStore_RejectsEmptyPayloadWithoutHarmingPrimary);
            yield return Pair("24_SimpleIniConfig_ThrottlesFlushAndRoundTripsValues", SimpleIniConfig_ThrottlesFlushAndRoundTripsValues);
        }

        private static KeyValuePair<string, Action> Pair(string name, Action body)
        {
            return new KeyValuePair<string, Action>(name, body);
        }

        private static void Assert(bool cond, string msg)
        {
            if (!cond) throw new InvalidOperationException(msg);
        }

        private static string CreateTempTestDir()
        {
            // Keep test temp artifacts strictly inside repo TestResults\ on E:\ (never C:\Temp!)
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestScratch", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void SafeDeleteDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }

        private static void Scheduler_IsolatesFaultingModuleAndKeepsSiblingRunning()
        {
            var logger = new FakeNotificationService();
            var scheduler = new ModuleScheduler(logger);
            var bad = new SpyModule("mod.bad") { ThrowOnFastTick = true };
            var good = new SpyModule("mod.good");

            scheduler.Register(bad);
            scheduler.Register(good);
            scheduler.StartAll();

            scheduler.Tick(1000);
            scheduler.Tick(1016);

            Assert(scheduler.IsFaulted("mod.bad"), "Faulting module must be marked Faulted.");
            Assert(bad.Stopped, "Faulting module must be stopped.");
            Assert(!scheduler.IsFaulted("mod.good"), "Healthy sibling must not fault.");
            Assert(good.FastTicks == 2, "Healthy sibling must keep ticking after neighbor faults.");
            Assert(logger.Logs.Exists(l => l.Contains("Simulated module crash")), "Fault reason must be logged.");
        }

        private static void Scheduler_StopsModulesInReverseOrder()
        {
            var logger = new FakeNotificationService();
            var order = new List<string>();
            var scheduler = new ModuleScheduler(logger);
            scheduler.Register(new SpyModule("first", order));
            scheduler.Register(new SpyModule("second", order));
            scheduler.Register(new SpyModule("third", order));
            scheduler.StartAll();
            scheduler.StopAll();

            Assert(order.Count == 3 && order[0] == "third" && order[1] == "second" && order[2] == "first",
                "Expected reverse stop order third -> second -> first.");
        }

        private static void Scheduler_RejectsDuplicateModuleId()
        {
            var scheduler = new ModuleScheduler(new FakeNotificationService());
            scheduler.Register(new SpyModule("dup.mod"));
            bool threw = false;
            try
            {
                scheduler.Register(new SpyModule("DUP.MOD"));
            }
            catch (InvalidOperationException)
            {
                threw = true;
            }
            Assert(threw, "Expected duplicate ModuleId registration to throw.");
        }

        private static void Scheduler_ThrottlesSlowAndHeartbeatAndHandlesWrap()
        {
            var scheduler = new ModuleScheduler(new FakeNotificationService(), 250, 1000);
            var mod = new SpyModule("mod.timer");
            scheduler.Register(mod);
            scheduler.StartAll();

            scheduler.Tick(1000); // slow #1, hb #1
            scheduler.Tick(1100); // fast only
            scheduler.Tick(1250); // slow #2
            scheduler.Tick(2000); // slow #3, hb #2
            scheduler.Tick(100);  // wrap-around -> slow #4, hb #3

            Assert(mod.FastTicks == 5, "Expected 5 fast ticks.");
            Assert(mod.SlowTicks == 4, "Expected 4 slow ticks including wrap-around.");
            Assert(scheduler.HeartbeatCount == 3, "Expected 3 heartbeats including wrap-around.");
        }

        private static void EntityRegistry_OwnedDeleted_BorrowedNeverTouched_DetachedReleased()
        {
            var adapter = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(adapter, 10);

            reg.TryTrackEntity(101, "taxi", EntityOwnership.Owned);
            reg.TryTrackEntity(202, "taxi", EntityOwnership.Borrowed);
            reg.TryTrackEntity(303, "taxi", EntityOwnership.Detached);

            reg.CleanupOwner("taxi");

            Assert(adapter.DeletedEntities.Contains(101), "Owned entity must be deleted.");
            Assert(!adapter.DeletedEntities.Contains(202) && !adapter.ReleasedEntities.Contains(202),
                "Borrowed entity (player vehicle) must NEVER be deleted or released.");
            Assert(adapter.ReleasedEntities.Contains(303) && !adapter.DeletedEntities.Contains(303),
                "Detached entity must be marked no longer needed, not deleted.");
            Assert(reg.TrackedEntityCount == 0, "All tracked entries for owner must be cleared.");
        }

        private static void EntityRegistry_EnforcesMaxOwnedEntityCap()
        {
            var adapter = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(adapter, 2);

            Assert(reg.TryTrackEntity(1, "mod", EntityOwnership.Owned), "First owned entity should succeed.");
            Assert(reg.TryTrackEntity(2, "mod", EntityOwnership.Owned), "Second owned entity should succeed.");
            Assert(!reg.TryTrackEntity(3, "mod", EntityOwnership.Owned), "Third owned entity must be rejected by cap.");
            Assert(reg.TryTrackEntity(4, "mod", EntityOwnership.Borrowed), "Borrowed entities should not consume owned cap.");
        }

        private static void EntityRegistry_RestoresCameraBeforeDeletingCamera()
        {
            var adapter = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(adapter);

            reg.TrackCamera(55, "fps");
            reg.CleanupOwner("fps");

            int restoreIdx = adapter.CallLog.IndexOf("RestoreRenderingCamera");
            int deleteIdx = adapter.CallLog.IndexOf("DeleteCamera:55");

            Assert(restoreIdx >= 0 && deleteIdx > restoreIdx,
                "RestoreRenderingCamera MUST be called strictly before DeleteCamera.");
            Assert(reg.TrackedCameraCount == 0, "Tracked camera count must return to 0.");
        }

        private static void EntityRegistry_TracksBlipsSeparatelyFromEntities()
        {
            var adapter = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(adapter);

            reg.TryTrackEntity(77, "dispatch", EntityOwnership.Owned);
            reg.TrackBlip(77, "dispatch"); // Same numeric handle space, distinct domain

            Assert(reg.TrackedEntityCount == 1 && reg.TrackedBlipCount == 1, "Entity and blip must be tracked separately.");
            reg.CleanupOwner("dispatch");
            Assert(adapter.DeletedEntities.Contains(77) && adapter.DeletedBlips.Contains(77),
                "Both entity and blip must be cleaned up.");
            Assert(reg.TrackedEntityCount == 0 && reg.TrackedBlipCount == 0, "Counts must return to zero.");
        }

        private static void AtomicFileStore_SavesLoadsAndRotatesThreeBackups()
        {
            string dir = CreateTempTestDir();
            try
            {
                var store = new AtomicFileStore(dir, new FakeNotificationService());
                store.Save("profile.json", "{\"rev\":1}");
                store.Save("profile.json", "{\"rev\":2}");
                store.Save("profile.json", "{\"rev\":3}");
                store.Save("profile.json", "{\"rev\":4}");

                string loaded;
                Assert(store.TryLoad("profile.json", s => s.Contains("\"rev\":"), out loaded), "Expected primary load.");
                Assert(loaded == "{\"rev\":4}", "Expected latest revision 4.");
                Assert(File.ReadAllText(Path.Combine(dir, "profile.json.bak1")) == "{\"rev\":3}", "bak1 should be rev 3.");
                Assert(File.ReadAllText(Path.Combine(dir, "profile.json.bak2")) == "{\"rev\":2}", "bak2 should be rev 2.");
                Assert(File.ReadAllText(Path.Combine(dir, "profile.json.bak3")) == "{\"rev\":1}", "bak3 should be rev 1.");
            }
            finally { SafeDeleteDir(dir); }
        }

        private static void AtomicFileStore_RecoversFromBackupWhenPrimaryCorrupted()
        {
            string dir = CreateTempTestDir();
            try
            {
                var logger = new FakeNotificationService();
                var store = new AtomicFileStore(dir, logger);
                store.Save("profile.json", "{\"rev\":1,\"ok\":true}");
                store.Save("profile.json", "{\"rev\":2,\"ok\":true}");

                // Corrupt primary file on disk
                File.WriteAllText(Path.Combine(dir, "profile.json"), "CORRUPT_PARTIAL_WRITE");

                string loaded;
                bool ok = store.TryLoad("profile.json", s => s.StartsWith("{") && s.EndsWith("}"), out loaded);
                Assert(ok && loaded == "{\"rev\":1,\"ok\":true}", "Expected automatic fallback to .bak1.");
                Assert(logger.Logs.Exists(l => l.Contains(".bak1")), "Expected recovery log citing .bak1.");
            }
            finally { SafeDeleteDir(dir); }
        }

        private static void AtomicFileStore_RejectsEmptyPayloadWithoutHarmingPrimary()
        {
            string dir = CreateTempTestDir();
            try
            {
                var store = new AtomicFileStore(dir, new FakeNotificationService());
                store.Save("profile.json", "{\"rev\":1}");

                bool threw = false;
                try { store.Save("profile.json", "   "); }
                catch (ArgumentException) { threw = true; }

                string loaded;
                Assert(threw, "Expected empty payload save to throw.");
                Assert(store.TryLoad("profile.json", null, out loaded) && loaded == "{\"rev\":1}",
                    "Valid primary file must remain intact.");
            }
            finally { SafeDeleteDir(dir); }
        }

        private static void SimpleIniConfig_ThrottlesFlushAndRoundTripsValues()
        {
            string dir = CreateTempTestDir();
            try
            {
                var store = new AtomicFileStore(dir, new FakeNotificationService());
                var ini = new SimpleIniConfig(store, "ClassicGTA.ini", 5000);
                ini.Set("General", "Enabled", "true");
                ini.Set("Keys", "PocketArsenalKey", "F10");

                Assert(ini.FlushIfDirty(1000), "First flush should write.");
                ini.Set("General", "Enabled", "false");
                Assert(!ini.FlushIfDirty(2000), "Second flush inside 5000ms window must be throttled.");
                Assert(ini.FlushIfDirty(6000), "Flush after 5000ms window must write.");

                var reloaded = new SimpleIniConfig(store, "ClassicGTA.ini", 5000);
                Assert(reloaded.Load(), "Reload should succeed.");
                Assert(!reloaded.GetBool("General", "Enabled", true), "Expected Enabled=false after reload.");
                Assert(reloaded.GetString("Keys", "PocketArsenalKey", "") == "F10", "Expected F10 after reload.");
            }
            finally { SafeDeleteDir(dir); }
        }
    }
}
