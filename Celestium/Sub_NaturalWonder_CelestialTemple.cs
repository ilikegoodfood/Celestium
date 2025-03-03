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
        public Sub_NaturalWonder_CelestialTemple(Settlement set)
            : base(set)
        {

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
