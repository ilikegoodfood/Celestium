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
                return $"{celestium.Menace} menace";
            }
            return base.getIconText();
        }

        public override string getHoverOverText()
        {
            return "Celestium. A new born Star. A newborn god. It rests just above the surface of the world, it's blinding rays melting the very rick beneath it. All that approach without due defference burn.";
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
