using PregFhNumber.PersonRegistry;

namespace PregFhNumber.Interfaces
{
    /// <summary>
    /// This interface abstracts out the common parts of the nearly-equal PRPA_MT101303UV02IdentifiedPerson and PRPA_MT101310UV02IdentifiedPerson.
    /// </summary>
    public interface IIdentifiedPerson
    {
        II[] id { get; set; }
        IPerson identifiedPerson { get; set; }
    }
}
