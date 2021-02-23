using System;
using System.Linq;
using System.Xml.Serialization;
using PregFhNumber.PersonRegistry;

namespace PregFhNumber
{
    internal static partial class Operations
    {
        private static readonly XmlSerializer AddPersonOrRevisePersonRecordResponseSerializer =
            new XmlSerializer(typeof(PRPA_IN101319NO01), new XmlRootAttribute { Namespace = RootNamespace });

        private static readonly XmlSerializer RevisePersonRecordRequestSerializer =
            new XmlSerializer(typeof(PRPA_IN101314NO01), new XmlRootAttribute { Namespace = RootNamespace });

        private static readonly XmlSerializer LinkPersonRecordsRequestSerializer =
            new XmlSerializer(typeof(PRPA_IN101901NO01), new XmlRootAttribute { Namespace = RootNamespace });

        private static readonly XmlSerializer UnlinkPersonRecordsRequestSerializer =
            new XmlSerializer(typeof(PRPA_IN101911NO01), new XmlRootAttribute { Namespace = RootNamespace });

        private static readonly XmlSerializer AcknowledgementSerializer =
            new XmlSerializer(typeof(MCAI_IN000004NO01), new XmlRootAttribute { Namespace = RootNamespace });

        internal static void RevisePersonRecord(PersonRegistryClient client)
        {
            var info = ReadPersonalInformation(true, true, false, false);

            var outerPerson = new PRPA_MT101302NO01IdentifiedPerson
            {
                id = new[] { new II { root = GetIdOid(info.fhNumber), extension = info.fhNumber } },
                identifiedPerson = new PRPA_MT101302NO01Person()
            };

            var nameItems = CreateNameItems(info);
            var items = nameItems.ToList();
            if (items.Any())
                outerPerson.identifiedPerson.name = new[] { new PN(items) };

            if (IsDateSpecifiedAndValid(info.dateOfBirth))
                outerPerson.identifiedPerson.birthTime = new TS(info.dateOfBirth);

            var addressItems = CreateAddressItems(info);
            var enumerable = addressItems.ToList();
            if (enumerable.Any())
                foreach (var addrItem in enumerable)
                {
                    outerPerson.identifiedPerson.addr = new[] { new AD(addrItem.Value) };

                }


            if (info.gender != "")
                outerPerson.identifiedPerson.administrativeGenderCode = CreateAdministrativeGenderCode(info.gender);

            var request = SetTopLevelFields(new PRPA_IN101314NO01
            {
                controlActProcess = new PRPA_IN101314NO01MFMI_MT700721UV01ControlActProcess
                {
                    subject = new PRPA_IN101314NO01MFMI_MT700721UV01Subject1
                    {
                        registrationRequest = new PRPA_IN101314NO01MFMI_MT700721UV01RegistrationRequest
                        {
                            subject1 = new PRPA_IN101314NO01MFMI_MT700721UV01Subject2
                            {
                                identifiedPerson = outerPerson
                            }
                        }
                    }
                }
            });

            RevisePersonRecordRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine("\n");
            PRPA_IN101319NO01 response = client.RevisePersonRecordAsync(request).Result;
            AddPersonOrRevisePersonRecordResponseSerializer.Serialize(Console.Out, response);
            Console.WriteLine();
        }

        internal static void LinkPersonRecords(PersonRegistryClient client)
        {
            Console.Write("Enter obsolete FH-number: ");
            string obsoleteFhNumber = Console.ReadLine();
            Console.Write("Enter surviving ID-number or FH-number: ");
            string survivingIdNumberOrFhNumber = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(obsoleteFhNumber) || string.IsNullOrWhiteSpace(survivingIdNumberOrFhNumber))
                return;

            var request = SetTopLevelFields(new PRPA_IN101901NO01
            {
                controlActProcess = new PRPA_IN101901NO01MFMI_MT700721UV01ControlActProcess
                {
                    subject = new PRPA_IN101901NO01MFMI_MT700721UV01Subject1
                    {
                        registrationRequest = new PRPA_IN101901NO01MFMI_MT700721UV01RegistrationRequest
                        {
                            subject1 = new PRPA_IN101901NO01MFMI_MT700721UV01Subject2
                            {
                                identifiedPerson = new PRPA_MT101901NO01IdentifiedPerson
                                {
                                    id = new[] { new II(GetIdOid(survivingIdNumberOrFhNumber), survivingIdNumberOrFhNumber) },
                                    identifiedBy = new[] {
                                        new PRPA_MT101901NO01SourceOf2 {
                                            //TODO: Is the value of statusCode important?
                                            otherIdentifiedPerson = new PRPA_MT101901NO01OtherIdentifiedPerson {
                                                id = new II(GetIdOid(obsoleteFhNumber), obsoleteFhNumber)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            LinkPersonRecordsRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine("\n");
            MCAI_IN000004NO01 response = client.LinkPersonRecordsAsync(request).Result;
            AcknowledgementSerializer.Serialize(Console.Out, response);
            Console.WriteLine();
        }

        internal static void UnlinkPersonRecords(PersonRegistryClient client)
        {
            Console.Write("Enter child FH-number: ");
            string obsoleteFhNumber = Console.ReadLine();
            Console.Write("Enter parent ID-number or FH-number: ");
            string survivingIdNumberOrFhNumber = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(obsoleteFhNumber) || string.IsNullOrWhiteSpace(survivingIdNumberOrFhNumber))
                return;

            var request = SetTopLevelFields(new PRPA_IN101911NO01
            {
                controlActProcess = new PRPA_IN101911NO01MFMI_MT700721UV01ControlActProcess
                {
                    subject = new PRPA_IN101911NO01MFMI_MT700721UV01Subject1
                    {
                        registrationRequest = new PRPA_IN101911NO01MFMI_MT700721UV01RegistrationRequest
                        {
                            subject1 = new PRPA_IN101911NO01MFMI_MT700721UV01Subject2
                            {
                                identifiedPerson = new PRPA_MT101911NO01IdentifiedPerson
                                {
                                    id = new[] { new II(GetIdOid(survivingIdNumberOrFhNumber), survivingIdNumberOrFhNumber) },
                                    identifiedBy = new PRPA_MT101911NO01SourceOf2
                                    {
                                        //TODO: Is the value of statusCode important?
                                        otherIdentifiedPerson = new PRPA_MT101911NO01OtherIdentifiedPerson
                                        {
                                            id = new II(GetIdOid(obsoleteFhNumber), obsoleteFhNumber)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            UnlinkPersonRecordsRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine("\n");
            MCAI_IN000004NO01 response = client.UnlinkPersonRecordsAsync(request).Result;
            AcknowledgementSerializer.Serialize(Console.Out, response);
            Console.WriteLine();
        }
    }
}
