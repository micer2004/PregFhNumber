using PregFhNumber.Interfaces;

namespace PregFhNumber.PersonRegistry
{
    public partial class MCCI_MT000300UV01Sender : ISenderOrReceiver
    {
        IDevice ISenderOrReceiver.device
        {
            get { return deviceField; }
            set { deviceField = (MCCI_MT000300UV01Device)value; }
        }
    }
}
