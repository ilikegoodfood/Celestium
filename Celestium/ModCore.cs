using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class ModCore : ModKernel
    {
        private static ModCore _instance;

        public static ModCore Instance
        {
            get
            {
                return _instance;
            }
        }

        private static CommunityLib.ModCore _comLib;

        public static CommunityLib.ModCore CommunityLib
        {
            get
            {
                return _comLib;
            }
        }

        public ComLibHooks Hooks;

        public static bool opt_EnableGod = true;

        public static int opt_SpawnPriority = 1;

        public Sub_NaturalWonder_CelestialObservatory_Lunar LunarObservatory;

        public Sub_NaturalWonder_CelestialObservatory_Solar SolarObservatory;

        public Sprite[] terrainAsh;

        public Sprite[] terrainAshForest;

        public Sprite[] terrainLavaSea;

        public override void onModsInitiallyLoaded()
        {
            terrainAsh = new Sprite[4] { EventManager.getImg("ILGF_Celestium.hexAshPlains00.png"), EventManager.getImg("ILGF_Celestium.hexAshPlains01.png"), EventManager.getImg("ILGF_Celestium.hexAshPlains02.png"), EventManager.getImg("ILGF_Celestium.hexAshPlains03.png") };
            terrainAshForest = new Sprite[4] { EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh00.png"), EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh01.png"), EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh02.png"), EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh03.png") };
            terrainLavaSea = new Sprite[4] { EventManager.getImg("ILGF_Celestium.hexLavaSea00.png"), EventManager.getImg("ILGF_Celestium.hexLavaSea01.png"), EventManager.getImg("ILGF_Celestium.hexLavaSea02.png"), EventManager.getImg("ILGF_Celestium.hexLavaSea03.png") };
        }

        public override void onStartGamePresssed(Map map, List<God> gods)
        {
            LunarObservatory = null;
            SolarObservatory = null;
        }

        public override void beforeMapGen(Map map)
        {
            _instance = this;
            Hooks = new ComLibHooks(map);
            GetModKernels(map);
        }

        public override void afterLoading(Map map)
        {
            _instance = this;
            GetModKernels(map);
        }

        private void GetModKernels(Map map)
        {
            foreach (ModKernel kernel in map.mods)
            {
                switch(kernel.GetType().Namespace)
                {
                    case "CommunityLib":
                        _comLib = kernel as CommunityLib.ModCore;
                        if (_comLib == null)
                        {
                            Console.WriteLine("Celestium: ERROR: Failed to resolve the Community Library.");
                            break;
                        }
                        _comLib.RegisterHooks(Hooks);
                        break;
                    default:
                        break;
                }
            }
        }

        public override void receiveModConfigOpts_int(string optName, int value)
        {
            if (optName == "Celestial Temple Spawn Priority")
            {
                opt_SpawnPriority = value;
            }
        }

        public override void receiveModConfigOpts_bool(string optName, bool value)
        {
            if (optName == "Enable Celestium God")
            {
                opt_EnableGod = value;
            }
        }
    }
}
