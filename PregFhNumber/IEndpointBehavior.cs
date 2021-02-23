using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace PregFhNumber
{
    public class CustomInspectorBehavior : IEndpointBehavior
    {
        private readonly ClientMessageInspector clientMessageInspector = new ClientMessageInspector();

        public string LastRequestXml
        {
            get { return clientMessageInspector.LastRequestXml; }
        }

        public string LastResponseXml
        {
            get { return clientMessageInspector.LastRequestXml; }
        }

        public void AddBindingParameters(
            ServiceEndpoint endpoint,
            System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(clientMessageInspector);
        }
    }
}
