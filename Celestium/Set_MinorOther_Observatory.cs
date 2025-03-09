using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celestium
{
    public class Set_MinorOther_Observatory : Set_MinorOther
    {
        public Set_MinorOther_Observatory(Location loc)
            : base(loc)
        {

        }

        public override void fallIntoRuin(string v, object killer = null)
        {
            List<Subsettlement> ruinedSubsettlements = new List<Subsettlement>();
            foreach (Subsettlement sub in subs)
            {
                if (!sub.survivesRuin())
                {
                    ruinedSubsettlements.Add(sub);
                }
            }
            foreach (Subsettlement sub in ruinedSubsettlements)
            {
                subs.Remove(sub);
            }

            List<Property> ruinedProperties = new List<Property>();
            foreach (Property property in location.properties)
            {
                if (property.removedOnRuin())
                {
                    ruinedProperties.Add(property);
                }
            }
            foreach (Property property in ruinedProperties)
            {
                location.properties.Remove(property);
            }
        }
    }
}
