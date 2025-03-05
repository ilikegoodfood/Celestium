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

        public Sub_NaturalWonder_CelestialObservatory_Lunar(Settlement set)
            : base(set)
        {

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
            if (settlement is Set_CityRuins ruins)
            {
                Respawn();
            }
            else if (settlement is Set_MinorOther setMinor)
            {
                if (setMinor.location.hex.getHabilitability() >= setMinor.location.map.param.mapGen_minHabitabilityForHumans * setMinor.location.map.param.mapGen_minHabitabilityForHumans)
                {
                    RespawnCooldown--;

                    if (RespawnCooldown <= 0)
                    {
                        RespawnCooldown = 0;

                        CheckSociety();

                        Set_MinorHuman setHuman = new Set_MinorHuman(setMinor.location);
                        setHuman.subs.Clear();
                        setHuman.subs.Add(this);
                        setMinor.subs.Remove(this);

                        setMinor.subs.Add(new Sub_Docks(setMinor));

                        settlement = setHuman;
                        setMinor.location.settlement = setHuman;

                        foreach (Subsettlement sub in setMinor.subs)
                        {
                            sub.settlement = setHuman;
                            setHuman.subs.Add(sub);
                        }

                        setHuman.population = Math.Min(setHuman.getFoodGenerated(), setHuman.getMaxPopulation());
                        setHuman.defences = setHuman.getMaxDefence();

                        if (setHuman.location.soc is Society society)
                        {
                            Person ruler = new Person(society);
                            if (society != setHuman.location.map.soc_dark)
                            {
                                ruler.embedIntoSociety();
                            }

                            setHuman.ruler = ruler;
                        }

                        if (settlement.location.map.burnInComplete && !settlement.location.map.automatic)
                        {
                            settlement.location.map.addUnifiedMessage(settlement.location, null, "Lunar Observatory Prospers", "Driven by the wealth and demand for specialised goods at the Lunar Observatory, a thriving town has sprung up around it.", "Lunar Observatory Propspers");
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
        }

        public void Respawn()
        {
            Settlement ruins = settlement.location.settlement;

            RespawnCooldown = 5 + Eleven.random.Next(6);

            Set_MinorOther setMinor = new Set_MinorOther(ruins.location);
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

        public void CheckSociety()
        {
            if (settlement.location.soc is Society && settlement.location.soc != settlement.location.map.soc_dark)
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

                return;
            }

            if (alliance != null)
            {
                settlement.location.soc = alliance;
                return;
            }

            settlement.location.soc = settlement.location.map.soc_dark;
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
