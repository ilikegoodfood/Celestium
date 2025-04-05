using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class P_Celestium_Grow : Power
    {
        public P_Celestium_Grow(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Grow";
        }

        public override string getDesc()
        {
            return "A mote of primal light, a glimmer of futures bright. Grow. Consume. Burn. Increases the radius of your influence by 1.5 hexes per cast, and slightly increase the global temperature.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_P_Grow.jpg");
        }

        public override int getCost()
        {
            if (map.overmind.god is God_Celestium celestium && celestium.Victory)
            {
                return 1;
            }

            return 9;
        }

        public override bool validTarget(Location loc)
        {
            return map.overmind.god is God_Celestium celestium && !celestium.Defeated;
        }

        public override bool validTarget(Unit unit)
        {
            return map.overmind.god is God_Celestium celestium && !celestium.Defeated;
        }

        public override void castCommon(Location loc)
        {
            base.castCommon(loc);

            God_Celestium celestium = map.overmind.god as God_Celestium;
            if (celestium != null)
            {
                celestium.Grow();
            }
        }
    }
}
