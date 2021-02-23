using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Security;
using Microsoft.Extensions.Configuration;

using PregFhNumber.PersonRegistry;


namespace PregFhNumber
{
    class Program
    {
        private static IConfiguration _configuration;

        static void Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();


            var client = CreateClient();
            while (true)
            {
                try
                {
                    Console.Write(
                        "\nWould you like to (F)indCandidates, (G)etDemographics, (A)ddPerson, (R)evisePersonRecord, (L)inkPersonRecords, (U)nlinkPersonRecords, change (P)rocessing code, run performance (T)est, or (E)xit? ");
                    var action = (Console.ReadLine() ?? "E").Trim().ToUpper();
                    if (action == "F")
                        Operations.FindCandidates(client);
                    else if (action == "G")
                        Operations.GetDemographics(client);
                    else if (action == "A")
                        Operations.AddPerson(client);
                    else if (action == "R")
                        Operations.RevisePersonRecord(client);
                    else if (action == "L")
                        Operations.LinkPersonRecords(client);
                    else if (action == "U")
                        Operations.UnlinkPersonRecords(client);
                    else if (action == "P")
                        Operations.ChangeProcessingCode();
                    else if (action == "T")
                        Operations.RunPerformanceTest(client);
                    else if (action == "E")
                        break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    if (e is EndpointNotFoundException && e.InnerException is WebException)
                    {
                        if (e.InnerException.Message == "The remote server returned an error: (404) Not Found.")
                            Console.WriteLine(
                                "\n\n=== An error occurred when connecting to the server: the server was found, but the service endpoint was not found. This probably means that the URL in the config file is wrong. ===\n");
                        else if (e.InnerException.Message.StartsWith("The remote name could not be resolved: "))
                            Console.WriteLine(
                                "\n\n=== An error occurred when connecting to the server: the server was not found. This probably means that the URL in the config file is wrong. ===\n");
                    }
                    else if (e is MessageSecurityException && e.InnerException is FaultException)
                    {
                        if (e.InnerException.Message == "An error occurred when verifying security for the message.")
                            Console.WriteLine(
                                "\n\n=== An error occurred when establishing a secure connection. A possible reason for this is that your local machine clock is out of synch with NHN's server clock. Please contact NHN and ask what the server time is on the server you're trying to connect to. ===\n");
                        else if (e.InnerException.Message ==
                                 "At least one security token in the message could not be validated.")
                            Console.WriteLine(
                                "\n\n=== An error occurred when authenticating. This probably means that the username and/or password in the config file are wrong. ===\n");
                    }

                    client.Abort();
                    ((IDisposable) client).Dispose();
                    client = CreateClient();
                }
            }

            client.Abort();
            ((IDisposable) client).Dispose();
        }

        internal static PersonRegistryClient CreateClient()
        {
            var client = new PersonRegistryClient
            {
                Endpoint =
                {
                    Address = new EndpointAddress(
                        new Uri(_configuration["ConnectionStrings:PregHl7Uri"]),
                        new DnsEndpointIdentity(_configuration["ConnectionStrings:PregHl7Dns"])),
                    Binding = new WSHttpBinding(SecurityMode.TransportWithMessageCredential)
                    {
                        Security =
                        {
                            Message = new NonDualMessageSecurityOverHttp
                            {
                                ClientCredentialType = MessageCredentialType.UserName
                            },
                            Transport = new HttpTransportSecurity
                            {
                                ClientCredentialType = HttpClientCredentialType.None
                            }
                        },
                        AllowCookies = true,
                        MaxReceivedMessageSize = int.MaxValue,
                        MaxBufferPoolSize = int.MaxValue,
                        CloseTimeout = new TimeSpan(0, 1, 0),
                        OpenTimeout = new TimeSpan(0, 1, 0),
                        ReceiveTimeout = new TimeSpan(0, 10, 0),
                        SendTimeout = new TimeSpan(0, 1, 0)
                    }
                },
                ClientCredentials =
                {
                    UserName =
                    {
                        UserName = _configuration["ClientCredentials:Username"],
                        Password = _configuration["ClientCredentials:Password"]
                    }
                }
            };

            //var requestInterceptor = new CustomInspectorBehavior();
            //client.Endpoint.EndpointBehaviors.Add(requestInterceptor);

            return client;
        }
    }
}