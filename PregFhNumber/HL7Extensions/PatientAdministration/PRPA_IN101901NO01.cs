using System.Linq;
using PregFhNumber.Interfaces;

namespace PregFhNumber.PersonRegistry
{
    public partial class PRPA_IN101901NO01 : IRequestMessage
    {
        ISenderOrReceiver IMessage.sender
        {
            get { return sender; }
            set { sender = (MCCI_MT000100UV01Sender)value; }
        }

        ISenderOrReceiver[] IMessage.receiver
        {
            get { return receiver; }
            set { receiver = value == null ? null : value.Cast<MCCI_MT000100UV01Receiver>().ToArray(); }
        }
    }
}
