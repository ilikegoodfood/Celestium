using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class Mg_DeathOfTheSun : Challenge
    {
        public Pr_GeomanticLocus Locus;

        public float TemperatureChangePerProgress = 0.0005f;

        public Mg_DeathOfTheSun(Location loc, Pr_GeomanticLocus locus)
            : base(loc)
        {
            Locus = locus;
        }

        public override string getName()
        {
            return "Geomancy: Death of the Sun";
        }

        public override string getDesc()
        {
            return "Channelled magic. Reduces the temperature of the entire world, slowly counteracting the effects of Celestium's brilliance. Effects applied each turn based on geomantic locus strength and caster lore stat.";
        }

        public override string getRestriction()
        {
            return "Requires Geomancy Level 1 and for the geomantic locus here to be above 10% charge";
        }

        public override string getCastFlavour()
        {
            return "The world begins to cool, Celestium grows colder and the ash falls. The cool earth gives more each harvest, and the people hungry people will soon be fed, without a burning summer to fear.";
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.deathOfTheSun;
        }

        public override bool isChannelled()
        {
            return true;
        }

        public override double getChannellingDanger()
        {
            return map.param.mg_channellingDanger_level3;
        }

        public override bool valid()
        {
            return location.properties.Contains(Locus) && Locus.charge >= 10.0 && map.overmind.god is God_Celestium celestium && !celestium.Defeated && celestium.GlobalThermalLimit > 0.0;
        }

        public override bool validFor(UA ua)
        {
            return !ua.map.opt_allowMagicalArmsRace || !ua.map.opt_enableMagic || ua.person.traits.Any(t => t is T_MasteryGeomancy geomancy && geomancy.level >= 1);
        }

        public override double getProfile()
        {
            if (map.overmind.god is God_Celestium celestium)
            {
                return Math.Round(20 + celestium.Menace);
            }

            return 0.0;
        }

        public override double getMenace()
        {
            return 0.0;
        }

        public override double getComplexity()
        {
            return 50.0;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 0.0;
            if (!(map.overmind.god is God_Celestium celestium) || celestium.GlobalThermalLimit <= 0.0 || celestium.Menace <= 0.0)
            {
                return utility;
            }

            double val = 500.0 * celestium.GlobalThermalLimit;
            msgs?.Add(new ReasonMsg("The world burns", val));
            utility += val;

            val = celestium.Menace;
            msgs?.Add(new ReasonMsg("Celestium is a threat", val));
            utility += val;

            return utility;
        }

        public override Challenge.challengeStat getChallengeType()
        {
            return Challenge.challengeStat.LORE;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = unit.getStatLore();
            if (result >= 1.0)
            {
                msgs?.Add(new ReasonMsg("Stat: Lore", result));
                return result;
            }

            result = 1.0;
            msgs?.Add(new ReasonMsg("Base", result));
            return result;
        }

        public override void turnTick(UA ua)
        {
            if(!(map.overmind.god is God_Celestium celestium))
            {
                return;
            }

            celestium.GlobalThermalLimit -= (ua.getStatLore() >= 1.0 ? ua.getStatLore() : 1f) * TemperatureChangePerProgress;
            if (celestium.GlobalThermalLimit < 0f)
            {
                celestium.GlobalThermalLimit = 0f;
            }

            Locus.influences.Add(new ReasonMsg(ua.getName(), -4.0));
            if (Locus.charge < 1.0)
            {
                Locus.charge = 1.0;
            }
            GraphicalMap.map.assignTerrainFromClimate();
        }

        public override int isGoodTernary()
        {
            return 1;
        }
    }
}
