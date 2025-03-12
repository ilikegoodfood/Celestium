using Assets.Code;
using CommunityLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Celestium
{
    public class P_Observatory_Moonfall : PowerDelayed
    {
        public int Radius = 5;

        public int ArmyDamage = 25;

        public int ArmyDamageRand = 10;

        public int AgentDamage = 7;

        public int AgentDamageRand = 2;

        public int SettlementDamage = 25;

        public int SettlementDamageRand = 10;

        public double SettlementDevestation = 100.0;

        public double SettlementDevestationRand = 50.0;

        public Sub_NaturalWonder_CelestialObservatory_Lunar LunarObservatory;

        public P_Observatory_Moonfall(Map map, Sub_NaturalWonder_CelestialObservatory_Lunar lunarOservatory)
            : base(map)
        {
            LunarObservatory = lunarOservatory;
        }

        public override string getName()
        {
            return "Prophecy Moonfall";
        }

        public override string getDesc()
        {
            return $"Predicts bombardment of a location and the surrounding area (within {Radius} hexes) by a cloud of small asteroids.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("ILGF_Celestium.Fore_Moonfall.png");
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

            int delay = 2 + Eleven.random.Next(4) + Eleven.random.Next(4);
            TargetDelays_Locations.Add(new Pair<Location, int>(loc, delay));

            LunarObservatory.PowerUsed = true;

            map.addUnifiedMessage(loc, LunarObservatory?.settlement.location, "Moonfall Prophecied", $"Moonfall has been prophecied to strike {loc.getName()} in {delay} turns, devestating all those it strikes. Stand clear!", "Moonfall Prophecied");
        }

        public override void CastDelayed(Location location)
        {
            Hex[][] surfaceGrid = map.grid[0];
            List<Unit> killedUnits = new List<Unit>();
            for (int x = location.hex.x - Radius - 1; x <= location.hex.x + Radius + 1; x++)
            {
                for (int y = location.hex.y - Radius - 1; y <= location.hex.y + Radius + 1; y++)
                {
                    if (Radius * Radius < ((location.hex.x - x) * (location.hex.x - x)) + ((location.hex.y - y) * (location.hex.y - y)))
                    {
                        continue;
                    }

                    Hex hex = surfaceGrid[x][y];
                    if (hex == null)
                    {
                        continue;
                    }

                    hex.volcanicDamage += map.param.mg_volcanicBaseEffect + Eleven.random.Next(map.param.mg_volcanicBaseRand);

                    if (hex.location == null)
                    {
                        continue;
                    }

                    killedUnits.Clear();
                    foreach (Unit unit in hex.location.units)
                    {
                        if (unit == map.awarenessManager.chosenOne)
                        {
                            continue;
                        }

                        if (unit is UA ua)
                        {
                            ua.hp -= AgentDamage + Eleven.random.Next(AgentDamageRand) - ua.defence;
                        }
                        else if (unit is UM)
                        {
                            unit.hp -= ArmyDamage + Eleven.random.Next(ArmyDamageRand);
                        }
                        
                        if (unit.hp <= 0)
                        {
                            killedUnits.Add(unit);
                        }
                    }

                    foreach (Unit unit in killedUnits)
                    {
                        unit.die(map, "killed by Moonfall");
                    }

                    if (hex.location.settlement == null)
                    {
                        continue;
                    }

                    hex.location.settlement.defences -= SettlementDamage + Eleven.random.Next(SettlementDamageRand);
                    if (hex.location.settlement.defences <= 0)
                    {
                        hex.location.settlement.defences = 0;
                        hex.location.settlement.fallIntoRuin("Razed by Moonfall");

                        if (hex.location.settlement is SettlementHuman)
                        {
                            Pr_Devastation devestation = (Pr_Devastation)hex.location.properties.FirstOrDefault(Pr => Pr is Pr_Devastation);
                            double devestationDelta = Eleven.random.NextDouble() * SettlementDevestationRand;
                            if (devestation != null)
                            {
                                devestation.charge += SettlementDevestation + devestationDelta;
                            }
                            else
                            {
                                devestation = new Pr_Devastation(hex.location);
                                devestation.charge = SettlementDevestation + devestationDelta;
                                hex.location.properties.Add(devestation);
                            }
                        }
                    }
                }
            }
        }
    }
}
