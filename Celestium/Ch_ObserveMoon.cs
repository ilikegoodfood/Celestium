using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class Ch_ObserveMoon : Challenge
    {
        public Sub_NaturalWonder_CelestialObservatory_Lunar LunarObservatory;

        public Ch_ObserveMoon(Location loc, Sub_NaturalWonder_CelestialObservatory_Lunar lunarObservatory)
            : base(loc)
        {
            LunarObservatory = lunarObservatory;
        }

        public override string getName()
        {
            return "Conduct Lunar Observations";
        }

        public override string getDesc()
        {
            return "Fully enshadow this location.";
        }

        public override string getCastFlavour()
        {
            return "The prestine, pockmarked surface of the Moon draws your eyes. The brilliant shine of the reflected sunlight plays against the impossible darkness of the shadows cast by the crater rms, mountains, and boulders of its surface. As you focus the observatory telescope on the darkness of lunar shadow, it reaches out to you.";
        }

        public override string getRestriction()
        {
            return $"The Lunar Observatory must be infiltrated. Cannot perform if <b>Ward</b> is higher than 50 charge.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_Moon.jpg");
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            Pr_Ward ward = (Pr_Ward)location.properties.FirstOrDefault(pr => pr is Pr_Ward);

            return location.getShadow() < 1.0 && LunarObservatory.infiltrated && (ward == null || ward.charge < 50.0);
        }

        public override bool validFor(UA ua)
        {
            return ua.isCommandable();
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
        }

        public override double getComplexity()
        {
            if (map.tutorial)
            {
                return map.param.ch_enshadow_complexity;
            }

            return map.param.ch_enshadow_parameterValue4;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double val = unit.getStatLore();
            if (val > 1.0)
            {
                msgs?.Add(new ReasonMsg("Stat: Lore", val));
                return val;
            }

            val = 1.0;
            msgs?.Add(new ReasonMsg("Base", val));
            return val;
        }

        public override void complete(UA u)
        {
            if (location.settlement == null)
            {
                location.hex.purity = 0f;
            }
            else
            {
                location.settlement.shadow = 1.0;
            }
        }

        public override int getCompletionProfile()
        {
            return this.map.param.ch_enshadow_parameterValue2;
        }

        public override int getCompletionMenace()
        {
            return this.map.param.ch_enshadow_parameterValue1;
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.SHADOW
            };
        }

        public override int[] buildNegativeTags()
        {
            return new int[]
            {
                Tags.COMBAT
            };
        }
    }
}
