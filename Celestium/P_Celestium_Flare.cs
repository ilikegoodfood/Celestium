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
    public class P_Celestium_Flare : Power
    {
        public int Duration = 3;

        public int Radius = 2;

        public P_Celestium_Flare(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Flare";
        }

        public override string getDesc()
        {
            return $"Briefly disrupt all heroes within a small radius ({Radius} hexes).";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_P_Flare.jpg");
        }

        public override int getCost()
        {
            return 2;
        }

        public override bool validTarget(Location loc)
        {
            bool agent = false;
            List<Hex> hexes = HexGridUtils.HexesWithinRadius(map, loc.hex, Radius, loc.hex.z, out _);
            foreach (Hex hex in hexes)
            {
                if (hex.location == null)
                {
                    continue;
                }

                foreach (Unit u in hex.location.units)
                {
                    if (!(u is UA) || u == map.awarenessManager.chosenOne || u.isCommandable())
                    {
                        continue;
                    }

                    agent = true;
                }
            }

            return agent && loc.hex.z != 1 && map.overmind.god is God_Celestium celestium && !celestium.Defeated;
        }

        public override bool validTarget(Unit unit)
        {
            bool agent = false;
            List<Hex> hexes = HexGridUtils.HexesWithinRadius(map, unit.location.hex, Radius, unit.location.hex.z, out _);
            foreach (Hex hex in hexes)
            {
                if (hex.location == null)
                {
                    continue;
                }

                foreach (Unit u in hex.location.units)
                {
                    if (!(u is UA) || u == map.awarenessManager.chosenOne || u.isCommandable())
                    {
                        continue;
                    }

                    agent = true;
                }
            }

            return agent && unit.location.hex.z != 1 && map.overmind.god is God_Celestium celestium && !celestium.Defeated;
        }

        public override void castCommon(Location loc)
        {
            base.castCommon(loc);

            List<Hex> hexes = HexGridUtils.HexesWithinRadius(map, loc.hex, Radius, loc.hex.z, out _);
            foreach (Hex hex in hexes)
            {
                if (hex.location == null)
                {
                    continue;
                }

                foreach (Unit u in hex.location.units)
                {
                    if (!(u is UA) || u == map.awarenessManager.chosenOne || u.isCommandable())
                    {
                        continue;
                    }

                    u.task = new Task_Disrupted(Duration);
                }
            }
        }
    }
}
