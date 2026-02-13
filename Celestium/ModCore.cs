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

        private Dictionary<string, ModIntegrationData> _modIntegrationData;

        public Dictionary<string, ModIntegrationData> ModIntegrationData => _modIntegrationData;

        private ComLibHooks Hooks;

        public static bool opt_EnableGod = true;

        public static int opt_SpawnPriority = 1;

        public bool Observatories = false;

        public bool Celestium = false;

        public God_Celestium CelestiumGod = null;

        public List<Sub_NaturalWonder_CelestialObservatory_Lunar> LunarObservatories = new List<Sub_NaturalWonder_CelestialObservatory_Lunar>();

        public List<Sub_NaturalWonder_CelestialObservatory_Solar> SolarObservatories = new List<Sub_NaturalWonder_CelestialObservatory_Solar>();

        public Dictionary<Settlement, List<Sub_NaturalWonder_CelestialObservatory_Solar>> SettlementsEffectedBySolarObservatories = new Dictionary<Settlement, List<Sub_NaturalWonder_CelestialObservatory_Solar>>();

        public Sprite[] TerrainAsh;

        public Sprite[] TerrainAshForest;

        public Sprite[] TerrainLavaSea;

        public Sprite[] TerrainBoilingSea;

        public float LavaTemperatureThreshold = 3f;

        public float SubterraneanLavaTemperatureThreshold = 3f;

        public float VolanicTemperatureThreshold = 2f;

        public float AshTemperatureThreshold = 1.25f;

        public override void onStartGamePresssed(Map map, List<God> gods)
        {
            Observatories = false;
            LunarObservatories.Clear();
            SolarObservatories.Clear();
            Celestium = false;
        }

        public override void beforeMapGen(Map map)
        {
            _instance = this;
            Hooks = new ComLibHooks(map);
            GetModKernels(map);

            EventModifications(map);
            LoadTerrainGraphics();
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

            EventModifications(map);
            LoadTerrainGraphics();

            if (map.overmind.god is God_Celestium celestium)
            {
                Celestium = true;
                CelestiumGod = celestium;
            }

            UpdateSaveGame(map);
        }

        private void GetModKernels(Map map)
        {
            _modIntegrationData = new Dictionary<string, ModIntegrationData>();

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
                        Hooks = new ComLibHooks(map);
                        break;
                    case "CustomVoidGod":
                        Console.WriteLine("CommunityLib: The Living Void is Enabled");
                        ModIntegrationData intDataVoid = new ModIntegrationData(kernel);
                        _modIntegrationData.Add("LivingVoid", intDataVoid);

                        if (_modIntegrationData.TryGetValue("LivingVoid", out intDataVoid))
                        {
                            Type godType = intDataVoid.assembly.GetType("CustomVoidGod.God_Vacuum", false);
                            if (godType != null)
                            {
                                intDataVoid.typeDict.Add("LivingVoid", godType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get The living void god Type (CustomVoidGod.God_Vacuum)");
                            }
                        }
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

        public void LoadTerrainGraphics()
        {
            TerrainAsh = new Sprite[4] { EventManager.getImg("ILGF_Celestium.hexAshPlains00.png"), EventManager.getImg("ILGF_Celestium.hexAshPlains01.png"), EventManager.getImg("ILGF_Celestium.hexAshPlains02.png"), EventManager.getImg("ILGF_Celestium.hexAshPlains03.png") };
            TerrainAshForest = new Sprite[4] { EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh00.png"), EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh01.png"), EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh02.png"), EventManager.getImg("ILGF_Celestium.hexForestBurnedAsh03.png") };
            TerrainLavaSea = new Sprite[4] { EventManager.getImg("ILGF_Celestium.hexLavaSea00.png"), EventManager.getImg("ILGF_Celestium.hexLavaSea01.png"), EventManager.getImg("ILGF_Celestium.hexLavaSea02.png"), EventManager.getImg("ILGF_Celestium.hexLavaSea03.png") };
            TerrainBoilingSea = new Sprite[1] { EventManager.getImg("ILGF_Celestium.hexBoilingSea00.png") };
        }

        public void UpdateSaveGame(Map map)
        {
            LavaTemperatureThreshold = 3f;
            SubterraneanLavaTemperatureThreshold = 3f;
            VolanicTemperatureThreshold = 2f;
            AshTemperatureThreshold = 1.25f;
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
                    threats.Add(new MsgEvent($"Moonfall will strike {pair.Item1.getName()} in {pair.Item2} turns.", 0.6 - (0.01 * pair.Item2), true, pair.Item1.hex));
                }
            }

            foreach (Sub_NaturalWonder_CelestialObservatory_Solar solarObservatory in SolarObservatories)
            {
                foreach (Pair<Location, int> pair in solarObservatory.Starfall.TargetDelays_Locations)
                {
                    threats.Add(new MsgEvent($"Starfall will strike {pair.Item1.getName()} in {pair.Item2} turns.", 0.6 - (0.01 * pair.Item2), true, pair.Item1.hex));
                }
            }

            if (!Celestium || CelestiumGod == null)
            {
                return;
            }

            foreach (Pr_Hotspot hotspot in CelestiumGod.Hotspots)
            {
                threats.Add(new MsgEvent($"{hotspot.getName()} ({(int)Math.Round(hotspot.charge)} charge) at {hotspot.location.getName()}.", ((hotspot.charge / 300.0) * hotspot.Size) / 3, true, hotspot.location.hex));
            }

            foreach (Unit unit in overmind.map.units)
            {
                if (!(unit is UA ua) || !(ua.task is Task_PerformChallenge challengeTask) || !(challengeTask.challenge is Mg_DeathOfTheSun death))
                {
                    continue;
                }

                threats.Add(new MsgEvent($"{ua.getName()} is performing {death.getName()}", ua.getStatLore() / 10, false, ua.location.hex));
            }
        }

        public override void onTurnEnd(Map map)
        {
            RecalculateSolarObservatoryTargets();

            if (!(map.overmind.god is God_Celestium celestium))
            {
                return;
            }
            Celestium = true;
        }

        public override void onTurnStart(Map map)
        {
            RecalculateSolarObservatoryTargets();

            if (!(map.overmind.god is God_Celestium celestium))
            {
                if (CelestiumGod != null)
                {
                    map.addUnifiedMessage(CelestiumGod.Settlement.location, null, "A Star Extinguished", "Whether extinguisged, or dormant, or driven out by an even more terrifying power, Celestium has vanished from the surface of the world. The magma cools, the fields of ash blow away in the winds. The world begins to cool rapidly. Perhaps the future will be birghter with a little less light in it.", "CELESTIUM REPLACED", true);

                    foreach (KeyValuePair<Hex, God_Celestium.TemperatureModifier> kvp in CelestiumGod.TemperatureMap)
                    {
                        Hex hex = kvp.Key;
                        God_Celestium.TemperatureModifier modifier = kvp.Value;
                        map.tempMap[hex.x][hex.y] -= modifier.Total;
                        hex.transientTempDelta += modifier.Total;
                        modifier.Global = 0f;
                        modifier.Inner = 0f;
                        modifier.Outer = 0f;

                        if (modifier.IsLavaSurface)
                        {
                            modifier.IsLavaSurface = false;
                            hex.volcanicDamage = 50;
                        }
                        if (modifier.IsLavaUnderground)
                        {
                            modifier.IsLavaUnderground = false;
                            Hex undergroundHex = map.grid[1][hex.x][hex.y];
                            if (undergroundHex != null)
                            {
                                undergroundHex.volcanicDamage = 50;
                            }
                        }
                    }
                    CelestiumGod = null;
                }

                return;
            }

            Celestium = true;
            if (CelestiumGod == null)
            {
                CelestiumGod = celestium;
            }
            else if (celestium != CelestiumGod)
            {
                foreach (KeyValuePair<Hex, God_Celestium.TemperatureModifier> kvp in CelestiumGod.TemperatureMap)
                {
                    Hex hex = kvp.Key;
                    God_Celestium.TemperatureModifier modifier = kvp.Value;
                    map.tempMap[hex.x][hex.y] -= modifier.Total;
                    hex.transientTempDelta += modifier.Total;
                    modifier.Global = 0f;
                    modifier.Inner = 0f;
                    modifier.Outer = 0f;

                    if (modifier.IsLavaSurface)
                    {
                        modifier.IsLavaSurface = false;
                        hex.volcanicDamage = 50;
                    }
                    if (modifier.IsLavaUnderground)
                    {
                        modifier.IsLavaUnderground = false;
                        Hex undergroundHex = map.grid[1][hex.x][hex.y];
                        if (undergroundHex != null)
                        {
                            undergroundHex.volcanicDamage = 50;
                        }
                    }
                }
                CelestiumGod = celestium;
            }

            if (map.overmind.endOfGameAchieved && !celestium.Victory)
            {
                celestium.Defeated = true;
            }
        }

        public void RecalculateSolarObservatoryTargets()
        {
            SettlementsEffectedBySolarObservatories.Clear();
            foreach (Sub_NaturalWonder_CelestialObservatory_Solar solarObservatory in SolarObservatories)
            {
                if (SettlementsEffectedBySolarObservatories.TryGetValue(solarObservatory.settlement, out List<Sub_NaturalWonder_CelestialObservatory_Solar> effectingObservatroies))
                {
                    if (effectingObservatroies == null)
                    {
                        SettlementsEffectedBySolarObservatories[solarObservatory.settlement] = new List<Sub_NaturalWonder_CelestialObservatory_Solar> { solarObservatory };
                    }
                    else if (!effectingObservatroies.Contains(solarObservatory))
                    {
                        effectingObservatroies.Add(solarObservatory);
                    }
                }
                else
                {
                    SettlementsEffectedBySolarObservatories.Add(solarObservatory.settlement, new List<Sub_NaturalWonder_CelestialObservatory_Solar> { solarObservatory });
                }

                if (SettlementsEffectedBySolarObservatories.TryGetValue(solarObservatory.LunarObservatory.settlement, out effectingObservatroies))
                {
                    if (effectingObservatroies == null)
                    {
                        SettlementsEffectedBySolarObservatories[solarObservatory.LunarObservatory.settlement] = new List<Sub_NaturalWonder_CelestialObservatory_Solar> { solarObservatory };
                    }
                    else if (!effectingObservatroies.Contains(solarObservatory))
                    {
                        effectingObservatroies.Add(solarObservatory);
                    }
                }
                else
                {
                    SettlementsEffectedBySolarObservatories.Add(solarObservatory.LunarObservatory.settlement, new List<Sub_NaturalWonder_CelestialObservatory_Solar> { solarObservatory });
                }

                if (solarObservatory.SolarObservationDuration > 0)
                {
                    foreach (Location neighbour in solarObservatory.settlement.location.getNeighbours())
                    {
                        if (neighbour.settlement == null)
                        {
                            continue;
                        }

                        if (SettlementsEffectedBySolarObservatories.TryGetValue(neighbour.settlement, out effectingObservatroies))
                        {
                            if (effectingObservatroies == null)
                            {
                                SettlementsEffectedBySolarObservatories[neighbour.settlement] = new List<Sub_NaturalWonder_CelestialObservatory_Solar> { solarObservatory };
                            }
                            if (!effectingObservatroies.Contains(solarObservatory))
                            {
                                effectingObservatroies.Add(solarObservatory);
                            }
                        }
                        else
                        {
                            SettlementsEffectedBySolarObservatories.Add(neighbour.settlement, new List<Sub_NaturalWonder_CelestialObservatory_Solar> { solarObservatory });
                        }
                    }
                }
            }
        }

        public override void onCheatEntered(string command)
        {
            if (!Observatories)
            {
                return;
            }

            string commandLower = command.ToLowerInvariant().Trim();
            string[] commandComps = commandLower.Split(' ');
            if (commandComps[0] == "moonfall")
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
            else if (commandComps[0] == "starfall")
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
            else if (commandComps[0] == "celestium")
            {
                if (commandComps.Length > 1)
                {
                    if (commandComps[1] == "move")
                    {
                        if (CelestiumGod == null || CelestiumGod != World.staticMap.overmind.god)
                        {
                            return;
                        }

                        if (GraphicalMap.selectedHex == null || GraphicalMap.selectedHex.z != 0 || GraphicalMap.selectedHex.location == null || GraphicalMap.selectedHex.location == CelestiumGod.Settlement.location)
                        {
                            return;
                        }

                        P_Celestium_Move move = (P_Celestium_Move)World.staticMap.overmind.god.powers.FirstOrDefault(p => p is P_Celestium_Move);
                        if (move == null)
                        {
                            return;
                        }

                        move.castCommon(GraphicalMap.selectedHex.location);
                    }
                }
                else
                {
                    if (Celestium)
                    {
                        return;
                    }

                    Sub_NaturalWonder_CelestialObservatory_Solar solarObservatory;
                    UA agent = (UA)World.staticMap.overmind.agents.FirstOrDefault(unit => unit is UA);

                    if (SolarObservatories.Count > 0 && agent != null)
                    {
                        solarObservatory = SolarObservatories[0];
                        Ch_Sunfall sunfall = (Ch_Sunfall)solarObservatory.Challenges.FirstOrDefault(ch => ch is Ch_Sunfall);

                        if (sunfall != null)
                        {
                            sunfall.complete(agent);
                        }
                    }
                }
            }
        }

        public override void onGraphicalHexUpdated(GraphicalHex graphicalHex)
        {
            Hex hex = graphicalHex.hex;
            if (hex == null)
            {
                return;
            }

            float temperature = hex.map.tempMap[hex.x][hex.y];
            if (temperature < AshTemperatureThreshold)
            {
                return;
            }

            bool isLand = hex.map.landmass[hex.x][hex.y];
            if (CelestiumGod == null)
            {
                if (temperature >= LavaTemperatureThreshold)
                {
                    if (isLand)
                    {
                        graphicalHex.terrainLayer.sprite = TerrainLavaSea[hex.graphicalIndexer % TerrainLavaSea.Length];
                    }
                    else
                    {
                        graphicalHex.terrainLayer.sprite = TerrainBoilingSea[0];
                    }
                    return;
                }
            }
            else if (CelestiumGod.TemperatureMap.TryGetValue(graphicalHex.map.grid[0][hex.x][hex.y], out God_Celestium.TemperatureModifier modifier))
            {
                if (hex.z == 1)
                {
                    if (modifier.IsLavaUnderground)
                    {
                        if (isLand)
                        {
                            graphicalHex.terrainLayer.sprite = TerrainLavaSea[hex.graphicalIndexer % TerrainLavaSea.Length];
                        }
                        else
                        {
                            graphicalHex.terrainLayer.sprite = TerrainBoilingSea[0];
                        }
                        return;
                    }
                }
                else if (modifier.IsLavaSurface)
                {
                    if (isLand)
                    {
                        graphicalHex.terrainLayer.sprite = TerrainLavaSea[hex.graphicalIndexer % TerrainLavaSea.Length];
                    }
                    else
                    {
                        graphicalHex.terrainLayer.sprite = TerrainBoilingSea[0];
                    }
                    return;
                }
            }
            else if (temperature >= LavaTemperatureThreshold)
            {
                if (isLand)
                {
                    graphicalHex.terrainLayer.sprite = TerrainLavaSea[hex.graphicalIndexer % TerrainLavaSea.Length];
                }
                else
                {
                    graphicalHex.terrainLayer.sprite = TerrainBoilingSea[0];
                }
                return;
            }

            if (hex.z == 1 || !isLand || hex.volcanicDamage > 0 || hex.isMountain)
            {
                return;
            }

            if (hex.isForest)
            {
                graphicalHex.terrainLayer.sprite = TerrainAshForest[hex.graphicalIndexer % TerrainAshForest.Length];
            }
            else
            {
                graphicalHex.terrainLayer.sprite = TerrainAsh[hex.graphicalIndexer % TerrainAsh.Length];
            }
        }

        public override float hexHabitability(Hex hex, float hab)
        {
            if (CelestiumGod == null || hex.location == null)
            {
                return hab;
            }

            if (CelestiumGod.TemperatureMap.TryGetValue(hex.map.grid[0][hex.x][hex.y], out God_Celestium.TemperatureModifier modifier))
            {
                if (hex.z == 1)
                {
                    if (modifier.IsLavaUnderground)
                    {
                        return -1f;
                    }
                }
                else if (modifier.IsLavaSurface)
                {
                    return -1f;
                }
            }

            return hab;
        }
    }
}
