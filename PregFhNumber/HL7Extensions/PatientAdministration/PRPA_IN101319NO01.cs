using System.Linq;
using PregFhNumber.Interfaces;

namespace PregFhNumber.PersonRegistry
{
    public partial class PRPA_IN101319NO01 : IResponseMessage
    {
        ISenderOrReceiver IMessage.sender
        {
            get { return sender; }
            set { sender = (MCCI_MT000300UV01Sender)value; }
        }

        ISenderOrReceiver[] IMessage.receiver
        {
            get { return receiver; }
            set { receiver = value == null ? null : value.Cast<MCCI_MT000300UV01Receiver>().ToArray(); }
        }
    }
}
