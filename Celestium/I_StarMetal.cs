using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class I_StarMetal : Item
    {
        public I_StarMetal(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Star Metal";
        }

        public override string getShortDesc()
        {
            return "A lump of celestial metal that has fallen from the sky. It radiates heat and power.";
        }

        public override Sprite getIconFore()
        {
            return EventManager.getImg("ILGF_Celestium.Fore_StarMetal.png");
        }

        public override int getLevel()
        {
            return LEVEL_NODROP;
        }

        public override int getMorality()
        {
            return MORALITY_NEUTRAL;
        }
    }
}
