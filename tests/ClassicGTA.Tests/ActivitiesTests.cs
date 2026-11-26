using System;
using System.Collections.Generic;
using System.IO;
using ClassicGTA.Contracts;
using ClassicGTA.Core.Activities;
using ClassicGTA.Infrastructure;

namespace ClassicGTA.Tests
{
    internal sealed class FakeTaxiAdapter : ITaxiAdapter
    {
        public bool InTaxi { get; set; }
        public int TaxiHandle { get; set; }
        public WorldPoint PlayerPosition { get; set; }
        public WorldPoint PickupPos { get; set; }
        public WorldPoint DropoffPos { get; set; }
        public bool FareInVehicle { get; set; }
        public bool FareAlive { get; set; }
        public int EnterTasks { get; private set; }
        public int LeaveTasks { get; private set; }

        public FakeTaxiAdapter()
        {
            InTaxi = true;
            TaxiHandle = 900;
            PlayerPosition = new WorldPoint(0f, 0f, 0f);
            PickupPos = new WorldPoint(50f, 0f, 0f);
            DropoffPos = new WorldPoint(550f, 0f, 0f); // 500m trip
            FareAlive = true;
        }

        public bool IsPlayerInTaxi(out int vehicleHandle)
        {
            vehicleHandle = InTaxi ? TaxiHandle : 0;
            return InTaxi;
        }

        public WorldPoint GetPlayerPosition() { return PlayerPosition; }

        public bool TrySpawnFare(
            WorldPoint nearPlayer,
            out int farePedHandle,
            out WorldPoint pickupPosition,
            out WorldPoint dropoffPosition,
            out int blipHandle)
        {
            farePedHandle = 901;
            pickupPosition = PickupPos;
            dropoffPosition = DropoffPos;
            blipHandle = 902;
            return true;
        }

        public bool IsFareInVehicle(int farePedHandle, int vehicleHandle) { return FareInVehicle; }
        public bool IsEntityAlive(int handle) { return FareAlive; }
        public void TaskFareEnterVehicle(int farePedHandle, int vehicleHandle) { EnterTasks++; }
        public void TaskFareLeaveVehicle(int farePedHandle, int vehicleHandle) { LeaveTasks++; }
        public void SetRouteBlip(int blipHandle, WorldPoint destination) { }
    }

    internal static class ActivitiesTests
    {
        public static IEnumerable<KeyValuePair<string, Action>> GetAll()
        {
            yield return Pair("47_Taxi_FullFiveStateFlow_PaysFareAndSpeedTipOnce", Taxi_FullFiveStateFlow_PaysFareAndSpeedTipOnce);
            yield return Pair("48_Taxi_AbortNeverDeletesBorrowedPlayerTaxi_CleansOwnedFareAndBlip", Taxi_AbortNeverDeletesBorrowedPlayerTaxi_CleansOwnedFareAndBlip);
            yield return Pair("49_Taxi_FareDiesEnRoute_AbortsWithZeroPayout", Taxi_FareDiesEnRoute_AbortsWithZeroPayout);
            yield return Pair("50_Rampage_KillThresholdAwardsBronzeSilverGoldAndNeverDowngradesMedal", Rampage_KillThresholdAwardsBronzeSilverGoldAndNeverDowngradesMedal);
            yield return Pair("51_Rampage_TimerExpiresBeforeTarget_FailsWithZeroPayout", Rampage_TimerExpiresBeforeTarget_FailsWithZeroPayout);
            yield return Pair("52_Rampage_PersistsBestMedalsAcrossSessions", Rampage_PersistsBestMedalsAcrossSessions);
            yield return Pair("53_RpgSkills_LevelsUpClampsAt100AndComputesPerks", RpgSkills_LevelsUpClampsAt100AndComputesPerks);
            yield return Pair("54_RpgSkills_RejectsNegativeXpAndPersistsAcrossSessions", RpgSkills_RejectsNegativeXpAndPersistsAcrossSessions);
            yield return Pair("55_Blackjack_Natural21_PaysThreeToTwoImmediately", Blackjack_Natural21_PaysThreeToTwoImmediately);
            yield return Pair("56_Blackjack_SoftAceAdjustmentAndPlayerBust_SettlesCorrectly", Blackjack_SoftAceAdjustmentAndPlayerBust_SettlesCorrectly);
            yield return Pair("57_Blackjack_DealerStandsOn17_WinPushAndDoubleDown", Blackjack_DealerStandsOn17_WinPushAndDoubleDown);
            yield return Pair("58_Blackjack_BlocksOnlineOverBetAndPostSettlementActions", Blackjack_BlocksOnlineOverBetAndPostSettlementActions);
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

        private static Func<int> Deck(params int[] cards)
        {
            int idx = 0;
            return () => cards[idx++];
        }

        private static void Taxi_FullFiveStateFlow_PaysFareAndSpeedTipOnce()
        {
            var ctx = new FakeGameContext();
            var adapter = new FakeTaxiAdapter();
            var econ = new FakeEconomyService(100);
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var notify = new FakeNotificationService();

            var taxi = new TaxiMissionController(ctx, adapter, econ, reg, notify);
            taxi.Start();
            Assert(taxi.StartShift() && taxi.CurrentState == TaxiState.Searching, "Expected Searching.");

            taxi.SlowTick(1000);
            Assert(taxi.CurrentState == TaxiState.Pickup, "Expected Pickup after fare spawn.");

            adapter.PlayerPosition = adapter.PickupPos;
            taxi.SlowTick(2000);
            Assert(adapter.EnterTasks == 1, "Expected fare enter vehicle task.");

            adapter.FareInVehicle = true;
            taxi.SlowTick(3000);
            Assert(taxi.CurrentState == TaxiState.EnRoute, "Expected EnRoute.");

            // Arrive quickly at dropoff (500m trip -> base $50 + $100 distance = $150 + $37 speed tip = $187)
            adapter.PlayerPosition = adapter.DropoffPos;
            taxi.SlowTick(10000);
            Assert(taxi.CurrentState == TaxiState.Payout, "Expected Payout state.");
            Assert(taxi.LastPayout == 187 && econ.Cash == 287 && econ.CreditCalls == 1,
                "Expected $187 single payout, got $" + taxi.LastPayout);

            taxi.SlowTick(10250);
            Assert(taxi.CurrentState == TaxiState.Idle && econ.CreditCalls == 1, "Must return Idle with zero double payout.");
            Assert(!cleanup.DeletedEntities.Contains(adapter.TaxiHandle), "Borrowed player taxi must never be deleted.");
        }

        private static void Taxi_AbortNeverDeletesBorrowedPlayerTaxi_CleansOwnedFareAndBlip()
        {
            var adapter = new FakeTaxiAdapter();
            var cleanup = new FakeCleanupAdapter();
            var reg = new EntityOwnershipRegistry(cleanup);
            var taxi = new TaxiMissionController(new FakeGameContext(), adapter, new FakeEconomyService(0), reg, new FakeNotificationService());
            taxi.Start();
            taxi.StartShift();
            taxi.SlowTick(1000); // Spawns fare (901) and blip (902)

            // Player exits taxi -> triggers abort
            adapter.InTaxi = false;
            taxi.SlowTick(1500);

            Assert(taxi.CurrentState == TaxiState.Idle, "Expected Idle after exit.");
            Assert(!cleanup.DeletedEntities.Contains(900) && !cleanup.ReleasedEntities.Contains(900),
                "Borrowed player taxi (900) MUST NEVER be deleted or released.");
            Assert(cleanup.DeletedEntities.Contains(901) && cleanup.DeletedBlips.Contains(902),
                "Owned fare ped and blip must be cleaned up.");
        }

        private static void Taxi_FareDiesEnRoute_AbortsWithZeroPayout()
        {
            var adapter = new FakeTaxiAdapter();
            var econ = new FakeEconomyService(100);
            var reg = new EntityOwnershipRegistry(new FakeCleanupAdapter());
            var taxi = new TaxiMissionController(new FakeGameContext(), adapter, econ, reg, new FakeNotificationService());
            taxi.Start();
            taxi.StartShift();
            taxi.SlowTick(1000);
            adapter.FareAlive = false;
            taxi.SlowTick(1500);

            Assert(taxi.CurrentState == TaxiState.Idle && econ.Cash == 100, "Dead fare must abort with zero payout.");
        }

        private static void Rampage_KillThresholdAwardsBronzeSilverGoldAndNeverDowngradesMedal()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(0);
                var notify = new FakeNotificationService();
                var rampage = new RampageController(new FakeGameContext(), econ, new AtomicFileStore(dir, notify), notify);

                // Run 1: Fast 10/10 kills (under 60% time) -> Gold ($2000 = 2x $1000)
                Assert(rampage.StartRampage("rampage.docks", 10, 100000, 1000, 1000), "Expected start.");
                for (int i = 0; i < 10; i++) rampage.RecordKill(10000 + i * 1000);
                MedalRating m1 = rampage.FinishEarly(30000);
                Assert(m1 == MedalRating.Gold && rampage.GetBestMedal("rampage.docks") == MedalRating.Gold, "Expected Gold.");
                Assert(econ.Cash == 2000, "Expected 2x Gold reward ($2000), got $" + econ.Cash);

                // Run 2: Slow exact 10/10 kills -> Bronze, but best medal MUST remain Gold
                Assert(rampage.StartRampage("rampage.docks", 10, 100000, 1000, 200000), "Expected second start.");
                for (int i = 0; i < 10; i++) rampage.RecordKill(280000 + i * 1000);
                MedalRating m2 = rampage.FinishEarly(295000);
                Assert(m2 == MedalRating.Bronze, "Expected Bronze on slow exact-target run.");
                Assert(rampage.GetBestMedal("rampage.docks") == MedalRating.Gold, "Best medal MUST NEVER downgrade from Gold to Bronze.");
            }
            finally { DeleteDir(dir); }
        }

        private static void Rampage_TimerExpiresBeforeTarget_FailsWithZeroPayout()
        {
            string dir = CreateTempDir();
            try
            {
                var econ = new FakeEconomyService(500);
                var notify = new FakeNotificationService();
                var rampage = new RampageController(new FakeGameContext(), econ, new AtomicFileStore(dir, notify), notify);

                rampage.StartRampage("rampage.alley", 10, 60000, 1000, 1000);
                rampage.RecordKill(5000);
                rampage.Tick(65000); // Expired with only 1/10 kills

                Assert(!rampage.IsActive && rampage.LastEarnedMedal == MedalRating.None && econ.Cash == 500,
                    "Expired incomplete rampage must fail with zero payout.");
            }
            finally { DeleteDir(dir); }
        }

        private static void Rampage_PersistsBestMedalsAcrossSessions()
        {
            string dir = CreateTempDir();
            try
            {
                var notify = new FakeNotificationService();
                var store = new AtomicFileStore(dir, notify);
                var r1 = new RampageController(new FakeGameContext(), new FakeEconomyService(0), store, notify);

                r1.StartRampage("rampage.pier", 8, 100000, 500, 1000);
                for (int i = 0; i < 10; i++) r1.RecordKill(80000 + i * 100); // 10/8 = 1.25x -> Silver
                r1.FinishEarly(90000);

                var r2 = new RampageController(new FakeGameContext(), new FakeEconomyService(0), store, notify);
                Assert(r2.LoadState() && r2.GetBestMedal("rampage.pier") == MedalRating.Silver,
                    "Expected Silver medal persisted across sessions.");
            }
            finally { DeleteDir(dir); }
        }

        private static void RpgSkills_LevelsUpClampsAt100AndComputesPerks()
        {
            string dir = CreateTempDir();
            try
            {
                var store = new AtomicFileStore(dir, new FakeNotificationService());
                var rpg = new RpgSkillEngine(new FakeGameContext(), store);

                Assert(rpg.AddXp(RpgSkillType.Stamina, 5000) == 50, "Expected Stamina level 50.");
                Assert(Math.Abs(rpg.GetStaminaRegenMultiplier() - 1.5f) < 0.001f, "Expected 1.5x stamina regen at lv50.");

                Assert(rpg.AddXp(RpgSkillType.Shooting, 999999) == 100, "Shooting must clamp at level 100.");
                Assert(rpg.GetWeaponRecoilReductionPercent() == 35, "Expected 35% recoil reduction at lv100.");
            }
            finally { DeleteDir(dir); }
        }

        private static void RpgSkills_RejectsNegativeXpAndPersistsAcrossSessions()
        {
            string dir = CreateTempDir();
            try
            {
                var store = new AtomicFileStore(dir, new FakeNotificationService());
                var rpg = new RpgSkillEngine(new FakeGameContext(), store);

                bool threw = false;
                try { rpg.AddXp(RpgSkillType.Driving, -10); }
                catch (ArgumentOutOfRangeException) { threw = true; }
                Assert(threw, "Negative XP must throw.");

                rpg.AddXp(RpgSkillType.Driving, 2500);
                rpg.SaveState();

                var reloaded = new RpgSkillEngine(new FakeGameContext(), store);
                Assert(reloaded.LoadState() && reloaded.GetLevel(RpgSkillType.Driving) == 25,
                    "Expected Driving level 25 persisted.");
            }
            finally { DeleteDir(dir); }
        }

        private static void Blackjack_Natural21_PaysThreeToTwoImmediately()
        {
            var econ = new FakeEconomyService(1000);
            // Deal order: Player(Ace=1), Dealer(7), Player(King=13), Dealer(9) -> Player=21, Dealer=16
            var bj = new BlackjackEngine(new FakeGameContext(), econ, Deck(1, 7, 13, 9));

            Assert(bj.StartHand(100), "Expected hand to start.");
            Assert(!bj.IsRoundActive && bj.Outcome == BlackjackOutcome.PlayerBlackjack, "Expected immediate PlayerBlackjack.");
            Assert(econ.Cash == 1150, "Natural 21 on $100 bet must net +$150 (total $1150), got $" + econ.Cash);
        }

        private static void Blackjack_SoftAceAdjustmentAndPlayerBust_SettlesCorrectly()
        {
            var econ = new FakeEconomyService(1000);
            // Player: Ace(1) + 6 = 17; Hit 10 -> Ace downgrades to 1 -> 17 (NOT bust!); Hit 8 -> 25 (Bust!)
            var bj = new BlackjackEngine(new FakeGameContext(), econ, Deck(1, 5, 6, 10, 10, 8));

            bj.StartHand(100);
            Assert(bj.PlayerTotal == 17, "Expected soft 17.");

            bj.Hit(); // Draws 10 -> must downgrade Ace so total is 17, still active!
            Assert(bj.IsRoundActive && bj.PlayerTotal == 17, "Soft Ace must downgrade from 11 to 1 instead of busting.");

            bj.Hit(); // Draws 8 -> 25 -> Bust
            Assert(!bj.IsRoundActive && bj.Outcome == BlackjackOutcome.PlayerBust && econ.Cash == 900,
                "Expected PlayerBust and $900 remaining.");
        }

        private static void Blackjack_DealerStandsOn17_WinPushAndDoubleDown()
        {
            var econ = new FakeEconomyService(1000);
            // Player: 5 + 6 = 11; Dealer: 10 + 7 = 17 (stands); Player DoubleDown draws 9 -> 20 vs 17 -> Win $400 on $200 bet
            var bj = new BlackjackEngine(new FakeGameContext(), econ, Deck(5, 10, 6, 7, 9));

            bj.StartHand(100);
            BlackjackOutcome outcome = bj.DoubleDown();

            Assert(outcome == BlackjackOutcome.PlayerWin && bj.PlayerTotal == 20 && bj.DealerTotal == 17,
                "Expected DoubleDown win 20 vs 17.");
            Assert(econ.Cash == 1200, "Expected +$200 profit ($1200), got $" + econ.Cash);
        }

        private static void Blackjack_BlocksOnlineOverBetAndPostSettlementActions()
        {
            var ctx = new FakeGameContext { IsOnlineSession = true };
            var econ = new FakeEconomyService(1000);
            var bj = new BlackjackEngine(ctx, econ, Deck(10, 10, 9, 9));

            Assert(!bj.StartHand(100), "Blackjack must be blocked in Online.");

            ctx.IsOnlineSession = false;
            Assert(!bj.StartHand(999999), "Bet over max must be rejected.");
            Assert(bj.StartHand(100), "Valid bet should start.");
            bj.Stand(); // Push 19 vs 19 -> returns $100 (cash=1000)
            bj.Stand(); // Repeat stand after settlement must NOT double-credit!
            Assert(econ.Cash == 1000, "Post-settlement Stand must never double-credit, got $" + econ.Cash);
        }
    }
}
