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

            Challenges.Add(new Ch_ObserveSun(set.location, this));
            Challenges.Add(new Ch_ProphecyStarfall(set.location, this));
        }

        public override string getName()
        {
            if (Shadow < 1.0)
            {
                return "Solar Observatory";
            }

            return $"Solar Observatory ({(int)Math.Round(Shadow * 100)}/{(int)Math.Round(MaximumShadow * 100)} charge)";
            
        }

        public override string getInvariantName()
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

            double burnRate = ShadowBurnRate;
            if (SolarObservationDuration > 0)
            {
                SolarObservationDuration--;
                burnRate += ShadowBurnRate;
            }

            if (settlement.shadow > 0.0)
            {
                double burn = Math.Min(settlement.shadow, burnRate);
                settlement.shadow -= burn;
                Shadow += burn;

                if (settlement.shadow < 0.0)
                {
                    settlement.shadow = 0.0;
                }
            }

            if (SolarObservationDuration > 0)
            {
                foreach (Location neighbour in settlement.location.getNeighbours())
                {
                    if (neighbour.settlement == LunarObservatory.settlement)
                    {
                        continue;
                    }

                    if (neighbour.settlement != null && neighbour.settlement.shadow > 0.0)
                    {
                        double burn = Math.Min(neighbour.settlement.shadow, ShadowBurnRate);
                        neighbour.settlement.shadow -= burn;
                        Shadow += burn;

                        if (neighbour.settlement.shadow < 0.0)
                        {
                            neighbour.settlement.shadow = 0.0;
                        }
                    }
                    else if (neighbour.hex.purity < 1f)
                    {
                        float burn = Math.Min(1f - neighbour.hex.purity, (float)ShadowBurnRate);
                        neighbour.hex.purity += burn;
                        Shadow += burn;

                        if (neighbour.hex.purity > 1f)
                        {
                            neighbour.hex.purity = 1f;
                        }
                    }
                }
            }

            if (LunarObservatory != null && LunarObservatory.settlement.shadow > 0.0)
            {
                double burn = Math.Min(LunarObservatory.settlement.shadow, burnRate);
                LunarObservatory.settlement.shadow -= burn;
                Shadow += burn;

                if (LunarObservatory.settlement.shadow < 0.0)
                {
                    LunarObservatory.settlement.shadow = 0.0;
                }
            }

            if (Shadow > MaximumShadow)
            {
                Shadow = MaximumShadow;
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
