using PregFhNumber.Interfaces;

namespace PregFhNumber.PersonRegistry
{
    public partial class MCCI_MT000300UV01LocatedEntity : ILocatedEntity
    {
        IPlace ILocatedEntity.location
        {
            get { return locationField; }
            set { locationField = (MCCI_MT000300UV01Place)value; }
        }
    }
}
