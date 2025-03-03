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
            return new List<WonderData> { new WonderData(typeof(Sub_NaturalWonder_CelestialTemple), ModCore.opt_SpawnPriority) };
        }

        public override void onMapGen_PlaceWonders(Type t, out bool failedToPlaceWonder)
        {
            failedToPlaceWonder = false;

            if (t == typeof(Sub_NaturalWonder_CelestialTemple))
            {
                List<Location> mountainLocations = new List<Location>();
                List<Location> hillLocations = new List<Location>();
                List<Location> occupiedMountainLocations = new List<Location>();

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
                                mountainLocations.Add(location);
                            }
                            else
                            {
                                hillLocations.Add(location);
                            }
                        }
                        else if (location.settlement is SettlementHuman && !ModCore.CommunityLib.checkIsWonder(location) && !ModCore.CommunityLib.checkIsElderTomb(location))
                        {
                            if (isMountain)
                            {
                                occupiedMountainLocations.Add(location);
                            }
                        }
                    }
                    else
                    {
                        if (location.settlement == null || location.settlement is Set_CityRuins)
                        {
                            if (isMountain)
                            {
                                mountainLocations.Add(location);
                            }
                            else
                            {
                                hillLocations.Add(location);
                            }
                        }
                        else if (location.settlement is SettlementHuman && !ModCore.CommunityLib.checkIsWonder(location) && !ModCore.CommunityLib.checkIsElderTomb(location))
                        {
                            if (isMountain)
                            {
                                occupiedMountainLocations.Add(location);
                            }
                        }
                    }
                }

                Location targetLocation = null;
                if (mountainLocations.Count > 0)
                {
                    if (mountainLocations.Count > 1)
                    {
                        targetLocation = mountainLocations[0]; 
                    }
                    else
                    {
                        targetLocation = mountainLocations[Eleven.random.Next(mountainLocations.Count)];
                    }
                }
                else if (hillLocations.Count > 0)
                {
                    if (hillLocations.Count > 1)
                    {
                        targetLocation = hillLocations[0];
                    }
                    else
                    {
                        targetLocation = hillLocations[Eleven.random.Next(hillLocations.Count)];
                    }
                }
                else if (hillLocations.Count > 0)
                {
                    if (hillLocations.Count > 1)
                    {
                        targetLocation = hillLocations[0];
                    }
                    else
                    {
                        targetLocation = hillLocations[Eleven.random.Next(hillLocations.Count)];
                    }
                }
                else if (occupiedMountainLocations.Count > 0)
                {
                    if (hillLocations.Count > 1)
                    {
                        targetLocation = occupiedMountainLocations[0];
                    }
                    else
                    {
                        targetLocation = occupiedMountainLocations[Eleven.random.Next(hillLocations.Count)];
                    }
                }

                if (targetLocation == null)
                {
                    failedToPlaceWonder = true;
                    return;
                }

                Set_MinorOther settlement = new Set_MinorOther(targetLocation);
                Sub_NaturalWonder_CelestialTemple celestialTemple = new Sub_NaturalWonder_CelestialTemple(settlement);
                settlement.subs.Add(celestialTemple);

                Settlement set = targetLocation.settlement;

                if (set != null)
                {
                    set.fallIntoRuin("Replaced with Celestial Temple");
                    settlement.subs.AddRange(set.subs);
                }

                targetLocation.settlement = settlement;
            }
        }
    }
}
