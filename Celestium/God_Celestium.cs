using Assets.Code;
using Assets.Code.Modding;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Celestium
{
    public class God_Celestium : God
    {
        public float LavaTemperatureThreshold = 3f;

        public float VolanicTemperatureThreshold = 2f;

        public float AshTemperatureThreshold = 1.25f;

        public Set_Celestium Settlement;

        public bool Victory = false;

        public bool Defeated = false;

        public class TemperatureModifier
        {
            public int Distance;

            public float Global;

            public float Outer;

            public float Inner;

            public bool IsLava;

            public float Total
            {
                get
                {
                    return Global + Outer + Inner;
                }
            }
        }

        public HashSet<Hex> InnerHexCache = new HashSet<Hex>();

        public HashSet<Hex> OuterHexCache = new HashSet<Hex>();

        public Dictionary<Hex, TemperatureModifier> TemperatureMap = new Dictionary<Hex, TemperatureModifier>();

        public int InnerRadius = 2;

        public int OuterRadius = 5;

        public float InnerThermalLimit = 5f;

        public float OuterThermalLimit = 1f;

        public float GlobalThermalLimit = 0f;

        public float ShadowToGlobalThermalConversion = 0.001f; // Shadow per 0.01 temperature: ~1000

        public float ShadowConversionRate = 0.1f;

        public float TemperatureIncrementRate = 0.1f;

        public float TemperatureDecrimentRate = -0.01f;

        public double Menace = 0.0;

        public List<Pr_Hotspot> Hotspots = new List<Pr_Hotspot>();

        public override void setup(Map map)
        {
            base.setup(map);

            ModCore.Instance.Celestium = true;

            map.overmind.sealProgress = 2;
            map.overmind.sealsBroken = 1;
            map.overmind.power = 3 * map.overmind.sealsBroken + 6;

            powers.Add(new P_Celestium_Grow(map));
            powerLevelReqs.Add(1);

            powers.Add(new P_Celestium_Flare(map));
            powerLevelReqs.Add(1);

            powers.Add(new P_Celestium_FindHotspots(map));
            powerLevelReqs.Add(2);

            powers.Add(new P_Celestium_BurnSoul(map));
            powerLevelReqs.Add(3);

            powers.Add(new P_Celestium_Move(map));
            powerLevelReqs.Add(4);
        }

        public override string getName()
        {
            return "Celestium";
        }

        public override string getDescFlavour()
        {
            return "A star is born. A Celestial, barely hours old, shines brilliantly enough to burn the world itself, vaporizing the shadowy remnets of any prior elder being that thought to claim it as their own.";
        }

        public override string getDescMechanics()
        {
            return "Celestium's mechanics revolve around the production of power, rampant destruction, and temperature changes. If provided with a steady supply of power and shadow Celestium can lay waste to the world by heating it until it becomes completely uninhabitable. Even the Dwarves aren't safe in their caverns. As it extends its reach, the global temperature will rise, and humanity will preish beneath the scorching ash that will coat the lands.";
        }

        public override bool selectable()
        {
            return false;
        }

        public override string lockMessage()
        {
            return "Celestium can only be birthed in an acitve game.";
        }

        public override string getCredits()
        {
            return "Mod Creator: ILikeGoodFood";
        }

        public override Sprite getSupplicant()
        {
            return EventManager.getImg("ILGF_Celestium.Fore_Supplicant.png");
        }

        public override bool hasSupplicantStartingTraits()
        {
            return false;
        }

        public override bool usesConventionalSeals()
        {
            return false;
        }

        public override int[] getSealLevels()
        {
            return new int[] {
                1,
                50,
                170,
                300
            };
        }

        public override void breakSeal(int sealsBroken)
        {
            map.overmind.power += 3.0;
            map.world.prefabStore.popMsgSeal();

            if (sealsBroken == 2 && !map.overmind.agentsGeneric.Any(ua => ua is UAE_Abstraction_Ember))
            {
                map.overmind.agentsGeneric.Add(new UAE_Abstraction_Ember(map));
            }
        }

        public override string getSealDesc()
        {
            if (Defeated)
            {
                return "Seals hold your God back from Awakening and employing their full power. Now that the stars are right, the seals will break, until the God returns. For each broken seal your maximum power increases by 3, and you may gain new abilities and agent capacity.";
            }

            return $"Temperature Thresholds: {map.overmind.sealsBroken - 1} of {getSealLevels().Count() - 1}\n\nCelestium's power is directly tied to the temperature modifier that it applies to the world. Once a temperature threshold is crossed, Celestium gains new powers, and may gain an increased agent limit.";
        }

        public override int[] getAgentCaps()
        {
            return new int[] {
                5,
                5,
                5,
                6,
                6
            };
        }

        public override int getMaxTurns()
        {
            return 500;
        }

        public override Sprite getGodBackground(World world)
        {
            return EventManager.getImg("ILGF_Celestium.Img_StarBirth.jpg");
        }

        public override Sprite getGodPortrait(World world)
        {
            return EventManager.getImg("ILGF_Celestium.Fore_Celestium.png");
        }

        public override double getWorldPanicOnAwake()
        {
            return 0.75;
        }

        public override void awaken()
        {
            ComputeHexDistances(true);
            GraphicalMap.selectedUnit = null;
            GraphicalMap.selectedHex = Settlement.location.hex;
            GraphicalMap.panTo(Settlement.location.hex);

            EventManager.ActiveEvent activeEvent = EventManager.events.Values.FirstOrDefault(ae => ae.type == EventData.Type.INERT && ae.data.id.Contains("CelestiumWakes"));
            if (activeEvent == null)
            {
                map.world.prefabStore.popMsg("UNABLE TO FIND CELESTIUM WAKES EVENT", true, true);
            }
            else
            {
                map.world.prefabStore.popEvent(activeEvent.data, EventContext.withNothing(map));
            }

            base.awaken();
            if (map.opt_enableMagic && map.opt_allowMagicalArmsRace)
            {
                map.overmind.magicalArmsRace = 1.0;
            }

            map.hintSystem.popCustomHint("Mechanics", "Celestium is a being of eternal, brilliant light. It has neither need nor ability to spread its power and influence over the people of the world. Instead, it gains new powers by growing and burning the world until it can overwhelm the world's inhabitants with cataclysmic climate change.");
        }

        public override string getAwakenMessage()
        {
            return getDescFlavour();
        }

        private void ComputeHexDistances(bool initializeRadii = false)
        {
            bool tradeNetworkUpdateRequired = false;
            foreach (Hex[] column in map.grid)
            {
                foreach (Hex hex in column)
                {
                    if (hex == null)
                    {
                        continue;
                    }

                    TemperatureModifier modifier = new TemperatureModifier();
                    modifier.Distance = HexGridUtils.HexDistance(Settlement.location.hex, hex);
                    TemperatureMap.Add(hex, modifier);
                    if (modifier.Distance <= InnerRadius)
                    {
                        InnerHexCache.Add(hex);
                    }
                    if (modifier.Distance <= OuterRadius)
                    {
                        OuterHexCache.Add(hex);
                    }

                    if (initializeRadii)
                    {
                        if (IncrementHexTemperatureModifier(hex, modifier, false, true, true))
                        {
                            tradeNetworkUpdateRequired = true;
                        }
                    }
                }
            }

            if (initializeRadii)
            {
                if (tradeNetworkUpdateRequired)
                {
                    map.tradeManager.cached.Clear();
                    map.tradeManager.checkTradeNetwork();
                }
            }
        }

        public void UpdateHexDistances(bool initializeRadii = false)
        {
            InnerHexCache.Clear();
            OuterHexCache.Clear();

            foreach (var kvp in TemperatureMap)
            {
                kvp.Value.Distance = HexGridUtils.HexDistance(kvp.Key, Settlement.location.hex);
                if (kvp.Value.Distance <= OuterRadius)
                {
                    if (kvp.Value.Distance <= InnerRadius)
                    {
                        InnerHexCache.Add(kvp.Key);
                    }

                    OuterHexCache.Add(kvp.Key);
                }
            }
        }

        public override void turnTick()
        {
            if (Defeated)
            {
                InnerRadius = 0;
                OuterRadius = 0;
                InnerThermalLimit = 0f;
                OuterThermalLimit = 0f;
                GlobalThermalLimit = 0f;

                map.overmind.sealProgress = 0;
                map.overmind.sealsBroken = 0;
                awake = false;
            }
            else if (awake && map.overmind.sealsBroken < getSealLevels().Length - 1)
            {
                // Only if the final seal has not already been broken.
                map.overmind.sealProgress = (int)Math.Ceiling(GlobalThermalLimit * 1000);
                if (map.overmind.sealProgress >= getSealLevels()[map.overmind.sealsBroken])
                {
                    map.overmind.sealsBroken++;
                    breakSeal(map.overmind.sealsBroken);
                }
            }

            Hotspots.Clear();
            // Burn Global Shadow from Hexes, converting from locations
            // Also repopulate Hotspots list.
            if (!Defeated)
            {
                foreach (Hex[] column in map.grid)
                {
                    foreach (Hex hex in column)
                    {
                        if (hex == null)
                        {
                            continue;
                        }

                        if (hex.location != null)
                        {
                            foreach (Property property in hex.location.properties)
                            {
                                if (property is Pr_Hotspot hotspot)
                                {
                                    Hotspots.Add(hotspot);
                                }
                                else if (property is Pr_GeomanticLocus locus)
                                {
                                    if (!locus.challenges.Any(ch => ch is Mg_DeathOfTheSun))
                                    {
                                        locus.challenges.Add(new Mg_DeathOfTheSun(hex.location, locus));
                                    }
                                }
                            }
                        }

                        if (hex.purity >= 1f)
                        {
                            continue;
                        }

                        Location loc = hex.location;
                        if (loc == null)
                        {
                            hex.purity += ShadowConversionRate;
                            if (hex.purity > 1f)
                            {
                                hex.purity = 1f;
                            }
                        }
                        else
                        {
                            if (loc.settlement == null)
                            {
                                float shadowBurn = Math.Min(ShadowConversionRate, 1f - loc.hex.purity);
                                loc.hex.purity += shadowBurn;

                                if (loc.hex.purity > 1f)
                                {
                                    loc.hex.purity = 1f;
                                }

                                GlobalThermalLimit += shadowBurn * ShadowToGlobalThermalConversion;
                            }
                        }
                    }
                }

                // Burn Global shadow from living People
                foreach (Person person in map.persons)
                {
                    if (person.isDead && (person.unit == null || !CommunityLib.ModCore.Get().checkIsUnitSubsumed(person.unit)))
                    {
                        continue;
                    }

                    float shadowBurn = (float)Math.Min(person.shadow, ShadowConversionRate);
                    person.shadow -= shadowBurn;

                    if (person.shadow < 0.0)
                    {
                        person.shadow = 0.0;
                    }

                    GlobalThermalLimit += shadowBurn * ShadowToGlobalThermalConversion;
                }
            }

            // Run temperature map updates, and deal turnTick Lava damage
            bool tradeNetworkUpdateRequired = false;
            List<Unit> killedUnits = new List<Unit>();
            foreach (Hex[] column in map.grid)
            {
                foreach (Hex hex in column)
                {
                    if (hex == null || !TemperatureMap.TryGetValue(hex, out TemperatureModifier modifier))
                    {
                        continue;
                    }

                    // Temperature Update
                    if (IncrementHexTemperatureModifier(hex, modifier))
                    {
                        tradeNetworkUpdateRequired = true;
                    }

                    // Turn Tick Lava Damage
                    for (int mapLayer = 0; mapLayer < map.grid.Length; mapLayer++)
                    {
                        if (!modifier.IsLava)
                        {
                            continue;
                        }

                        Hex hex2 = map.grid[hex.x][hex.y];
                        if (hex2 != null && hex2.location != null)
                        {
                            killedUnits.Clear();
                            foreach (Unit unit in hex2.location.units)
                            {
                                if (unit is UA && unit.task is Task_PerformChallenge challengeTask && challengeTask.challenge is Ch_ReforgeTheSeals)
                                {
                                    continue;
                                }

                                unit.hp -= (int)Math.Ceiling(0.05 * unit.maxHp);

                                if (unit.hp <= 0)
                                {
                                    killedUnits.Add(unit);
                                }
                            }

                            foreach (Unit unit in killedUnits)
                            {
                                unit.die(map, "Burned to death travelling too close to Celestium.");
                            }
                        }
                    }
                }
            }

            if (tradeNetworkUpdateRequired)
            {
                map.tradeManager.cached.Clear();
                map.tradeManager.checkTradeNetwork();
            }

            GraphicalMap.map.assignTerrainFromClimate();
        }

        #region TemperatureManagement
        public bool IncrementHexTemperatureModifier(Hex hex, TemperatureModifier modifier, bool updateTradeRoutes = false, bool maximiseInner = false, bool maximiseOuter = false, bool maximiseGlobal = false)
        {
            float temperatureDelta = 0f;
            temperatureDelta += IncrementHexGlobalTemperatureModifier(hex, modifier, maximiseGlobal);
            temperatureDelta += IncrementHexOuterTemperatureModifier(hex, modifier, maximiseOuter);
            temperatureDelta += IncrementHexInnerTemperatureModifier(hex, modifier, maximiseInner);

            bool tradeNetworkUpdateRequired = false;
            float[] tempMapRow = map.tempMap[hex.x];
            if (temperatureDelta != 0f)
            {
                tempMapRow[hex.y] += temperatureDelta;
                tradeNetworkUpdateRequired = ManageLavaHex(hex, modifier, tempMapRow[hex.y]);

                if (hex.location != null && hex.settlement != null)
                {
                    if (hex.settlement is SettlementHuman)
                    {
                        if (hex.getHabilitability() < map.param.mapGen_minHabitabilityForHumans)
                        {
                            if (hex.location.settlement is SettlementHuman && !(hex.location.soc.isDark() || (hex.location.soc is HolyOrder order && order.worshipsThePlayer)))
                            {
                                Menace += map.param.um_firstDaughterMenaceGain;
                            }

                            hex.location.settlement.fallIntoRuin("Climate unable to support human life", this);
                        }
                    }
                    else if (hex.settlement is Set_OrcCamp)
                    {
                        if (hex.getHabilitability() < map.param.orc_habRequirement * map.opt_orcHabMult)
                        {
                            hex.location.settlement.fallIntoRuin("Climate unable to support orc life", this);
                        }
                    }
                }

                if (tradeNetworkUpdateRequired && updateTradeRoutes)
                {
                    map.tradeManager.cached.Clear();
                    map.tradeManager.checkTradeNetwork();
                }
            }

            if (!modifier.IsLava)
            {
                if (tempMapRow[hex.y] >= VolanicTemperatureThreshold && hex.volcanicDamage < 10)
                {
                    hex.volcanicDamage = 10;
                }
            }

            return tradeNetworkUpdateRequired;
        }

        public float IncrementHexGlobalTemperatureModifier(Hex hex, TemperatureModifier modifier, bool maximise = false, bool applyTemperatureChange = false)
        {
            float totalDelta = 0f;
            float delta;
            if (modifier.Global < GlobalThermalLimit)
            {
                if (maximise)
                {
                    delta = GlobalThermalLimit - modifier.Global;
                    modifier.Global += delta;
                    totalDelta += delta;
                }
                else
                {
                    delta = Mathf.Min(TemperatureIncrementRate, GlobalThermalLimit - modifier.Global);
                    modifier.Global += delta;
                    totalDelta += delta;
                }
            }
            else if (modifier.Global > GlobalThermalLimit)
            {
                if (maximise)
                {
                    delta = modifier.Global - GlobalThermalLimit;
                    modifier.Global = GlobalThermalLimit;
                    totalDelta += delta;
                }
                else
                {
                    delta = Mathf.Max(TemperatureDecrimentRate, GlobalThermalLimit - modifier.Global);
                    modifier.Global += delta;
                    totalDelta += delta;
                }
            }

            if (applyTemperatureChange)
            {
                float[] tempMapRow = map.tempMap[hex.x];
                tempMapRow[hex.y] += totalDelta;

                if (tempMapRow[hex.y] >= VolanicTemperatureThreshold && hex.volcanicDamage < 10)
                {
                    hex.volcanicDamage = 10;
                }

                bool tradeNetworkUpdateRequired = ManageLavaHex(hex, modifier, tempMapRow[hex.y]);

                if (hex.location != null && hex.settlement != null)
                {
                    if (hex.settlement is SettlementHuman)
                    {
                        if (hex.getHabilitability() < map.param.mapGen_minHabitabilityForHumans)
                        {
                            if (hex.location.settlement is SettlementHuman && !(hex.location.soc.isDark() || (hex.location.soc is HolyOrder order && order.worshipsThePlayer)))
                            {
                                Menace += map.param.um_firstDaughterMenaceGain;
                            }

                            hex.location.settlement.fallIntoRuin("Climate unable to support human life", this);
                        }
                    }
                    else if (hex.settlement is Set_OrcCamp)
                    {
                        if (hex.getHabilitability() < map.param.orc_habRequirement * map.opt_orcHabMult)
                        {
                            hex.location.settlement.fallIntoRuin("Climate unable to support orc life", this);
                        }
                    }
                }

                if (tradeNetworkUpdateRequired)
                {
                    map.tradeManager.cached.Clear();
                    map.tradeManager.checkTradeNetwork();
                }
            }

            return totalDelta;
        }

        public float IncrementHexOuterTemperatureModifier(Hex hex, TemperatureModifier modifier, bool maximise = false, bool applyTemperatureChange = false)
        {
            float totalDelta = 0f;
            float delta;
            if (modifier.Distance <= OuterRadius)
            {
                float outerThermalLimit = GetOuterThermalLimit(modifier.Distance);
                if (modifier.Outer < outerThermalLimit)
                {
                    if (maximise)
                    {
                        delta = outerThermalLimit - modifier.Outer;
                        modifier.Outer += delta;
                        totalDelta += delta;
                    }
                    else
                    {
                        delta = Mathf.Min(TemperatureIncrementRate, outerThermalLimit - modifier.Outer);
                        modifier.Outer += delta;
                        totalDelta += delta;
                    }
                }
                else if (modifier.Outer > outerThermalLimit)
                {
                    if (maximise)
                    {
                        delta = modifier.Outer - outerThermalLimit;
                        modifier.Outer = outerThermalLimit;
                        totalDelta += delta;
                    }
                    else
                    {
                        delta = Mathf.Max(TemperatureDecrimentRate, outerThermalLimit - modifier.Outer);
                        modifier.Outer += delta;
                        totalDelta += delta;
                    }
                }
            }
            else
            {
                if (maximise)
                {
                    totalDelta -= modifier.Outer;
                    modifier.Outer = 0f;
                }
                else if (modifier.Outer < 0f)
                {
                    delta = Mathf.Min(TemperatureIncrementRate, OuterThermalLimit);
                    modifier.Outer += delta;
                    totalDelta += delta;
                }
                else if (modifier.Outer > 0f)
                {
                    delta = Mathf.Max(TemperatureDecrimentRate, -modifier.Outer);
                    modifier.Outer += delta;
                    totalDelta += delta;
                }
            }

            if (applyTemperatureChange)
            {
                float[] tempMapRow = map.tempMap[hex.x];
                tempMapRow[hex.y] += totalDelta;

                if (tempMapRow[hex.y] >= VolanicTemperatureThreshold && hex.volcanicDamage < 10)
                {
                    hex.volcanicDamage = 10;
                }

                bool tradeNetworkUpdateRequired = ManageLavaHex(hex, modifier, tempMapRow[hex.y]);

                if (hex.location != null && hex.settlement != null)
                {
                    if (hex.settlement is SettlementHuman)
                    {
                        if (hex.getHabilitability() < map.param.mapGen_minHabitabilityForHumans)
                        {
                            if (hex.location.settlement is SettlementHuman && !(hex.location.soc.isDark() || (hex.location.soc is HolyOrder order && order.worshipsThePlayer)))
                            {
                                Menace += map.param.um_firstDaughterMenaceGain;
                            }

                            hex.location.settlement.fallIntoRuin("Climate unable to support human life", this);
                        }
                    }
                    else if (hex.settlement is Set_OrcCamp)
                    {
                        if (hex.getHabilitability() < map.param.orc_habRequirement * map.opt_orcHabMult)
                        {
                            hex.location.settlement.fallIntoRuin("Climate unable to support orc life", this);
                        }
                    }
                }

                if (tradeNetworkUpdateRequired)
                {
                    map.tradeManager.cached.Clear();
                    map.tradeManager.checkTradeNetwork();
                }
            }

            return totalDelta;
        }

        public float IncrementHexInnerTemperatureModifier(Hex hex, TemperatureModifier modifier, bool maximise = false, bool applyTemperatureChange = false)
        {
            float totalDelta = 0f;
            float delta;
            if (modifier.Distance <= InnerRadius)
            {
                if (modifier.Inner < InnerThermalLimit)
                {
                    if (maximise)
                    {
                        delta = InnerThermalLimit - modifier.Inner;
                        modifier.Inner += delta;
                        totalDelta += delta;
                    }
                    else
                    {
                        delta = Mathf.Min(TemperatureIncrementRate, InnerThermalLimit - modifier.Inner);
                        modifier.Inner += delta;
                        totalDelta += delta;
                    }
                }
                else if (modifier.Inner > InnerThermalLimit)
                {
                    if (maximise)
                    {
                        delta = modifier.Inner - InnerThermalLimit;
                        modifier.Inner = InnerThermalLimit;
                        totalDelta += delta;
                    }
                    else
                    {
                        delta = Mathf.Max(TemperatureDecrimentRate, InnerThermalLimit - modifier.Inner);
                        modifier.Inner += delta;
                        totalDelta += delta;
                    }
                }
            }
            else
            {
                if (maximise)
                {
                    totalDelta -= modifier.Inner;
                    modifier.Inner = 0f;
                }
                else if (modifier.Inner < 0f)
                {
                    delta = Mathf.Min(TemperatureIncrementRate, InnerThermalLimit);
                    modifier.Inner += delta;
                    totalDelta += delta;
                }
                else if (modifier.Inner > 0f)
                {
                    delta = Mathf.Max(TemperatureDecrimentRate, -modifier.Inner);
                    modifier.Inner += delta;
                    totalDelta += delta;
                }
            }

            if (applyTemperatureChange)
            {
                float[] tempMapRow = map.tempMap[hex.x];
                tempMapRow[hex.y] += totalDelta;

                if (tempMapRow[hex.y] >= VolanicTemperatureThreshold && hex.volcanicDamage < 10)
                {
                    hex.volcanicDamage = 10;
                }

                bool tradeNetworkUpdateRequired = ManageLavaHex(hex, modifier, tempMapRow[hex.y]);

                if (hex.location != null && hex.settlement != null)
                {
                    if (hex.settlement is SettlementHuman)
                    {
                        if (hex.getHabilitability() < map.param.mapGen_minHabitabilityForHumans)
                        {
                            if (hex.location.settlement is SettlementHuman && !(hex.location.soc.isDark() || (hex.location.soc is HolyOrder order && order.worshipsThePlayer)))
                            {
                                Menace += map.param.um_firstDaughterMenaceGain;
                            }

                            hex.location.settlement.fallIntoRuin("Climate unable to support human life", this);
                        }
                    }
                    else if (hex.settlement is Set_OrcCamp)
                    {
                        if (hex.getHabilitability() < map.param.orc_habRequirement * map.opt_orcHabMult)
                        {
                            hex.location.settlement.fallIntoRuin("Climate unable to support orc life", this);
                        }
                    }
                }

                if (tradeNetworkUpdateRequired)
                {
                    map.tradeManager.cached.Clear();
                    map.tradeManager.checkTradeNetwork();
                }
            }

            return totalDelta;
        }

        public bool ManageLavaHex(Hex hex, TemperatureModifier modifier, float temperature, bool updateTradeNetwork = false)
        {
            bool tradeNetworkUpdateRequired = false;
            if (temperature >= LavaTemperatureThreshold)
            {
                if (!modifier.IsLava)
                {
                    tradeNetworkUpdateRequired = ConvertHexToLava(hex, modifier, updateTradeNetwork);
                }
            }
            else if (modifier.IsLava)
            {
                modifier.IsLava = false;
            }

            return tradeNetworkUpdateRequired;
        }

        public bool ConvertHexToLava(Hex hex, TemperatureModifier modifier, bool updateTradeNetwork = false)
        {
            bool tradeNetworkUpdateRequired = false;
            if (hex == null)
            {
                return tradeNetworkUpdateRequired;
            }

            if (hex.location != null)
            {
                if (hex.location.settlement != null)
                {
                    if (hex.location.settlement is SettlementHuman && (hex.location.soc == null || !(hex.location.soc.isDark() || (hex.location.soc is HolyOrder order && order.worshipsThePlayer))))
                    {
                        Menace += map.param.um_firstDaughterMenaceGain;
                    }

                    if (hex.location.settlement is SettlementHuman humanSettlement && humanSettlement.supportedMilitary != null)
                    {
                        humanSettlement.supportedMilitary.die(map, "Home location subsumed by lava");
                    }
                    hex.location.settlement.fallIntoRuin("Subsumed by lava", this);
                }
                else if (!CommunityLib.ModCore.Get().checkIsNaturalWonder(hex.location))
                {
                    List<Property> propertiesToRemove = new List<Property>();
                    foreach (Property property in hex.location.properties)
                    {
                        if (property.removedOnRuin())
                        {
                            propertiesToRemove.Add(property);
                        }
                    }

                    foreach (Property property in propertiesToRemove)
                    {
                        hex.location.properties.Remove(property);
                    }
                }

                List<TradeRoute> tradeRoutes = map.tradeManager.tradeDensity[hex.location.index];
                List<TradeRoute> tradeRoutesToRemove = new List<TradeRoute>();
                if (tradeRoutes != null && tradeRoutes.Count > 0)
                {
                    foreach (TradeRoute localTradeRoute in tradeRoutes)
                    {
                        tradeRoutesToRemove.Add(localTradeRoute);
                    }

                    if (tradeRoutesToRemove.Count > 0)
                    {
                        tradeNetworkUpdateRequired = true;

                        foreach (TradeRoute tradeRouteToRemove in tradeRoutesToRemove)
                        {
                            map.tradeManager.routes.Remove(tradeRouteToRemove);

                            foreach (Location step in tradeRouteToRemove.path)
                            {
                                map.tradeManager.tradeDensity[step.index].Remove(tradeRouteToRemove);
                            }
                        }

                        if (updateTradeNetwork)
                        {
                            map.tradeManager.cached.Clear();
                            map.tradeManager.checkTradeNetwork();
                        }
                    }
                }
            }

            if (map.landmass[hex.x][hex.y])
            {
                hex.terrain = Hex.terrainType.VOLCANO;
                modifier.IsLava = true;
            }
            else
            {
                modifier.IsLava = true;
            }

            return tradeNetworkUpdateRequired;
        }
        #endregion

        public float GetOuterThermalLimit(double distance)
        {
            return (float)Math.Min(OuterThermalLimit + (OuterRadius * 0.1) - (distance * 0.1), 5f);
        }

        public override List<Power> getPowers()
        {
            List<Power> list = new List<Power>();
            for (int i = 0; i < powers.Count; i++)
            {
                if (!powers[i].isPassiveOnly())
                {
                    bool flag2 = powerLevelReqs[i] <= map.overmind.sealsBroken;
                    if (flag2)
                    {
                        list.Add(powers[i]);
                    }
                }
            }
            return list;
        }

        public override int getMaxPower()
        {
            return 3 * map.overmind.sealsBroken + 6;
        }

        public override double getPowerPerTurn()
        {
            return map.param.overmind_powerRegen * getMaxPower() * map.difficultyMult_shrinkWithDifficulty;
        }

        public override void turnTick_PowerGain()
        {
            map.overmind.power += getPowerPerTurn();
            if (map.overmind.power > getMaxPower())
            {
                map.overmind.power = getMaxPower();
            }
        }

        public void Grow()
        {
            GlobalThermalLimit += 0.03f;
            if (((InnerRadius + OuterRadius) & 1) == 0)
            {
                OuterRadius++;
            }
            else
            {
                OuterRadius += 2;
                InnerRadius++;
            }

            List<Hex> outerHexes = HexGridUtils.HexesWithinRadius(map, Settlement.location.hex, OuterRadius, out List<Hex>[] byDistance);
            if (outerHexes.Count == 0)
            {
                World.Log("Celestium: ERROR: Failed to update distance caches");
            }

            for (int d = 0; d < byDistance.Length; d++)
            {
                foreach (Hex hex in byDistance[d])
                {
                    if (d < InnerRadius)
                    {
                        InnerHexCache.Add(hex);
                    }
                    
                    OuterHexCache.Add(hex);
                }
            }
        }

        public override string getVictoryMessage(int victoryMode)
        {
            Victory = true;
            switch(victoryMode)
            {
                case 0:
                    return "The remnants of power from the being that previously claimed this world have ensnared its inhabitants in a malaise of apathy, hopelessness, and darkness. They celebrate your dawning light, unaware of the consequences of doing so. None shall oppose your growth, and many shall aid it.";
                case 1:
                    return "They have been blinded by your light. They fumble madly in the darkness caused by your radiance. They have lost even the power to observe the sun, and as such, they are powerless over you. As their insane ramblings climb in torturous pitch, you grow, unseen and unopposed.";
                case 2:
                    return "An empire born in the name of your predecessor, now bent to your will, dominates the world. It guards you from the malice of the world's remaining free inhabitants, feeds you the heat of the world, and builds temples of flame in your name.";
                case 3:
                    int overheatCount = 0;
                    foreach (Location location in map.locations)
                    {
                        if (map.tempMap[location.hex.x][location.hex.y] >= 1.0)
                        {
                            overheatCount++;
                        }
                    }

                    if (overheatCount >= 0.5 * map.locations.Count)
                    {
                        return "Fire and ash bathe the world. The once great-empires that opposed you melt into the softening rock, scarcely a transient wrinkle on the fabric of time. The world burns beneath your omnipresent brilliance. It shrinks as it melts and boils. Ash and smoke rise into the sky, only to be sucked into you as you swell, faster and faster, larger and larger, brighter and brighter... And soon it will all be gone. And all that will remain is Celestium, a newborn star, shrouded in the spiraling remnants of its birthplace.";
                    }
                    return "Fire and ash. The once-great empires that opposed you burn, like so many motes of light in a darkness you shall never see. The pyres of their demise are but glimmers before your radiance, their transience exemplified by their vast dimness. The world itself has yet to catch aflame, but those upon it have, and their charred remains are but kindling for your eternal fire.";
                case 4:
                    return "Your radiance chills those it falls upon to their(?) core. A cold star, a thing of unfeeling, unquenchable malice. Your light does not bring life to a world, and your world is no longer illuminated by its birth star. Here, only you shine across the ice sheets and frozen tundra, sheltered from view beneath clouds that block out the sun.";
                case 5:
                    return "From the darkest, dampest depths of the world, you have drawn life, casting light and heat into their once perpetually black domain. These mutant beasts, clinging to their water and shade, have consumed your enemies. Their fear of you is insurmountable, and your growth inevitable.";
                case 6:
                    return "";
            }

            return "";
        }

        public override void victoryBoxPopulating(PopupVictory specific)
        {
            specific.image.sprite = EventManager.getImg("ILGF_Celestium.Img_StarVictory.jpg");
        }
    }
}
