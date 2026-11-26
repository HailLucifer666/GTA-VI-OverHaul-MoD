using ClassicGTA.Contracts;
using GTA;
using GTA.Native;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVGameContext : IGameContext
    {
        private const ulong HashNetworkIsSessionStarted = 0x9DE624D2FC4B603FUL;
        private const ulong HashNetworkIsInSession = 0xCA97246103B63917UL;

        public bool IsPlayerReady
        {
            get
            {
                Ped ped = Game.Player.Character;
                return ped != null && ped.Exists() && ped.IsAlive;
            }
        }

        public int PlayerHandle
        {
            get
            {
                Ped ped = Game.Player.Character;
                return ped != null && ped.Exists() ? ped.Handle : 0;
            }
        }

        public bool IsInCutscene { get { return Game.IsCutsceneActive; } }
        public bool IsPaused { get { return Game.IsPaused; } }
        public bool IsLoading { get { return Game.IsLoading; } }

        public bool IsOnlineSession
        {
            get
            {
                try
                {
                    return Function.Call<bool>((Hash)HashNetworkIsSessionStarted) ||
                           Function.Call<bool>((Hash)HashNetworkIsInSession);
                }
                catch
                {
                    // S-08 Fix: Fail closed — if session state cannot be determined, treat as Online.
                    return true;
                }
            }
        }

        public bool IsInVehicle
        {
            get
            {
                Ped ped = Game.Player.Character;
                return ped != null && ped.Exists() && ped.IsInVehicle();
            }
        }

        public bool IsScreenFadedOut { get { return GTA.UI.Screen.IsFadedOut; } }
        public int GameTimeMs { get { return Game.GameTime; } }
    }
}
