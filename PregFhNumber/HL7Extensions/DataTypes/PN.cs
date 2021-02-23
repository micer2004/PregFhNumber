using System.Collections.Generic;
using System.Linq;

namespace PregFhNumber.PersonRegistry
{
    public partial class PN : EN
    {
        public PN()
        {
        }

        public PN(IEnumerable<ENXP> items)
            : this()
        {
            this.Items = items.ToArray();
        }
    }
}
