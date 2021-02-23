using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace PregFhNumber
{
    public class ClientMessageInspector : IClientMessageInspector
    {
        public string LastRequestXml { get; private set; }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            printSoapRequest(reply);
        }

        public object BeforeSendRequest(ref Message request, System.ServiceModel.IClientChannel channel)
        {
            printSoapRequest(request);

            return request;
        }

        public void printSoapRequest(Message request)
        {
            Console.WriteLine(request);
        }
    }
}
