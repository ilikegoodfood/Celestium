﻿using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class Ch_Sunfall : Challenge
    {
        public int PowerCost = 3;

        public Ch_Sunfall(Location loc)
            : base(loc)
        {

        }

        public override string getName()
        {
            return "Prophecy Sunfall";
        }

        public override string getDesc()
        {
            return "Birth a star at this Solar Observsatory. This will have irrevocable and permenant effects on your game.";
        }

        public override string getCastFlavour()
        {
            return "By focussing you power on the Celestial Metal, and combining it with the painful radiace embueing the Solar Observatory, you can move the stars themselves.";
        }

        public override string getRestriction()
        {
            return $"Requires {PowerCost} power. Must be performed at a fully charged Solar Observatory by and agent carrying the Star Metal item retrieved from the Starfall. Can only be performed once per game.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_Sunfall.jpg");
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            return ModCore.opt_EnableGod && !ModCore.Instance.Celestium && map.overmind.power >= PowerCost;
        }

        public override bool validFor(UA ua)
        {
            return ua.isCommandable() && ua.person.items.Any(i => i is I_StarMetal);
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
        }

        public override double getComplexity()
        {
            return map.param.ch_enshadow_parameterValue4 * 2;
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
            ModCore.Instance.Celestium = true;
            God_Celestium Celestium = new God_Celestium();
            ModCore.Instance.CelestiumGod = Celestium;
            u.map.overmind.god = Celestium;
            Celestium.setup(u.map);

            for (int i = 0; i < u.person.items.Length; i++)
            {
                if (u.person.items[i] is I_StarMetal)
                {
                    u.person.items[i] = null;
                    break;
                }
            }

            u.map.world.ui.checkData();
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
