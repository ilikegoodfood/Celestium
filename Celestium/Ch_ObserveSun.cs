using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class Ch_ObserveSun : Challenge
    {
        public Sub_NaturalWonder_CelestialObservatory_Solar SolarObservatory;

        public int EffectDuration = 25;

        public Ch_ObserveSun(Location loc, Sub_NaturalWonder_CelestialObservatory_Solar solarObservatory)
            : base(loc)
        {
            SolarObservatory = solarObservatory;
        }

        public override string getName()
        {
            return "Conduct Solar Observations";
        }

        public override string getDesc()
        {
            return $"For {EffectDuration} turns, double the rate at which the Solar Observatory burns shadow from itself and the lunar observatory. This increases its charge rate. Also causes it to burn, and charge from, shadow in neighbouring locations.";
        }

        public override string getCastFlavour()
        {
            return "The brilliance of the sun burns you. Even through the projection on the observatory wall that your agent beholds, you feel your power being burned away by its blazing light. And as you feel yourself diminished by it, it's shine intensifies.";
        }

        public override string getRestriction()
        {
            if (SolarObservatory.SolarObservationDuration > 0)
            {
                return $"Costs 1 Power. Cannot be performed too soon after prior solar observations ({SolarObservatory.SolarObservationDuration} turns).";
            }

            return $"Costs 1 Power. Cannot be performed too soon after prior solar observations.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_Sun.jpg");
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            return map.overmind.power >= 1.0 && SolarObservatory.SolarObservationDuration <= 0;
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
            SolarObservatory.SolarObservationDuration += EffectDuration;
            map.overmind.power--;
        }

        public override int getCompletionProfile()
        {
            return this.map.param.ch_enshadow_parameterValue2;
        }

        public override int getCompletionMenace()
        {
            return this.map.param.ch_enshadow_parameterValue1;
        }

        public override int[] buildNegativeTags()
        {
            return new int[]
            {
                Tags.SHADOW
            };
        }
    }
}
