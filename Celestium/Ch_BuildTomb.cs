using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SortedDictionaryProvider;

namespace Celestium
{
    public class Ch_BuildTomb : Ch_ReforgeTheSeals
    {
        public Ch_BuildTomb(Location loc)
            : base(loc)
        {

        }

        public override string getName()
        {
            return "Build an Elder Tomb.";
        }

        public override void complete(UA u)
        {
            map.overmind.defeat(u.getName() + " has managed to seal you within an elder tomb, condemending you to waiting thousands of years until you can return to the world");
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = map.param.ch_reforgetheseals_parameterValue2;
            msgs?.Add(new ReasonMsg("Cost to Self", utility));

            double val = 0;
            if (ua.person.awareness == 1.0)
            {
                val = 200.0 * (1.0 - ua.person.shadow);
                msgs?.Add(new ReasonMsg("Awareness", val));
                utility += val;
            }

            val = 200.0 * map.worldPanic;
            msgs?.Add(new ReasonMsg("World Panic", val));
            utility += val;

            Pr_ArcaneFortress fortress = (Pr_ArcaneFortress)location.properties.FirstOrDefault(pr => pr is Pr_ArcaneFortress);
            if (fortress == null || fortress.charge < 100.0)
            {
                val = 50.0 - (fortress?.charge ?? 0.0);
                msgs?.Add(new ReasonMsg("Chosen One Needs Arcane Fortress", val));
            }

            if (map.turn - cooldownTurn < map.param.ch_reforgeTheSealsCooldown)
            {
                val = map.param.ch_reforgetheseals_parameterValue3;
                msgs?.Add(new ReasonMsg("Recent failure", val));
                utility += val;
            }

            if (debugLure)
            {
                val = 20000.0;
                msgs?.Add(new ReasonMsg("Debug Lure", val));
                utility += val;
            }

            return utility;
        }

        public override void turnTick(UA ua)
        {
            Pr_ArcaneFortress fortress = (Pr_ArcaneFortress)ua.location.properties.FirstOrDefault(pr => pr is Pr_ArcaneFortress);
            if (fortress != null && fortress.charge <= 46.0)
            {
                fortress.influences.Add(new ReasonMsg("Builidng an Elder Tomb", 4.0));
            }
        }
    }
}
