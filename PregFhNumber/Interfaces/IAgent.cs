using PregFhNumber.PersonRegistry;

namespace PregFhNumber.Interfaces
{
    public interface IAgent
    {
        CS[] realmCode { get; set; }
        II typeId { get; set; }
        II[] templateId { get; set; }
        IOrganization representedOrganization { get; set; }
        NullFlavor nullFlavor { get; set; }
        RoleClassAgent classCode { get; set; }
    }
}
