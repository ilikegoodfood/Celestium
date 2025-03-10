using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class Ch_ProphecyStarfall : Challenge
    {
        public Sub_NaturalWonder_CelestialObservatory_Solar SolarObservatory;

        public Ch_ProphecyStarfall(Location loc, Sub_NaturalWonder_CelestialObservatory_Solar solarObservatory)
            : base(loc)
        {
            SolarObservatory = solarObservatory;
        }

        public override string getName()
        {
            return "Prophecy Starfall";
        }

        public override string getDesc()
        {
            return $"Predicts bombardment of a location of your choosing, and the surrounding area (within {SolarObservatory.Starfall.Radius} hexes), after a short delay, by large meteor. It will destroy all settlments withing the radius, and kill all units, excluding the CHosen One. It will also leave behind a mass of Star Metal.";
        }

        public override string getCastFlavour()
        {
            return "Ancient power resides in this observatory. By expending some effort, you can alter what it sees, and the observatory's power will make it real. A nudge here, a nudge there, and the sky falls on your enemies.";
        }

        public override string getRestriction()
        {
            return $"The Solar Observatory must have {(int)Math.Round(SolarObservatory.MaximumShadow * 100.0)} charge. Can only be used once per game.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_Starfall.jpg");
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            return !SolarObservatory.PowerUsed && SolarObservatory.Shadow >= SolarObservatory.MaximumShadow;
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
            u.map.world.selector = new Sel_CastPower(u.map.world, SolarObservatory.Starfall);
        }

        public override int getCompletionProfile()
        {
            return 20;
        }

        public override int getCompletionMenace()
        {
            return 40;
        }
    }
}
