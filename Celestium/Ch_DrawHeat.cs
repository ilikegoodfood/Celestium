using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class Ch_DrawHeat : Challenge
    {
        public Pr_Hotspot Hotspot;

        public float TemperatureChangePerProgress = 0.001f;

        public double ProfilePerTurn = 0.5;

        public double MenacePerTurn = 1.5;

        public Ch_DrawHeat(Pr_Hotspot hotspot)
            : base(hotspot.location)
        {
            Hotspot = hotspot;
        }

        public override string getName()
        {
            return "Draw Heat";
        }

        public override string getDesc()
        {
            return "Draw heat from deep within the earth, cooking the surface and accelerating Celestium's growth. Larger hotspots and agents with higher lore are more efficient.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_Hotspot.jpg");
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
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
            return Hotspot != null && Hotspot.charge >= 1.0 && Hotspot.location.properties.Contains(Hotspot) && location.map.overmind.god is God_Celestium;
        }

        public override bool validFor(UA ua)
        {
            return ua.isCommandable();
        }

        public override double getComplexity()
        {
            return 150.0;
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
            if (!(ua.map.overmind.god is God_Celestium celestium))
            {
                return;
            }

            double progress = getProgressPerTurn(ua, null);
            celestium.GlobalThermalLimit += (float)progress * TemperatureChangePerProgress * (1 + Hotspot.Size);

            Hotspot.influences.Add(new ReasonMsg("Heat drawn to the surface", -5.0));

            ua.addProfile(ProfilePerTurn);
            ua.addMenace(MenacePerTurn);
        }

        public override int isGoodTernary()
        {
            return -1;
        }
    }
}
