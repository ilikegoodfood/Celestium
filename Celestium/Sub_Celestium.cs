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
    public class Sub_Celestium : Subsettlement
    {
        public Sub_Celestium(Settlement set)
            : base(set)
        {
        }

        public override string getName()
        {
            return "Celestium";
        }

        public override string getIconText()
        {
            if (settlement.map.overmind.god is God_Celestium celestium)
            {
                return $"+{(int)Math.Round(celestium.GlobalThermalLimit * 100)}% temperature | {celestium.Menace} menace";
            }

            return base.getIconText();
        }

        public override string getHoverOverText()
        {
            return "Celestium, a newborn star, consumes power, in the form of shadow, to increase global temperatures. Once strong enough, it will start to draw heat from the world itself, allowing it to rapidly accelerate this process.";
        }

        public override Sprite getIcon()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_P_Grow.jpg");
        }

        public override bool survivesRuin()
        {
            return true;
        }

        public override bool canBeInfiltrated()
        {
            return false;
        }
    }
}
