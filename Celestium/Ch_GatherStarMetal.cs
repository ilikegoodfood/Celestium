using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celestium
{
    public class Ch_GatherStarMetal : Ch_BuyItem
    {
        public Ch_GatherStarMetal(Location loc)
            : base(loc)
        {
            onSale = new I_StarMetal(loc.map);
            tern = 0;
        }

        public override string getName()
        {
            return "Gather Star Metal";
        }

        public override string getDesc()
        {
            return "Gather the lump of Star Metal that resides here.";
        }

        public override string getRestriction()
        {
            return "";
        }

        public override bool validFor(UA ua)
        {
            return ua.isCommandable();
        }

        public override void complete(UA u)
        {
            u.person.gainItem(onSale);

            Pr_StarMetal starMetal = (Pr_StarMetal)location.properties.FirstOrDefault(pr => pr is Pr_StarMetal star && star.challenges.Contains(this));
            if (starMetal != null)
            {
                location.properties.Remove(starMetal);
            }
        }

        public override int getDanger()
        {
            return 3;
        }
    }
}
