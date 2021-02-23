using PregFhNumber.Interfaces;

namespace PregFhNumber.PersonRegistry
{
    public partial class PRPA_MT101310NO01IdentifiedPerson : IIdentifiedPerson
    {
        IPerson IIdentifiedPerson.identifiedPerson
        {
            get { return identifiedPerson; }
            set { identifiedPerson = (PRPA_MT101310NO01Person)value; }
        }
    }
}
