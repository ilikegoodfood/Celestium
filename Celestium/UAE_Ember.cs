using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class UAE_Ember : UAE
    {
        public UAE_Ember(Location loc, Society sg)
            : base(loc, sg)
        {
            person.stat_might = 3;
            person.stat_lore = 3;
            person.stat_intrigue = 3;
            person.stat_command = 3;
            person.isMale = true;
            person.species = map.species_monster;

            corrupted = true;
        }

        public override bool definesName()
        {
            return true;
        }

        public override string getName()
        {
            return "An Ember";
        }

        public override Sprite getPortraitForeground()
        {
            return EventManager.getImg("ILGF_Celestium.Fore_FireAgent.png");
        }

        public override Sprite getPortraitBackground()
        {
            return map.world.iconStore.standardBack;
        }

        public override bool isCommandable()
        {
            return corrupted || person.state == Person.personState.enthralled || person.traits.Any(t => t.grantsCommand());
        }

        public override bool hasStartingTraits()
        {
            return false;
        }

        public override int[] getPositiveTags()
        {
            return new int[0];
        }
    }
}
