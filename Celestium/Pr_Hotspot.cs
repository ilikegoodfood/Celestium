using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class Pr_Hotspot : Property
    {
        public List<Challenge> Challenges = new List<Challenge>();

        public int Size;

        public double DecayRate = 0.05;

        public Pr_Hotspot(Location loc)
            : base(loc)
        {
            Challenges.Add(new Ch_DrawHeat(this));
        }

        public override string getName()
        {
            switch (Size)
            {
                case 0:
                    return "Small Geothermal Hotspot";
                case 1:
                    return "Moderate Geothermal Hotspot";
                case 2:
                    return "Large Geothermal Hotspot";
                case 3:
                    return "Extreme Geothermal Hotspot";
                default:
                    return "Small Geothermal Hotspot";
            }
        }

        public override string getInvariantName()
        {
            return "Geothermal Hotspot";
        }

        public override string getDesc()
        {
            return "A source of heat both just beneath the world's surface, and in the underground. They are trasnient, and will decay over time. The larger they are, the faster they decay.";
        }

        public override Sprite getSprite(World world)
        {
            return EventManager.getImg("ILGF_Celestium.Icon_Hotspot.jpg");
        }

        public override List<Challenge> getChallenges()
        {
            return Challenges;
        }

        public override void turnTick()
        {
            if (charge < 1.0)
            {
                charge = 0.0;
                return;
            }

            double decay = -Math.Max(2, charge * DecayRate);
            if (charge - decay < 1.0)
            {
                decay = charge;
            }
            influences.Add(new ReasonMsg("Natural Cooling", decay));
        }

        public override bool survivesRuin()
        {
            return true;
        }
    }
}
