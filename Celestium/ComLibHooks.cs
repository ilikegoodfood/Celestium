using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celestium
{
    public class ComLibHooks : Hooks
    {
        public ComLibHooks(Map map)
            : base(map)
        {

        }

        public override List<WonderData> onMapGen_PlaceWonders()
        {
            return new List<WonderData> { new WonderData(typeof(Sub_NaturalWonder_CelestialObservatory), ModCore.opt_SpawnPriority, false) };
        }

        public override void onMapGen_PlaceWonders(Type t, out bool failedToPlaceWonder)
        {
            failedToPlaceWonder = false;

            if (t == typeof(Sub_NaturalWonder_CelestialObservatory))
            {
                FindPlacesForLunarObservatory(map, out List<Location> primaryLocations);

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

                FindPlacesForSolarObservatory(map, out primaryLocations, out List<Location> secondaryLocations, out List<Location> ternaryLocations, out List<Location> quaternaryLocations);

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

            Settlement set = location.settlement;

            if (set != null)
            {
                set.fallIntoRuin("Replaced by Solar Observatory");
                settlement.subs.AddRange(set.subs);
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
                if (location.hex.z != 0 || location.isOcean)
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
                if (location.hex.z != 0 || !location.isCoastal || location.hex.getHabilitability() < map.param.mapGen_minHabitabilityForHumans)
                {
                    continue;
                }

                if (location.settlement == null || location.settlement is Set_CityRuins || location.settlement is Set_MinorHuman || location.settlement is Set_DwarvenOutpost)
                {
                    primaryLocations.Add(location);
                }
            }
        }

        public override void onGetTradeRouteEndpoints(Map map, List<Location> endpoints)
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

        public override void onSettlementFallIntoRuin_EndOfProcess(Settlement set, string v, object killer = null)
        {
            if (!ModCore.Instance.Observatories)
            {
                return;
            }

            Sub_NaturalWonder_CelestialObservatory_Lunar lunarObservatory = (Sub_NaturalWonder_CelestialObservatory_Lunar)set.subs.FirstOrDefault(sub => sub is Sub_NaturalWonder_CelestialObservatory_Lunar);
            if (lunarObservatory != null)
            {
                lunarObservatory.Respawn();
            }
        }
    }
}
