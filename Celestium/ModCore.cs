using Assets.Code;
using Assets.Code.Modding;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private ComLibHooks Hooks;

        public static bool opt_EnableGod = true;

        public static int opt_SpawnPriority = 1;

        public bool Observatories = false;

        public List<Sub_NaturalWonder_CelestialObservatory_Lunar> LunarObservatories = new List<Sub_NaturalWonder_CelestialObservatory_Lunar>();

        public List<Sub_NaturalWonder_CelestialObservatory_Solar> SolarObservatories = new List<Sub_NaturalWonder_CelestialObservatory_Solar>();

        public Sprite[] TerrainAsh;

        public Sprite[] TerrainAshForest;

        public Sprite[] TerrainLavaSea;

        public override void onModsInitiallyLoaded()
        {
            TerrainAsh = new Sprite[4] { EventManager.getImg("ILGF_Celestium.hexAshPlains00.png"), EventManager.getImg("ILGF_Celestium.hexAshPlains01.png"), EventManager.getImg("ILGF_Celestium.hexAshPlains02.png"), EventManager.getImg("ILGF_Celestium.hexAshPlains03.png") };
            TerrainAshForest = new Sprite[4] { EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh00.png"), EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh01.png"), EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh02.png"), EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh03.png") };
            TerrainLavaSea = new Sprite[4] { EventManager.getImg("ILGF_Celestium.hexLavaSea00.png"), EventManager.getImg("ILGF_Celestium.hexLavaSea01.png"), EventManager.getImg("ILGF_Celestium.hexLavaSea02.png"), EventManager.getImg("ILGF_Celestium.hexLavaSea03.png") };
        }

        public override void onStartGamePresssed(Map map, List<God> gods)
        {
            Observatories = false;
            LunarObservatories.Clear();
            SolarObservatories.Clear();
        }

        public override void beforeMapGen(Map map)
        {
            _instance = this;
            Hooks = new ComLibHooks(map);
            GetModKernels(map);

            EventModifications(map);
        }

        public override void afterMapGenBeforeHistorical(Map map)
        {
            if (!Observatories)
            {
                return;
            }
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

        public virtual void EventModifications(Map map)
        {
            Dictionary<string, EventRuntime.Field> fields = EventRuntime.fields;
            Dictionary<string, EventRuntime.Property> properties = EventRuntime.properties;

            if (!fields.ContainsKey("is_wonder_lunarObservatory"))
            {
                fields.Add("is_wonder_lunarObservatory", new EventRuntime.TypedField<bool>((EventContext c) => c.location.settlement != null && c.location.settlement.subs.Any(sub => sub is Sub_NaturalWonder_CelestialObservatory_Lunar)));
            }
            if (!fields.ContainsKey("is_wonder_solarObservatory"))
            {
                fields.Add("is_wonder_solarObservatory", new EventRuntime.TypedField<bool>((EventContext c) => c.location.settlement != null && c.location.settlement.subs.Any(sub => sub is Sub_NaturalWonder_CelestialObservatory_Solar)));
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

        public override void populatingThreats(Overmind overmind, List<MsgEvent> threats)
        {
            if (!Observatories)
            {
                return;
            }

            foreach (Sub_NaturalWonder_CelestialObservatory_Lunar lunarObservatory in LunarObservatories)
            {
                foreach (Pair<Location, int> pair in lunarObservatory.Moonfall.TargetDelays_Locations)
                {
                    threats.Add(new MsgEvent($"Moonfall will strike {pair.Item1.getName()} in {pair.Item2} turns.", 0.5, true, pair.Item1.hex));
                }
            }

            foreach (Sub_NaturalWonder_CelestialObservatory_Solar solarObservatory in SolarObservatories)
            {
                foreach (Pair<Location, int> pair in solarObservatory.Starfall.TargetDelays_Locations)
                {
                    threats.Add(new MsgEvent($"Starfall will strike {pair.Item1.getName()} in {pair.Item2} turns.", 0.5, true, pair.Item1.hex));
                }
            }
        }

        public override void onCheatEntered(string command)
        {
            if (!Observatories)
            {
                return;
            }

            string commandLower = command.ToLowerInvariant();
            if (commandLower == "moonfall")
            {
                if (GraphicalMap.selectedHex != null)
                {
                    if (GraphicalMap.selectedHex.location != null)
                    {
                        LunarObservatories[0].Moonfall.cast(GraphicalMap.selectedHex.location);
                    }
                    else if (GraphicalMap.selectedHex.territoryOf != -1)
                    {
                        Location location = GraphicalMap.selectedHex.map.locations[GraphicalMap.selectedHex.territoryOf];
                        if (location != null)
                        {
                            LunarObservatories[0].Moonfall.cast(GraphicalMap.selectedHex.location);
                        }
                    }
                }
                else if (GraphicalMap.selectedUnit != null)
                {
                    LunarObservatories[0].Moonfall.cast(GraphicalMap.selectedUnit.location);
                }
            }
            else if (commandLower == "starfall")
            {
                if (GraphicalMap.selectedHex != null)
                {
                    if (GraphicalMap.selectedHex.location != null)
                    {
                        SolarObservatories[0].Starfall.cast(GraphicalMap.selectedHex.location);
                    }
                    else if (GraphicalMap.selectedHex.territoryOf != -1)
                    {
                        Location location = GraphicalMap.selectedHex.map.locations[GraphicalMap.selectedHex.territoryOf];
                        if (location != null)
                        {
                            SolarObservatories[0].Starfall.cast(GraphicalMap.selectedHex.location);
                        }
                    }
                }
                else if (GraphicalMap.selectedUnit != null)
                {
                    SolarObservatories[0].Starfall.cast(GraphicalMap.selectedUnit.location);
                }
            }
        }
    }
}
