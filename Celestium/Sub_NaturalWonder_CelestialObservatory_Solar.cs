using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class Sub_NaturalWonder_CelestialObservatory_Solar : Sub_NaturalWonder_CelestialObservatory
    {
        public double Shadow = 0.0;

        public Sub_NaturalWonder_CelestialObservatory_Lunar LunarObservatory;

        public Sub_NaturalWonder_CelestialObservatory_Solar(Settlement set, Sub_NaturalWonder_CelestialObservatory_Lunar lunarObservatory)
            : base(set)
        {
            LunarObservatory = lunarObservatory;
        }

        public override string getName()
        {
            if (Shadow > 0)
            {
                return $"Solar Observatory ({(int)Math.Round(Shadow)} charge)";
            }

            return "Solar Observatory";
        }

        public override string getInvariantName()
        {
            return "Solar Observatory";
        }

        public override Sprite getIcon()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_SolarObservatory.jpg");
        }

        public override bool definesName()
        {
            return true;
        }

        public override string generateParentName()
        {
            return "Solar Observatory";
        }

        public override bool definesSprite()
        {
            return true;
        }

        public override Sprite getLocationSprite(World world)
        {
            return EventManager.getImg("ILGF_Celestium.Icon_SolarObservatory.png");
        }

        public override void turnTick()
        {
            base.turnTick();
        }

        public override bool canBeInfiltrated()
        {
            return false;
        }

        public override bool survivesRuin()
        {
            return true;
        }
    }
}
