using ClassicGTA.Contracts;
using GTA;
using GTA.Native;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVGameContext : IGameContext
    {
        private const ulong HashNetworkIsSessionStarted = 0x9DE624D2FC4B603FUL;

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
            get { return Function.Call<bool>((Hash)HashNetworkIsSessionStarted); }
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
