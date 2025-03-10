using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class Pr_StarMetal : Property
    {
        public List<Challenge> challenges = new List<Challenge>();

        public Pr_StarMetal(Location loc)
            : base(loc)
        {
            challenges.Add(new Ch_GatherStarMetal(loc));
            charge = 100.0;
        }

        public override string getName()
        {
            return "Star Metal";
        }

        public override string getDesc()
        {
            return "A chunk of celestial metal, still hot from it's arrival as a shooting star. It radiated unseen energies, in addition to it's heat.";
        }

        public override Sprite getSprite(World world)
        {
            return EventManager.getImg("ILGF_Celestium.Fore_StarMetal.png");
        }

        public override bool hasBackgroundHexView()
        {
            return true;
        }

        public override Sprite getHexBackgroundSprite()
        {
            return EventManager.getImg("ILGF_Celestium.Hex_StarMetal.png");
        }

        public override List<Challenge> getChallenges()
        {
            return challenges;
        }
    }
}
