using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return "Build an elder tomb.";
        }

        public override void complete(UA u)
        {
            map.overmind.defeat(u.getName() + " has managed to seal you within an elder tomb, condemending you to waiting thousands of years until you can return to the world");
        }
    }
}
