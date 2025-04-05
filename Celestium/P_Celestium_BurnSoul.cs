using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class P_Celestium_BurnSoul : Power
    {
        public int Duration = 5;

        public P_Celestium_BurnSoul(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Burn Soul";
        }

        public override string getDesc()
        {
            return $"Grants the target agent a massive boost to all stats (+2), at the cost of health per turn, for {Duration} turns. Player agents that die while under the effect of BurningSoul become Embers.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_P_Flare.jpg");
        }

        public override int getCost()
        {
            return 1;
        }

        public override bool validTarget(Location loc)
        {
            return false;
        }

        public override bool validTarget(Unit unit)
        {
            return map.overmind.god is God_Celestium celestium && !celestium.Defeated && unit is UA ua && (ua.isCommandable() || (ua.person != null && ua.person.hasSoul)) && !ua.person.traits.Any(t => t is T_BurningSoul);
        }

        public override void cast(Unit unit)
        {
            if (!(unit is UA ua) || unit.person == null || !unit.person.hasSoul || ua.person.traits.Any(t => t is T_BurningSoul))
            {
                return;
            }

            base.cast(unit);

            ua.person.receiveTrait(new T_BurningSoul());
        }
    }
}
