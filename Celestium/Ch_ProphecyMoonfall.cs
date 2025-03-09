using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class Ch_ProphecyMoonfall : Challenge
    {
        public Sub_NaturalWonder_CelestialObservatory_Lunar LunarObservatory;

        public Ch_ProphecyMoonfall(Location loc, Sub_NaturalWonder_CelestialObservatory_Lunar lunarObservatory)
            : base(loc)
        {
            LunarObservatory = lunarObservatory;
        }

        public override string getName()
        {
            return "Prophecy Moonfall";
        }

        public override string getDesc()
        {
            return $"Predicts bombardment of a location of your choosing, and the surrounding area (within {LunarObservatory.Moonfall.Radius} hexes), after a short delay, by a cloud of small meteors. These will deal damage to city defenses, armies, and agents within the area, possibly razing settlements and killing units.";
        }

        public override string getCastFlavour()
        {
            return "Ancient power resides in this observatory. By expending some effort, you can alter what it sees, and the observatory's power will make it real. A nudge here, a nudge there, and the sky falls on your enemies.";
        }

        public override string getRestriction()
        {
            return "The Lunar Observatory must be infiltrated. Can only be used once per game.";
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.assaultChanneller;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            return !LunarObservatory.PowerUsed && location.getShadow() > 0.5;
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
            u.map.world.selector = new Sel_CastPower(u.map.world, LunarObservatory.Moonfall);
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
