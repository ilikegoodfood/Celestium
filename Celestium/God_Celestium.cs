using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class God_Celestium : God
    {
        public override void setup(Map map)
        {
            base.setup(map);

            map.overmind.sealProgress = 1;
            map.overmind.sealsBroken = 1;
            map.overmind.power = 3 * map.overmind.sealsBroken + 10;

            awaken();
        }

        public override string getName()
        {
            return "Celestium";
        }

        public override string getDescFlavour()
        {
            return "A star is born. A Celestial, barely hours old, shines brilliantly enough to burn the world itself, vaporizing the shadowy remnets of any prior elder being that thought to claim theis world as it's own.";
        }

        public override string getDescMechanics()
        {
            return "Celestium's mechanics revolve around the production of power, rampant destruction, and temperature changes. If provided with a steady supply of power Celestium can lay watse to the world by altering the movments of the celestial bodies. If it burns enough shadow, the global temperature will rise, and humanity will preish beneath the scorching ash that will coat the lands.";
        }

        public override bool selectable()
        {
            return false;
        }

        public override string getCredits()
        {
            return "Mod Creator: ILikeGoodFood";
        }

        public override Sprite getSupplicant()
        {
            return map.world.textureStore.agent_supplicantSnake;
        }

        public override bool hasSupplicantStartingTraits()
        {
            return false;
        }

        public override void breakSeal(int sealsBroken)
        {
            map.overmind.power += 3.0;
            map.world.prefabStore.popMsgSeal();
        }

        public override int[] getSealLevels()
        {
            return new int[] {
                0,
                64,
                160,
                328
            };
        }

        public override int[] getAgentCaps()
        {
            return new int[] {
                5,
                6
            };
        }

        public override int getMaxTurns()
        {
            return 500;
        }

        public override Sprite getGodBackground(World world)
        {
            return EventManager.getImg("ILGF_Celestium.Img_StarBirth.jpg");
        }

        public override Sprite getGodPortrait(World world)
        {
            return EventManager.getImg("ILGF_Celestium.Img_StarBirth.jpg");
        }

        public override double getWorldPanicOnAwake()
        {
            return 0.75;
        }

        public override void awaken()
        {
            base.awaken();
        }

        public override string getAwakenMessage()
        {
            return "A new Star is born! It's radiance shines out over the world, buring everything it touches, including yourself. Your influence is ripped from you, combusted like so much kindling before a blaze... A blaze of unimagined preportions.\n\nYou have been Betrayed! Celestium, the living star, a being of immesurable destructive power, is born, and before it, you are burned away as little more than a whisp of your own shadow...\n\nYou are now playing as Celestium, a newborn star. Celestium is a ravenous consumer of power that can move the heavens themselves in its quest for fuel.";
        }

        public override List<Power> getPowers()
        {
            List<Power> list = new List<Power>();
            for (int i = 0; i < powers.Count; i++)
            {
                if (!powers[i].isPassiveOnly())
                {
                    bool flag2 = powerLevelReqs[i] <= map.overmind.sealsBroken;
                    if (flag2)
                    {
                        list.Add(powers[i]);
                    }
                }
            }
            return list;
        }

        public override int getMaxPower()
        {
            return 3 * map.overmind.sealsBroken + 10;
        }

        public override double getPowerPerTurn()
        {
            return map.param.overmind_powerRegen * getMaxPower() * map.difficultyMult_shrinkWithDifficulty;
        }

        public override void turnTick_PowerGain()
        {
            double powerPerTurn = getPowerPerTurn();
            map.overmind.power += powerPerTurn;
            if (map.overmind.power > 3 * map.overmind.sealsBroken + 10)
            {
                map.overmind.power = 3 * map.overmind.sealsBroken + 10;
            }
            else
            {
                bool flag2 = this.map.overmind.power < (double)(this.map.overmind.sealsBroken + 1) && this.map.burnInComplete;
                if (flag2)
                {
                    this.map.hintSystem.popHint(HintSystem.hintType.IASTUR_REGEN);
                }
            }
        }
    }
}
