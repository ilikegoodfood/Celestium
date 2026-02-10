using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celestium
{
    public class ComLibHooks
    {
        private Map _map;

        public ComLibHooks(Map map)
        {
            _map = map;

            HooksDelegateRegistry registry = CommunityLib.ModCore.Get().HookRegistry;
            registry.RegisterHook_onMapGen_PlaceWonders_1(onMapGen_PlaceWonders);
            registry.RegisterHook_onMapGen_PlaceWonders_2(onMapGen_PlaceWonders);
            registry.RegisterHook_onGetTradeRouteEndpoints(onGetTradeRouteEndpoints);
            registry.RegisterHook_onSettlementCalculatesShadowGain(onSettlementComputesShadowGain);
            registry.RegisterHook_onSettlementFallIntoRuin_EndOfProcess(onSettlementFallIntoRuin_EndOfProcess);
            registry.RegisterHook_onMoveTaken(onMoveTaken);
            registry.RegisterHook_onPopulatingPathfindingDelegates(onPopulatingPathfindingDelegates);
            registry.RegisterHook_onPopulatingTradeRoutePathfindingDelegates(onPopulatingTradeRoutePathfindingDelegates);
        }

        public List<WonderData> onMapGen_PlaceWonders()
        {
            return new List<WonderData> { new WonderData(typeof(Sub_NaturalWonder_CelestialObservatory), ModCore.opt_SpawnPriority, false) };
        }

        public void onMapGen_PlaceWonders(Type t, out bool failedToPlaceWonder)
        {
            failedToPlaceWonder = false;

            if (t == typeof(Sub_NaturalWonder_CelestialObservatory))
            {
                if (ModCore.Instance.Observatories)
                {
                    failedToPlaceWonder = true;
                    return;
                }

                FindPlacesForLunarObservatory(_map, out List<Location> primaryLocations);

                Location targetLunarLocation = null;
                if (primaryLocations.Count > 0)
                {
                    if (primaryLocations.Count > 1)
                    {
                        targetLunarLocation = primaryLocations[Eleven.random.Next(primaryLocations.Count)];
                    }
                    else
                    {
                        targetLunarLocation = primaryLocations[0];
                    }
                }

                if (targetLunarLocation == null)
                {
                    failedToPlaceWonder = true;
                    return;
                }

                FindPlacesForSolarObservatory(_map, out primaryLocations, out List<Location> secondaryLocations, out List<Location> ternaryLocations, out List<Location> quaternaryLocations);
                primaryLocations.Remove(targetLunarLocation);
                secondaryLocations.Remove(targetLunarLocation);
                ternaryLocations.Remove(targetLunarLocation);
                quaternaryLocations.Remove(targetLunarLocation);

                Location targetSolarLocation = null;
                if (primaryLocations.Count > 0)
                {
                    if (primaryLocations.Count > 1)
                    {
                        targetSolarLocation = primaryLocations[0]; 
                    }
                    else
                    {
                        targetSolarLocation = primaryLocations[Eleven.random.Next(primaryLocations.Count)];
                    }
                }
                else if (secondaryLocations.Count > 0)
                {
                    if (secondaryLocations.Count > 1)
                    {
                        targetSolarLocation = secondaryLocations[0];
                    }
                    else
                    {
                        targetSolarLocation = secondaryLocations[Eleven.random.Next(secondaryLocations.Count)];
                    }
                }
                else if (secondaryLocations.Count > 0)
                {
                    if (secondaryLocations.Count > 1)
                    {
                        targetSolarLocation = secondaryLocations[0];
                    }
                    else
                    {
                        targetSolarLocation = secondaryLocations[Eleven.random.Next(secondaryLocations.Count)];
                    }
                }
                else if (ternaryLocations.Count > 0)
                {
                    if (secondaryLocations.Count > 1)
                    {
                        targetSolarLocation = ternaryLocations[0];
                    }
                    else
                    {
                        targetSolarLocation = ternaryLocations[Eleven.random.Next(ternaryLocations.Count)];
                    }
                }
                else if (quaternaryLocations.Count > 0)
                {
                    if (secondaryLocations.Count > 1)
                    {
                        targetSolarLocation = quaternaryLocations[0];
                    }
                    else
                    {
                        targetSolarLocation = quaternaryLocations[Eleven.random.Next(quaternaryLocations.Count)];
                    }
                }

                if (targetSolarLocation == null)
                {
                    failedToPlaceWonder = true;
                    return;
                }

                Sub_NaturalWonder_CelestialObservatory_Lunar lunarObsratory = PlaceLunarObservatory(targetLunarLocation);
                Sub_NaturalWonder_CelestialObservatory_Solar solarObservatory = PlaceSolarObservatory(targetSolarLocation, lunarObsratory);

                ModCore.Instance.LunarObservatories.Add(lunarObsratory);
                ModCore.Instance.SolarObservatories.Add(solarObservatory);
                ModCore.Instance.Observatories = true;
            }
        }

        public Sub_NaturalWonder_CelestialObservatory_Lunar PlaceLunarObservatory(Location location)
        {
            Set_MinorOther settlement = new Set_MinorOther_Observatory(location);
            settlement.subs.Clear();
            Sub_NaturalWonder_CelestialObservatory_Lunar lunarObservatory = new Sub_NaturalWonder_CelestialObservatory_Lunar(settlement);
            settlement.subs.Add(lunarObservatory);

            Settlement set = location.settlement;

            if (set != null)
            {
                set.fallIntoRuin("Replaced by Lunar Observatory");
                settlement.subs.AddRange(set.subs);
            }

            location.settlement = settlement;
            location.soc = null;

            return lunarObservatory;
        }

        public Sub_NaturalWonder_CelestialObservatory_Solar PlaceSolarObservatory(Location location, Sub_NaturalWonder_CelestialObservatory_Lunar lunarObservatory)
        {
            Set_MinorOther settlement = new Set_MinorOther_Observatory(location);
            settlement.subs.Clear();
            Sub_NaturalWonder_CelestialObservatory_Solar solarObservatory = new Sub_NaturalWonder_CelestialObservatory_Solar(settlement, lunarObservatory);
            settlement.subs.Add(solarObservatory);

            if (location.settlement != null)
            {
                location.settlement.fallIntoRuin("Replaced by Solar Observatory");
                settlement.subs.AddRange(location.settlement.subs);
            }

            location.settlement = settlement;
            location.soc = null;

            return solarObservatory;
        }

        public void FindPlacesForSolarObservatory(Map map, out List<Location> primaryLocations, out List<Location> secondaryLocations, out List<Location> ternaryLocations, out List<Location> quaternaryLocations)
        {
            primaryLocations = new List<Location>();
            secondaryLocations = new List<Location>();
            ternaryLocations = new List<Location>();
            quaternaryLocations = new List<Location>();

            foreach (Location location in map.locations)
            {
                if (location.isOcean)
                {
                    continue;
                }

                bool isMountain;
                if (location.hex.isMountain)
                {
                    isMountain = true;
                }
                else if (location.hex.isHills)
                {
                    isMountain = false;
                }
                else
                {
                    continue;
                }

                if (location.soc != null)
                {
                    if (location.settlement == null || location.settlement is Set_CityRuins)
                    {
                        if (isMountain)
                        {
                            primaryLocations.Add(location);
                        }
                        else
                        {
                            ternaryLocations.Add(location);
                        }
                    }
                    else if (location.settlement is SettlementHuman && !ModCore.CommunityLib.checkIsWonder(location) && !ModCore.CommunityLib.checkIsElderTomb(location))
                    {
                        if (isMountain)
                        {
                            secondaryLocations.Add(location);
                        }
                        else
                        {
                            quaternaryLocations.Add(location);
                        }
                    }
                }
                else
                {
                    if (location.settlement == null || location.settlement is Set_CityRuins)
                    {
                        if (isMountain)
                        {
                            primaryLocations.Add(location);
                        }
                        else
                        {
                            ternaryLocations.Add(location);
                        }
                    }
                    else if (location.settlement is SettlementHuman && !ModCore.CommunityLib.checkIsWonder(location) && !ModCore.CommunityLib.checkIsElderTomb(location))
                    {
                        if (isMountain)
                        {
                            secondaryLocations.Add(location);
                        }
                        else
                        {
                            quaternaryLocations.Add(location);
                        }
                    }
                }
            }
        }

        public void FindPlacesForLunarObservatory(Map map, out List<Location> primaryLocations)
        {
            primaryLocations = new List<Location>();
            foreach (Location location in map.locations)
            {
                if (!location.isCoastal || location.hex.getHabilitability() < map.param.mapGen_minHabitabilityForHumans)
                {
                    continue;
                }

                if (location.settlement == null || location.settlement is Set_CityRuins || location.settlement is Set_MinorHuman)
                {
                    primaryLocations.Add(location);
                }
            }
        }

        public void onGetTradeRouteEndpoints(Map map, List<Location> endpoints)
        {
            if (!ModCore.Instance.Observatories)
            {
                return;
            }

            foreach (Sub_NaturalWonder_CelestialObservatory_Lunar lunarObservatory in ModCore.Instance.LunarObservatories)
            {
                if (lunarObservatory.settlement is SettlementHuman && !endpoints.Contains(lunarObservatory.settlement.location))
                {
                    endpoints.Add(lunarObservatory.settlement.location);
                }
            }
        }

        public double onSettlementComputesShadowGain(Settlement set, List<ReasonMsg> msgs, double shadowGain)
        {
            if (_map.overmind.god is God_Celestium celestium)
            {
                if (celestium.Defeated)
                {
                    return shadowGain;
                }

                double delta = Math.Min(celestium.ShadowConversionRate, set.shadow + shadowGain);
                if (delta > 0.0)
                {
                    if (msgs == null)
                    {
                        celestium.GlobalThermalLimit += ((float)delta) * celestium.ShadowToGlobalThermalConversion;
                    }
                    else
                    {
                        msgs.Add(new ReasonMsg("Burned by Celestium's light", -delta));
                    }
                    
                    shadowGain -= delta;
                }
            }

            if (set.shadow + shadowGain <= 0.0)
            {
                return shadowGain;
            }

            double ShadowBurnRate = 0.02;
            if (ModCore.Instance.SettlementsEffectedBySolarObservatories.TryGetValue(set, out List<Sub_NaturalWonder_CelestialObservatory_Solar> effectingObservatories))
            {
                double delta = 0.0;
                foreach (Sub_NaturalWonder_CelestialObservatory_Solar effectingObservatory in effectingObservatories)
                {
                    if(effectingObservatory.settlement == set || effectingObservatory.LunarObservatory.settlement == set)
                    {
                        if (effectingObservatory.SolarObservationDuration > 0)
                        {
                            delta += ShadowBurnRate * 2;
                        }
                        else
                        {
                            delta += ShadowBurnRate;
                        }
                    }
                    else
                    {
                        delta += ShadowBurnRate;
                    }
                }

                delta = Math.Min(delta, set.shadow + shadowGain);
                if (delta > 0.0)
                {
                    if (msgs == null)
                    {
                        foreach (Sub_NaturalWonder_CelestialObservatory_Solar effectingObservatory in effectingObservatories)
                        {
                            effectingObservatory.GainShadow(delta / effectingObservatories.Count);
                        }
                    }
                    else
                    {
                        msgs.Add(new ReasonMsg("Solar Observatory", -delta));
                    }

                    shadowGain -= delta;
                }
            }

            return shadowGain;
        }

        public void onSettlementFallIntoRuin_EndOfProcess(Settlement set, string v, object killer = null)
        {
            if (!ModCore.Instance.Observatories)
            {
                return;
            }

            Sub_NaturalWonder_CelestialObservatory_Lunar lunarObservatory = (Sub_NaturalWonder_CelestialObservatory_Lunar)set.subs.FirstOrDefault(sub => sub is Sub_NaturalWonder_CelestialObservatory_Lunar);
            if (lunarObservatory != null)
            {
                lunarObservatory.CheckSettlementType();
            }
        }

        public void onUnitDeath_StartOfProcess(Unit u, string v, Person killer)
        {
            if (u is UA ua && ua.isCommandable() && ua.person.traits.Any(t => t is T_BurningSoul))
            {

            }
        }

        public void onMoveTaken(Unit u, Location locA, Location locB)
        {
            if (!(u.map.overmind.god is God_Celestium celestium) || u == null || u == u.map.awarenessManager.chosenOne || u.isCommandable())
            {
                return;
            }

            if (celestium.TemperatureMap.TryGetValue(_map.grid[locB.hex.x][locB.hex.y], out God_Celestium.TemperatureModifier modifier))
            {
                if (modifier.IsLava)
                {
                    u.hp -= (int)Math.Ceiling(0.05 * u.maxHp);

                    if (u.hp <= 0)
                    {
                        u.die(locB.map, "Burned to death travelling too close to Celestium.");
                    }
                }
            }
        }

        public void onPopulatingPathfindingDelegates(Location locA, Location locB, Unit u, List<Func<Location[], Location, Unit, double>> pathfindingDelegates, List<Func<Location[], Location, Unit, List<Location>>> getNeighbourDelegates)
        {
            if (locA.map.overmind.god is God_Celestium && u != null && u != u.map.awarenessManager.chosenOne && !u.isCommandable())
            {
                pathfindingDelegates.Add(delegate_MAGMABURNS);
            }
        }

        public double delegate_MAGMABURNS(Location[] currentPath, Location location, Unit u)
        {
            if (!(location.hex.map.overmind.god is God_Celestium celestium))
            {
                return 0.0;
            }

            if (u == null)
            {
                return 10.0;
            }

            int damageInstanceCount = 0;
            // check if next location is lava
            if (celestium.TemperatureMap.TryGetValue(location.hex, out God_Celestium.TemperatureModifier modifier))
            {
                if (modifier.IsLava)
                {
                    damageInstanceCount++;
                }
            }
            else if (location.map.tempMap[location.hex.x][location.hex.y] >= celestium.LavaTemperatureThreshold)
            {
                damageInstanceCount++;
            }

            // Count lava locations already passed through
            foreach (Location loc in currentPath)
            {
                if (celestium.TemperatureMap.TryGetValue(loc.hex, out modifier))
                {
                    if (modifier.IsLava)
                    {
                        damageInstanceCount++;
                    }
                }
                else if (loc.map.tempMap[loc.hex.x][loc.hex.y] >= celestium.LavaTemperatureThreshold)
                {
                    damageInstanceCount++;
                }
            }

            damageInstanceCount = (int)Math.Ceiling((double)damageInstanceCount / (double)u.getMaxMoves());
            if ((damageInstanceCount + 1) * Math.Ceiling(0.05 * u.maxHp) >= u.hp)
            {
                return 10000.0;
            }

            return 10.0;
        }

        public void onPopulatingTradeRoutePathfindingDelegates(Location start, List<Func<Location[], Location, double>> pathfindingDelegates, List<Func<Location[], Location, bool>> destinationValidityDelegates)
        {
            if (start.map.overmind.god is God_Celestium)
            {
                pathfindingDelegates.Add(delegate_TRADE_MAGMABURNS);
            }
        }

        public static double delegate_TRADE_MAGMABURNS(Location[] currentPath, Location location)
        {
            if (!(location.map.overmind.god is God_Celestium celestium))
            {
                return 0.0;
            }

            if (celestium.TemperatureMap.TryGetValue(location.hex, out God_Celestium.TemperatureModifier modifier))
            {
                if (modifier.IsLava)
                {
                    return 10000.0;
                }
            }
            else if (location.map.tempMap[location.hex.x][location.hex.y] >= celestium.LavaTemperatureThreshold)
            {
                return 10000.0;
            }

            return 0.0;
        }
    }
}
