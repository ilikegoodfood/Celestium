using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class God_Celestium : God
    {
        public float LavaTemperatureThreshold = 3f;

        public float VolanicTemperatureThreshold = 2f;

        public float AshTemperatureThreshold = 1.25f;

        public Set_Celestium Settlement;

        public class TemperatureModifier
        {
            public double Distance;

            public float Global;

            public float Outer;

            public float Inner;

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

        public HashSet<Location> MeltedLocations = new HashSet<Location>();

        public int InnerRadius = 2;

        public int OuterRadius = 5;

        public float InnerThermalLimit = 5f;

        public float OuterThermalLimit = 1f;

        public float GlobalThermalLimit = 0f;

        public float ShadowToGlobalThermalConversion = 0.001f; // Shadow per 0.01 temperature: ~1000

        public float ShadowConversionRate = 0.1f;

        public float TemperatureIncrementRate = 0.035f;

        public override void setup(Map map)
        {
            base.setup(map);

            map.overmind.sealProgress = 1;
            map.overmind.sealsBroken = 1;
            map.overmind.power = 3 * map.overmind.sealsBroken + 6;
        }

        public override string getName()
        {
            return "Celestium";
        }

        public override string getDescFlavour()
        {
            return "A star is born. A Celestial, barely hours old, shines brilliantly enough to burn the world itself, vaporizing the shadowy remnets of any prior elder being that thought to claim theis world as it's own.";
        }

        public override string getDescMechanics()
        {
            return "Celestium's mechanics revolve around the production of power, rampant destruction, and temperature changes. If provided with a steady supply of power Celestium can lay watse to the world by altering the movments of the celestial bodies. If it burns enough shadow, the global temperature will rise, and humanity will preish beneath the scorching ash that will coat the lands.";
        }

        public override bool selectable()
        {
            return false;
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

        public override void breakSeal(int sealsBroken)
        {
            map.overmind.power += 3.0;
            map.world.prefabStore.popMsgSeal();
        }

        public override int[] getSealLevels()
        {
            return new int[] {
                0,
                64,
                160,
                328
            };
        }

        public override string getSealDesc()
        {
            return "Seals represent the gorwth of your God's power. Now that Celestium has been born, he will continue to grow in power, breaking seals. For each broken seal your maximum power increases by 3, and you may gain new abilities and agent capacity.";
        }

        public override int[] getAgentCaps()
        {
            return new int[] {
                5,
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
            base.awaken();
        }

        public void ComputeHexDistances(bool initializeRadii = false)
        {
            foreach (Hex[] column in map.grid[0])
            {
                foreach (Hex hex in column)
                {
                    if (hex == null)
                    {
                        continue;
                    }

                    double distance = Math.Sqrt((hex.x - Settlement.location.hex.x) * (hex.x - Settlement.location.hex.x) + (hex.y - Settlement.location.hex.y) * (hex.y - Settlement.location.hex.y));
                    TemperatureModifier modifier = new TemperatureModifier();
                    modifier.Distance = distance;
                    TemperatureMap.Add(hex, modifier);

                    if (initializeRadii)
                    {
                        IncrementHexTemperatureModifier(hex, modifier, true);
                    }
                }
            }

            if (initializeRadii)
            {
                map.assignTerrainFromClimate();
            }
        }

        public override void turnTick()
        {
            // Since Celestium is awake and still needs to tick seals, tick them here.
            if (awake && map.overmind.sealsBroken < getSealLevels().Length - 1)
            {
                // Only if the fnal seal has not already been broken.
                map.overmind.godSealTick();
            }

            // Burn Glkobal SHadow from Hexes, converting from locations
            foreach (Hex[][] mapLayer in map.grid)
            {
                foreach (Hex[] column in mapLayer)
                {
                    foreach (Hex hex in column)
                    {
                        if (hex == null || hex.purity >= 1f)
                        {
                            continue;
                        }

                        Location loc = hex.location;
                        if (loc == null)
                        {
                            hex.purity += (float)ShadowConversionRate;
                            if (hex.purity > 1f)
                            {
                                hex.purity = 1f;
                            }
                        }
                        else if (loc.settlement == null)
                        {
                            float shadowBurn = Math.Min(ShadowConversionRate, 1f - loc.hex.purity);
                            loc.hex.purity += (float)shadowBurn;

                            if (loc.hex.purity > 1f)
                            {
                                loc.hex.purity = 1f;
                            }

                            GlobalThermalLimit += shadowBurn * ShadowToGlobalThermalConversion;
                        }
                        else
                        {
                            float shadowBurn = (float)Math.Min(ShadowConversionRate, loc.settlement.shadow);
                            loc.settlement.shadow -= shadowBurn;

                            if (loc.settlement.shadow < 0.0)
                            {
                                loc.settlement.shadow = 0.0;
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

            foreach (Hex[] column in map.grid[0])
            {
                foreach (Hex hex in column)
                {
                    if (hex == null || !TemperatureMap.TryGetValue(hex, out TemperatureModifier modifier))
                    {
                        continue;
                    }

                    IncrementHexTemperatureModifier(hex, modifier);
                }
            }
        }

        public void IncrementHexTemperatureModifier(Hex hex, TemperatureModifier modifier, bool toMax = false)
        {
            float temperatureDelta = 0f;
            if (modifier.Global < GlobalThermalLimit)
            {
                if (toMax)
                {
                    modifier.Global = GlobalThermalLimit;
                    temperatureDelta += modifier.Global;
                }
                else
                {
                    float delta = (float)Math.Min(TemperatureIncrementRate, GlobalThermalLimit - modifier.Global);
                    modifier.Global += delta;
                    temperatureDelta += delta;
                }
            }

            if (modifier.Distance <= OuterRadius)
            {
                
                if (toMax)
                {
                    modifier.Outer = GetOuterThermalLimit(modifier.Distance);
                    temperatureDelta += modifier.Outer;
                }
                else
                {
                    float delta = (float)Math.Min(TemperatureIncrementRate, GetOuterThermalLimit(modifier.Distance) - modifier.Outer);
                    modifier.Outer += delta;
                    temperatureDelta += delta;
                }
                

                if (modifier.Distance <= InnerRadius)
                {
                    if (toMax)
                    {
                        modifier.Inner = InnerThermalLimit;
                        temperatureDelta += modifier.Inner;
                    }
                    else
                    {
                        float delta = (float)Math.Min(TemperatureIncrementRate, InnerThermalLimit - modifier.Inner);
                        modifier.Inner += delta;
                        temperatureDelta += delta;
                    }
                }
            }

            if (temperatureDelta > 0f)
            {
                map.tempMap[hex.x][hex.y] += temperatureDelta;

                if (map.tempMap[hex.x][hex.y] >= VolanicTemperatureThreshold)
                {
                    hex.volcanicDamage = 10;
                }

                if (map.tempMap[hex.x][hex.y] >= LavaTemperatureThreshold)
                {
                    ConvertHexToLava(hex, modifier);
                }
            }
        }

        public void ConvertHexToLava(Hex hex, TemperatureModifier modifier)
        {
            if (hex.location != null)
            {
                if (!MeltedLocations.Contains(hex.location))
                {
                    MeltedLocations.Add(hex.location);
                    if (hex.location.settlement != null)
                    {
                        hex.location.settlement.fallIntoRuin("Subsumed by lava", this);
                    }
                    else
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
                }

                hex.location.isOcean = false;
                hex.location.isCoastal = false;
            }
            hex.terrain = Hex.terrainType.VOLCANO;

            if (modifier.Total >= LavaTemperatureThreshold)
            {
                Hex subterraneanHex = map.grid[1][hex.x][hex.y];
                if (subterraneanHex != null)
                {
                    if (subterraneanHex.location != null)
                    {
                        if (!MeltedLocations.Contains(subterraneanHex.location))
                        {
                            MeltedLocations.Add(subterraneanHex.location);
                            if (subterraneanHex.location.settlement != null)
                            {
                                subterraneanHex.location.settlement.fallIntoRuin("Melted by Celestium", this);
                            }
                            else
                            {
                                List<Property> ruinedProperties = new List<Property>();
                                foreach (Property property in subterraneanHex.location.properties)
                                {
                                    if (property.removedOnRuin())
                                    {
                                        ruinedProperties.Add(property);
                                    }
                                }

                                foreach (Property property in ruinedProperties)
                                {
                                    subterraneanHex.location.properties.Remove(property);
                                }
                            }
                        }

                        MeltedLocations.Add(subterraneanHex.location);

                        if (subterraneanHex.location.isOcean)
                        {
                            subterraneanHex.location.isOcean = false;
                            subterraneanHex.location.isCoastal = false;
                        }
                    }
                    subterraneanHex.terrain = Hex.terrainType.VOLCANO;
                }
            }
        }

        public float GetOuterThermalLimit(double distance)
        {
            return (float)Math.Min(OuterThermalLimit + (OuterRadius * 0.1) - (distance * 0.1), 5f);
        }

        public override string getAwakenMessage()
        {
            return "A new Star is born! It's radiance shines out over the world, buring everything it touches, including yourself. Your influence is ripped from you, combusted like so much kindling before a blaze... A blaze of unimagined preportions.\n\nYou have been Betrayed! Celestium, the living star, a being of immesurable destructive power, is born, and before it, you are burned away as little more than a whisp of your own shadow...\n\nYou are now playing as Celestium, a newborn star. Celestium is a ravenous consumer of power that can move the heavens themselves in its quest for fuel.";
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
            if (((InnerRadius + OuterRadius) & 1) == 0)
            {
                OuterRadius++;
            }
            else
            {
                OuterRadius += 2;
                InnerRadius++;
            }
        }
    }
}
