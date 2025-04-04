using Assets.Code;
using CommunityLib;
using System.Collections.Generic;
using UnityEngine;

namespace Celestium
{
    public class P_Observatory_Starfall : PowerDelayed
    {
        public int Radius = 2;

        public Sub_NaturalWonder_CelestialObservatory_Solar SolarObservatory;

        public P_Observatory_Starfall(Map map, Sub_NaturalWonder_CelestialObservatory_Solar solarObservatory)
            : base(map)
        {
            SolarObservatory = solarObservatory;
        }
        public override string getName()
        {
            return "Prophecy Starfall";
        }

        public override string getDesc()
        {
            return $"Predicts the derstruction of a location and the surrounding area (within {Radius} hexes) by an asteroid impact.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("ILGF_Celestium.Fore_Starfall.png");
        }

        public override Sprite getIconBack()
        {
            return EventManager.getImg("ILGF_Celestium.Back_Stars.jpg");
        }

        public override bool validTarget(Unit unit)
        {
            return false;
        }

        public override bool validTarget(Location loc)
        {
            return loc.hex.z == 0;
        }

        public override void cast(Location loc)
        {
            base.cast(loc);

            int delay = 2 + Eleven.random.Next(3) + Eleven.random.Next(3);
            TargetDelays_Locations.Add(new Pair<Location, int>(loc, delay));

            SolarObservatory.PowerUsed = true;

            map.addUnifiedMessage(loc, SolarObservatory?.settlement.location, "Starfall Prophecied", $"Starfall has been prophecied to strke {loc.getName()} in {delay} turns, devestating all those it strikes. Stand clear!", "Starfall Prophecied");
        }

        public override void CastDelayed(Location location)
        {
            Hex[][] surfaceGrid = map.grid[0];
            List<Unit> killedUnits = new List<Unit>();

            foreach (Hex hex in HexGridUtils.HexesWithinRadius(map, location.hex, Radius, out _))
            {
                hex.volcanicDamage += map.param.mg_volcanicBaseEffect + Eleven.random.Next(map.param.mg_volcanicBaseRand) + Eleven.random.Next(map.param.mg_volcanicBaseRand);

                if (hex.location == null)
                {
                    continue;
                }

                killedUnits.Clear();
                foreach (Unit unit in hex.location.units)
                {
                    killedUnits.Add(unit);
                }

                foreach (Unit unit in killedUnits)
                {
                    unit.die(map, "Vaporized by Starfall");
                }

                if (hex.location.settlement == null)
                {
                    continue;
                }

                if (hex.location.settlement is SettlementHuman humanSettlement && humanSettlement.supportedMilitary != null)
                {
                    humanSettlement.supportedMilitary.die(map, "Home location vaporized by starfall");
                }
                hex.location.settlement.fallIntoRuin("Vaporized by starfall");
            }

            if (ModCore.opt_EnableGod)
            {
                location.properties.Add(new Pr_StarMetal(location));
            }
        }
    }
}
