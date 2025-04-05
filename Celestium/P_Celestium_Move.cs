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
    public class P_Celestium_Move : Power
    {
        public P_Celestium_Move(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Move";
        }

        public override string getDesc()
        {
            return $"In a brilliant flash of light and heat, a great trembling of the world, a burning of the sky...  Celestium Moves! Move Celestium to the target location on the surface.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_P_Grow.jpg");
        }

        public override int getCost()
        {
            return 15;
        }

        public override bool validTarget(Location loc)
        {
            return !CommunityLib.ModCore.Get().checkIsNaturalWonder(loc) && map.overmind.god is God_Celestium celestium && !celestium.Defeated;
        }

        public override bool validTarget(Unit unit)
        {
            return !CommunityLib.ModCore.Get().checkIsNaturalWonder(unit.location) && map.overmind.god is God_Celestium celestium && !celestium.Defeated;
        }

        public override void castCommon(Location loc)
        {
            if (!(map.overmind.god is God_Celestium celestium))
            {
                return;
            }

            base.castCommon(loc);

            Hex destinationHex = loc.hex;
            Settlement destinationSettlement = loc.settlement;
            if (destinationSettlement != null)
            {
                destinationSettlement.fallIntoRuin("Vaporized by Celestium");
            }
            celestium.GlobalThermalLimit += 0.1f;

            List<Hex> hexes = HexGridUtils.HexesWithinLine(map, celestium.Settlement.location.hex, loc.hex);
            List<Unit> killedUnits = new List<Unit>();
            bool tradeNetworkUpdateNeeded = false;
            foreach (Hex hex in hexes)
            {
                if (celestium.TemperatureMap.TryGetValue(hex, out God_Celestium.TemperatureModifier modifier))
                {
                    float deltaTemp = 0;
                    if (modifier.Inner < celestium.InnerThermalLimit)
                    {
                        deltaTemp += celestium.InnerThermalLimit - modifier.Inner;
                        modifier.Inner = celestium.InnerThermalLimit;
                    }
                    if (modifier.Outer < celestium.OuterThermalLimit)
                    {
                        deltaTemp += celestium.OuterThermalLimit - modifier.Outer;
                        modifier.Outer = celestium.OuterThermalLimit;
                    }
                    if (modifier.Global < celestium.GlobalThermalLimit)
                    {
                        deltaTemp += celestium.GlobalThermalLimit - modifier.Global;
                        modifier.Global = celestium.GlobalThermalLimit;
                    }
                    
                    map.tempMap[hex.x][hex.y] += deltaTemp;

                    if(celestium.ManageLavaHex(hex, modifier, modifier.Total))
                    {
                        tradeNetworkUpdateNeeded = true;
                    }
                }

                if (hex.location != null)
                {
                    killedUnits.Clear();
                    foreach(Unit unit in hex.location.units)
                    {
                        unit.hp -= (int)Math.Ceiling(0.1 * unit.maxHp);

                        if (unit.hp <= 0)
                        {
                            killedUnits.Add(unit);
                        }
                    }

                    foreach (Unit unit in killedUnits)
                    {
                        unit.die(map, "Vaporized by Celestium.");
                    }
                }
            }

            celestium.Settlement.location.settlement = null;
            celestium.Settlement.location = loc;
            loc.settlement = celestium.Settlement;
            celestium.UpdateHexDistances();

            foreach (Hex hex in celestium.InnerHexCache)
            {
                if (!celestium.TemperatureMap.TryGetValue(hex, out God_Celestium.TemperatureModifier modifier))
                {
                    continue;
                }
                
                if (celestium.IncrementHexTemperatureModifier(hex, modifier, false, true, true, true))
                {
                    tradeNetworkUpdateNeeded = true;
                }
            }

            map.assignTerrainFromClimate();

            if (tradeNetworkUpdateNeeded)
            {
                map.tradeManager.cached.Clear();
                map.tradeManager.checkTradeNetwork();
            }
        }
    }
}
