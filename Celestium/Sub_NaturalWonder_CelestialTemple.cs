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
    public class Sub_NaturalWonder_CelestialTemple : Sub_NaturalWonder
    {
        public int Power = 0;

        public Sub_NaturalWonder_CelestialTemple(Settlement set)
            : base(set)
        {

        }

        public override string getName()
        {
            if (Power > 0)
            {
                return $"Celestial Temple ({Power} power invested)";
            }

            return "Celestial Temple";
        }

        public override Sprite getIcon()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_CelestialTemple.jpg");
        }

        public override string getInvariantName()
        {
            return "Celestial Temple";
        }

        public override bool definesName()
        {
            return true;
        }

        public override string generateParentName()
        {
            return "Celestial Temple";
        }

        public override bool definesSprite()
        {
            return true;
        }

        public override Sprite getLocationSprite(World world)
        {
            return EventManager.getImg("ILGF_Celestium.Icon_CelestialTemple.png");
        }
    }
}
