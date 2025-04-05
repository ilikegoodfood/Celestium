using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celestium
{
    public class T_BurningSoul : Trait
    {
        public int Duration = 5;

        public bool bonusMove = false;

        public override string getName()
        {
            return $"Bruning Soul ({Duration + 1} turns)";
        }

        public override string getDesc()
        {
            return "Grants +2 all stats and a bonus movement speed once per turn at the cost of -1 hp per turn. Agents that die while under the effect of Burning Soul increase the global temperature slightly, and Player agents become Embers.";
        }

        public override void turnTick(Person p)
        {
            Duration--;
            if (Duration < 0 || !p.hasSoul)
            {
                p.traits.Remove(this);
                return;
            }

            if (!(p.unit is UA ua))
            {
                p.traits.Remove(this);
                return;
            }

            ua.hp--;
            if (ua.hp <= 0)
            {
                ua.die(ua.map, "Burned by Celestium's Light");
                return;
            }

            ua.movesTaken--;
        }

        public override int getAttackChange()
        {
            return 2;
        }

        public override int getMightChange()
        {
            return 2;
        }

        public override int getLoreChange()
        {
            return 2;
        }

        public override int getIntrigueChange()
        {
            return 2;
        }

        public override int getCommandChange()
        {
            return 2;
        }

        public override void onDeath(Unit unit, Person killer)
        {
            if (unit == null || !(unit is UA ua))
            {
                return;
            }

            if (unit.map.overmind.god is God_Celestium celestium)
            {
                celestium.GlobalThermalLimit += 0.1f;
            }

            if (!unit.isCommandable())
            {
                return;
            }

            UAE_Ember ember = new UAE_Ember(ua.location, ua.map.soc_dark);
            ua.location.units.Add(ember);
            ua.map.units.Add(ember);
            ua.map.overmind.agents.Add(ember);

            ember.task = ua.task;
            if (ua.task is Task_PerformChallenge performTask)
            {
                performTask.challenge.claimedBy = ember;
            }
            ua.task = null;
        }
    }
}
