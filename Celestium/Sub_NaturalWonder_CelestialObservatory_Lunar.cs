using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class Sub_NaturalWonder_CelestialObservatory_Lunar : Sub_NaturalWonder_CelestialObservatory
    {
        public int RespawnCooldown = 0;

        public List<Challenge> Challenges = new List<Challenge>();

        public P_Observatory_Moonfall Moonfall;

        public bool PowerUsed = false;

        public Sub_NaturalWonder_CelestialObservatory_Lunar(Settlement set)
            : base(set)
        {
            Moonfall = new P_Observatory_Moonfall(set.location.map, this);

            Challenges.Add(new Ch_Infiltrate(set.location, this));
            Challenges.Add(new Ch_Rest(set.location));
            Challenges.Add(new Ch_ObserveMoon(set.location, this));
            Challenges.Add(new Ch_ProphecyMoonfall(set.location, this));
        }

        public override string getName()
        {
            return "Lunar Observatory";
        }

        public override string getInvariantName()
        {
            return "Lunar Observatory";
        }

        public override Sprite getIcon()
        {
            return EventManager.getImg("ILGF_Celestium.Icon_LunarObservatory.jpg");
        }

        public override string getHoverOverText()
        {
            return $"This short partially ruined tower squating on the shoreline contains an ancient observatory, inhabited by scholars fascinated with the movements of the moon. An unknown power lingers here, one that the scholars are unknowingly drawn by... And it hungers for Shadow.";
        }

        public override bool definesName()
        {
            return true;
        }

        public override string generateParentName()
        {
            return "Lunar Observatory";
        }

        public override bool definesSprite()
        {
            return true;
        }

        public override Sprite getLocationSprite(World world)
        {
            return EventManager.getImg("ILGF_Celestium.Icon_LunarObservatory.png");
        }

        public override void turnTick()
        {
            Moonfall.turnTick();

            if (settlement.location.settlement != settlement)
            {
                World.Log("Celestium: Settlement at location is not this subsettlement's settlement. Replacing.");
                settlement.location.settlement = settlement;
            }

            if (settlement is Set_MinorOther setMinor)
            {
                if (CanProsper())
                {
                    RespawnCooldown--;

                    if (RespawnCooldown <= 0)
                    {
                        World.Log($"Celestium: Lunar Observatory prospers.");

                        RespawnCooldown = 0;

                        Set_MinorHuman setHuman = new Set_MinorHuman(setMinor.location);
                        setHuman.subs.Clear();
                        setHuman.subs.Add(this);
                        setMinor.subs.Remove(this);

                        setHuman.subs.Add(new Sub_Docks(setHuman));

                        settlement = setHuman;
                        setMinor.location.settlement = setHuman;

                        foreach (Subsettlement sub in setMinor.subs)
                        {
                            sub.settlement = setHuman;
                            setHuman.subs.Add(sub);
                        }

                        setHuman.population = Math.Min(setHuman.getFoodGenerated(), setHuman.getMaxPopulation());
                        setHuman.defences = setHuman.getMaxDefence();

                        CheckSociety();

                        if (settlement.location.map.burnInComplete && !settlement.location.map.automatic)
                        {
                            settlement.location.map.addUnifiedMessage(settlement.location, null, "Lunar Observatory Prospers", "The lunar obervatory has attracted scholars from across the known world. A small trading settlement has formed around it.", "Lunar Observatory Prospers");
                        }
                    }
                }
            }
            else if (settlement is SettlementHuman setHuman)
            {
                CheckSociety();

                if (!setHuman.subs.Any(sub => sub is Sub_Docks))
                {
                    setHuman.subs.Add(new Sub_Docks(setHuman));
                }
            }
            else
            {
                CheckSettlementType();
            }
        }

        public void CheckSettlementType()
        {
            Settlement ruins = settlement.location.settlement;

            if (ruins == null)
            {
                World.Log($"Celestium: Lunar Observatory must exist in a settlement of type `Set_MinorOther` or `SettlementHuman`. It was found that it's settlment has been removed entirely. Respawning with new `Set_MinorOther`.");
            }

            if (ruins is Set_MinorOther || ruins is SettlementHuman)
            {
                return;
            }

            if (!(ruins is Set_CityRuins))
            {
                World.Log($"Celestium: Lunar Observatory must exist in a settlement of type `Set_MinorOther` or `SettlementHuman`. It was found in Settlement of type {ruins.GetType().Name}. Replacing with new `Set_MinorOther`.");
            }

            RespawnCooldown = 5 + Eleven.random.Next(6);

            Set_MinorOther setMinor = new Set_MinorOther_Observatory(ruins.location);
            setMinor.subs.Clear();
            setMinor.subs.Add(this);
            ruins.subs.Remove(this);
            settlement = setMinor;
            ruins.location.settlement = setMinor;
            ruins.location.soc = null;

            foreach (Subsettlement sub in ruins.subs)
            {
                sub.settlement = setMinor;
                setMinor.subs.Add(sub);
            }
        }

        public bool CanProsper()
        {
            if (settlement.location.hex.getHabilitability() < settlement.location.map.param.mapGen_minHabitabilityForHumans)
            {
                return false;
            }

            if (settlement.location.units.Any(u => u is UM_RavenousDead) || settlement.location.getNeighbours().Any(n => n.units.Any(u => u is UM_RavenousDead)))
            {
                return false;
            }

            if (settlement.location.getNeighbours().Any(n => n.soc is SG_Orc))
            {
                return false;
            }

            return true;
        }

        public void CheckSociety()
        {
            if (!(settlement is SettlementHuman humanSettlement) || (settlement.location.soc is Society && settlement.location.soc != settlement.location.map.soc_dark))
            {
                return;
            }

            Society alliance = null;
            List<Society> societies = new List<Society>();
            foreach (Location neighbour in settlement.location.getNeighbours())
            {
                if (neighbour.hex.z == 0 && neighbour.soc is Society society)
                {
                    if (society.isAlliance)
                    {
                        alliance = society;
                    }
                    else
                    {
                        societies.Add(society);
                    }
                }
            }

            if (societies.Count > 0)
            {
                if (societies.Count > 1)
                {
                    settlement.location.soc= societies[Eleven.random.Next(societies.Count)];
                }
                else
                {
                    settlement.location.soc = societies[0];
                }
            }
            else if (alliance != null)
            {
                settlement.location.soc = alliance;
            }
            else
            {
                Society society = new Society(settlement.location.map, settlement.location);
                settlement.location.soc = society;
                society.setName("Lunaria");
            }

            humanSettlement.ruler?.embedIntoSociety();
        }

        public override double getProsperityInfluence()
        {
            return 0.1;
        }

        public override bool canBeInfiltrated()
        {
            return true;
        }

        public override bool survivesRuin()
        {
            return true;
        }

        public override List<Challenge> getChallenges()
        {
            return Challenges;
        }
    }
}
