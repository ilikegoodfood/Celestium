using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celestium
{
    public class Ch_ProphecyDusk : Ch_FulfillTheProphecy
    {
        public Ch_ProphecyDusk(Location loc)
            : base(loc)
        {

        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = base.getUtility(ua, msgs);

            if (!(map.overmind.god is God_Celestium celestium) || celestium.GlobalThermalLimit <= 0.0 || celestium.Menace <= 0.0)
            {
                return utility;
            }

            double val = 200.0 * celestium.GlobalThermalLimit;
            msgs?.Add(new ReasonMsg("The world burns", val));
            utility += val;

            val = celestium.Menace;
            msgs?.Add(new ReasonMsg("Celestium is a threat", val));
            utility += val;

            return utility;
        }
    }
}
