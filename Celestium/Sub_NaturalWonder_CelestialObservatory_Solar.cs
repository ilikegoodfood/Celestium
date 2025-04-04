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
        public int SolarObservationDuration = 0;

        public double Shadow = 0.0;

        public double ShadowBurnRate = 0.02;

        public double MaximumShadow = 3.0;

        public bool PowerUsed = false;

        public Sub_NaturalWonder_CelestialObservatory_Lunar LunarObservatory;

        public P_Observatory_Starfall Starfall;

        public List<Challenge> Challenges = new List<Challenge>();

        public Sub_NaturalWonder_CelestialObservatory_Solar(Settlement set, Sub_NaturalWonder_CelestialObservatory_Lunar lunarObservatory)
            : base(set)
        {
            LunarObservatory = lunarObservatory;

            Starfall = new P_Observatory_Starfall(set.location.map, this);

            Challenges.Add(new Ch_Rest(set.location));
            Challenges.Add(new Ch_ObserveSun(set.location, this));
            Challenges.Add(new Ch_ProphecyStarfall(set.location, this));

            if (ModCore.opt_EnableGod)
            {
                Challenges.Add(new Ch_Sunfall(set.location));
            }
        }

        public override string getName()
        {
            return "Solar Observatory";
            
        }

        public override Sprite getIcon()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_SolarObservatory.jpg");
        }

        public override string getHoverOverText()
        {
            return $"This squat, partially ruined tower nestled in the mountains contains an ancient observatory, inhabited by scholars fascinated with the movement of the sun and stars. An unknown power lingers here, one that the scholars are unknowingly drawn by... And it hungers for Shadow.";
        }

        public override string getIconText()
        {
            if (ModCore.Instance.Celestium && settlement.map.overmind.god is God_Celestium celestium && !celestium.Defeated)
            {
                return "";
            }

            return $"Shadow Burnt {(int)Math.Round(Shadow * 100)}/{(int)Math.Round(MaximumShadow * 100)}";
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
            Starfall.turnTick();
        }

        public void GainShadow(double shadowGain)
        {
            Shadow += shadowGain;
            if (Shadow > MaximumShadow)
            {
                Shadow = MaximumShadow;
            }
            else if (Shadow < 0.0)
            {
                Shadow = 0.0;
            }
        }

        public override List<Challenge> getChallenges()
        {
            return Challenges;
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
