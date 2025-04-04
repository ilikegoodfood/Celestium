using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class P_Celestium_FindHotspots : Power
    {
        public int SpawnCount = 2;

        public int HotspotLimit = 4;

        public double weightSubterraneanSpwn = 0.67;

        public P_Celestium_FindHotspots(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Seek Hotspots";
        }

        public override string getDesc()
        {
            return $"Spawns {SpawnCount} geothermal hotspots.";
        }

        public override string getFlavour()
        {
            return "The world contains vast reserves of heat and energy buried beneath the surface. If you could tap into these reserves, you could grow faster, hotter, and stronger.";
        }

        public override string getRestrictionText()
        {
            return $"You cannot seek more hotspots while {HotspotLimit} or more are known.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_Hotspot.jpg");
        }

        public override int getCost()
        {
            return 2;
        }

        public override bool validTarget(Location loc)
        {
            return map.overmind.god is God_Celestium celestium && !celestium.Defeated && celestium.Hotspots.Count < HotspotLimit;
        }

        public override bool validTarget(Unit unit)
        {
            return map.overmind.god is God_Celestium celestium && !celestium.Defeated && celestium.Hotspots.Count < HotspotLimit;
        }

        public override void castCommon(Location loc)
        {
            base.castCommon(loc);

            Pr_Hotspot[] hotspots = new Pr_Hotspot[SpawnCount];
            int hotspotCount = 0;
            for (int i = 0; i < SpawnCount; i++)
            {
                int mapLayer = Eleven.random.NextDouble() < weightSubterraneanSpwn ? 1 : 0;
                int size = Eleven.random.Next(4);

                bool rerollSize = false;
                if (mapLayer == 1)
                {
                    if (size == 0)
                    {
                        rerollSize = true;
                    }
                }
                else if (size == 3)
                {
                    rerollSize = true;
                }

                if (rerollSize)
                {
                    size = Eleven.random.Next(4);
                }

                double charge = 2.0 + Eleven.random.Next(25) + Eleven.random.Next(25);
                charge += 50 * size;
                if (charge < 50.0)
                {
                    charge = Math.Max(30.0 + Eleven.random.Next(11) + Eleven.random.Next(11), charge);
                }
                
                List<Location> targetLocations = new List<Location>();
                foreach (Location location in map.locations)
                {
                    if (location.hex.z != mapLayer || location.isOcean || location.properties.Any(pr => pr is Pr_Hotspot))
                    {
                        continue;
                    }

                    targetLocations.Add(location);
                }

                Location target;
                if (targetLocations.Count == 0)
                {
                    continue;
                }

                if (targetLocations.Count == 1)
                {
                    target = targetLocations[0];
                }
                else
                {
                    target = targetLocations[Eleven.random.Next(targetLocations.Count)];
                }

                hotspotCount++;
                hotspots[i] = new Pr_Hotspot(target);
                hotspots[i].charge = charge;
                hotspots[i].Size = size;
                target.properties.Add(hotspots[i]);
                
                if (loc.map.overmind.god is God_Celestium celestium)
                {
                    celestium.Hotspots.Add(hotspots[i]);
                }
            }

            Pr_Hotspot hotspotA = hotspots.FirstOrDefault();
            Pr_Hotspot hotspotB = hotspots.FirstOrDefault(hs => hs != null && hs != hotspotA);
            if (!loc.map.automatic && loc.map.world.displayMessages)
            {
                loc.map.addUnifiedMessage(hotspotA?.location, hotspotB?.location, "Hotspots Discovered", $"{hotspotCount} geothermal hotspots have been discovered. May they help you burn the world!", "Hotspots Discovered");
            }
        }
    }
}
