using System.Collections.Generic;
using System.Linq;

namespace PregFhNumber.PersonRegistry
{
    public partial class AD : ANY
    {
        public AD()
        {
        }

        public AD(IEnumerable<ADXP> items)
            : this()
        {
            this.Items = items.ToArray();
        }

        public AD(IEnumerable<ADXP> items, PostalAddressUse[] usage)
            : this()
        {
            Items = items.ToArray();
            use = usage;
        }
    }
}
