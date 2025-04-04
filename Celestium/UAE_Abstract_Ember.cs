using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celestium
{
    public class UAE_Abstraction_Ember : UAE_Abstraction
    {
        public UAE_Abstraction_Ember(Map map)
            : base(map, -1)
        {

        }

        public override string getName()
        {
            return "An Ember";
        }

        public override string getDesc()
        {
            return "A generaic agent with no particular strengths or weakneses..";
        }

        public override string getFlavour()
        {
            return "A lviing Ember of Celestium. An mote of his brilliant will let loose upon the world.";
        }

        public override string getRestrictions()
        {
            return "Must be created at Celestium";
        }

        public override Sprite getForeground()
        {
            return EventManager.getImg("ILGF_Celestium.Fore_FireAgent.png");
        }

        public override Sprite getBackground()
        {
            return map.world.iconStore.standardBack;
        }

        public override Sprite getBoxImg()
        {
            return base.getBoxImg();
        }

        public override int getStatMight()
        {
            return 3;
        }

        public override int getStatLore()
        {
            return 3;
        }

        public override int getStatIntrigue()
        {
            return 3;
        }

        public override int getStatCommand()
        {
            return 3;
        }

        public override bool validTarget(Location loc)
        {
            return loc.settlement is Set_TombOfGods && loc.settlement.subs.Any(sub => sub is Sub_Celestium);
        }

        public override void createAgent(Location target)
        {
            UAE_Ember ember = new UAE_Ember(target, target.map.soc_dark);

            target.units.Add(ember);
            target.map.units.Add(ember);
            target.map.overmind.agents.Add(ember);
            
            target.map.overmind.availableEnthrallments--;
            ember.person.skillPoints++;
            ember.person.state = Person.personState.enthralled;
            ember.person.shadow = 0.0;
            ember.person.extremeHates.Clear();
            ember.person.extremeHates.Clear();
            ember.person.hates.Clear();
            ember.person.likes.Clear();

            if (!target.map.automatic)
            {
                GraphicalMap.selectedUnit = ember;
                map.world.prefabStore.popAgentLevelUp(ember);
                GraphicalMap.panTo(target.hex);
            }
        }
    }
}
