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
    public class P_Celestium_Collapse : Power
    {
        public int Radius = 5;

        public int ArmyDamage = 30;

        public int ArmyDamageRand = 20;

        public int AgentDamage = 6;

        public int AgentDamageRand = 4;

        public int SettlementDamage = 35;

        public int SettlementDamageRand = 20;

        public double SettlementDevestation = 150.0;

        public double SettlementDevestationRand = 75.0;

        public int disruptDuration = 1;

        public P_Celestium_Collapse(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Underground Collapse";
        }

        public override string getDesc()
        {
            return $"Cause a devastating cave in in the underground, damaging and potentially killing agents, and devastating and potentially razing settlements, in a {Radius} hex radius.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_P_Collapse.jpg");
        }

        public override int getCost()
        {
            return 5;
        }

        public override bool validTarget(Location loc)
        {
            return loc.hex.z == 1 && map.overmind.god is God_Celestium celestium && !celestium.Defeated;
        }

        public override bool validTarget(Unit unit)
        {
            return unit.location.hex.z == 1 && map.overmind.god is God_Celestium celestium && !celestium.Defeated;
        }

        public override void castCommon(Location loc)
        {
            base.castCommon(loc);

            List<Unit> killedUnits = new List<Unit>();
            List<Hex> hexes = HexGridUtils.HexesWithinRadius(map, loc.hex, Radius, 1, out _);
            foreach (Hex hex in hexes)
            {
                if (hex.location == null)
                {
                    continue;
                }

                killedUnits.Clear();
                foreach (Unit unit in hex.location.units)
                {
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
                    else
                    {
                        unit.task = new Task_Disrupted(disruptDuration);
                    }
                }

                foreach (Unit unit in killedUnits)
                {
                    unit.die(map, "Killed by cave in");
                }

                if (hex.location.settlement == null)
                {
                    continue;
                }

                hex.location.settlement.defences -= SettlementDamage + Eleven.random.Next(SettlementDamageRand);
                if (hex.location.settlement.defences <= 0)
                {
                    hex.location.settlement.defences = 0;
                    if (loc.settlement is SettlementHuman humanSettlement && humanSettlement.supportedMilitary != null)
                    {
                        humanSettlement.supportedMilitary.die(map, "Home location destroyed by cave in");
                    }
                    hex.location.settlement.fallIntoRuin("Destroyed by cave in");

                    if (hex.location.settlement is SettlementHuman humanSettlement2)
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

                        if (humanSettlement2.ruler != null)
                        {
                            humanSettlement2.actionProgress = 0;
                            humanSettlement2.actionUnderway = null;
                        }
                    }
                }
                else
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

                    if (hex.location.settlement is SettlementHuman humanSettlement && humanSettlement.ruler != null)
                    {
                        humanSettlement.actionProgress = 0;
                        humanSettlement.actionUnderway = null;
                    }
                }
            }
        }
    }
}
