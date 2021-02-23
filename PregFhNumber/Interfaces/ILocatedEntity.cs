using PregFhNumber.PersonRegistry;

namespace PregFhNumber.Interfaces
{
    public interface ILocatedEntity
    {
        CS[] realmCode { get; set; }
        II typeId { get; set; }
        II[] templateId { get; set; }
        IPlace location { get; set; }
        NullFlavor nullFlavor { get; set; }
        RoleClassLocatedEntity classCode { get; set; }
    }
}
