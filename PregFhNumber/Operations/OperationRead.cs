using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using PregFhNumber.Constants;
using PregFhNumber.PersonRegistry;

namespace PregFhNumber
{
    internal static partial class Operations
    {
        private const string RootNamespace = "urn:hl7-org:v3";

        private static readonly XmlSerializer FindCandidatesRequestSerializer =
            new XmlSerializer(typeof(PRPA_IN101305NO01), new XmlRootAttribute { Namespace = RootNamespace });

        private static readonly XmlSerializer FindCandidatesResponseSerializer =
            new XmlSerializer(typeof(PRPA_IN101306NO01), new XmlRootAttribute { Namespace = RootNamespace });

        private static readonly XmlSerializer GetDemographicsRequestSerializer =
            new XmlSerializer(typeof(PRPA_IN101307NO01), new XmlRootAttribute { Namespace = RootNamespace });

        private static readonly XmlSerializer GetDemographicsResponseSerializer =
            new XmlSerializer(typeof(PRPA_IN101308NO01), new XmlRootAttribute { Namespace = RootNamespace });


        internal static void GetDemographics(PersonRegistryClient client)
        {
            string idNumber = ReadLineAndTrim("Enter id number: ");
            if (string.IsNullOrWhiteSpace(idNumber))
                return;

            var id = new II { root = GetIdOid(idNumber.Trim()), extension = idNumber.Trim() };
            PRPA_IN101307NO01 request = CreateGetDemographicsRequest(id);

            GetDemographicsRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine("\n");
            PRPA_IN101308NO01 response = client.GetDemographicsAsync(request).Result;
            GetDemographicsResponseSerializer.Serialize(Console.Out, response);
            Console.WriteLine("\n");

            string queryResponseCode = response.controlActProcess.queryAck.queryResponseCode.code;
            switch (queryResponseCode)
            {
                case QueryResponseCode.Ok:
                    Console.WriteLine(PersonToString(response.controlActProcess.subject[0].registrationEvent.subject1
                        .identifiedPerson));
                    break;
                case QueryResponseCode.NoResultsFound:
                    Console.WriteLine("No results found");
                    break;
                case QueryResponseCode.QueryParameterError:
                    Console.WriteLine("Query parameter error");
                    break;
                default:
                    Console.WriteLine($"Unrecognized query response code: '{queryResponseCode}'");
                    break;
            }
        }

        internal static void FindCandidates(PersonRegistryClient client)
        {
            var info = ReadPersonalInformation(false, true, false, true);
            var paramList = new PRPA_MT101306NO01ParameterList();

            // Search for people created within specified period
            //var requestCreation = false;
            //var resp = ReadLineAndTrim("Søke på creationTime (J/N): ");
            //if (resp.ToUpper() == "J") requestCreation = true;

            //if (requestCreation)
            //{
            //    var creationFrom = ReadLineAndTrim($"Fra dato ({DateFormat}): ");
            //    var creationTo = ReadLineAndTrim($"Til dato ({DateFormat}): ");

            //    var ds = new PRPA_MT101306NO01QueryByParameter();
            //    ds.
            //}


            var nameItems = CreateNameItems(info);
            var items = nameItems.ToList();
            if (items.Any())
                paramList.personName = CreatePersonNameParameter(items);

            if (IsDateSpecifiedAndValid(info.dateOfBirth))
                paramList.personBirthTime = CreatePersonBirthTimeParameter(info.dateOfBirth);

            if (info.gender != string.Empty)
                paramList.personAdministrativeGender = CreatePersonAdministrativeGenderParameter(info.gender);

            var addressItems = CreateAddressItems(info);
            var addressList = addressItems.ToList();
            if (addressList.Any())
            {
                foreach (var addrItem in addressList)
                {
                    paramList.identifiedPersonAddress = addrItem.Key switch
                    {
                        "private" => CreateIdentifiedPersonAddressParameter(addrItem.Value, PostalAddressUse.H),
                        "temp" => CreateIdentifiedPersonAddressParameter(addrItem.Value, PostalAddressUse.WP),
                        _ => paramList.identifiedPersonAddress
                    };
                }
            }


            var message = SetTopLevelFields(new PRPA_IN101305NO01
            {
                controlActProcess = new PRPA_IN101305NO01QUQI_MT021001UV01ControlActProcess
                {
                    queryByParameter = new PRPA_MT101306NO01QueryByParameter
                    {
                        parameterList = paramList

                    }
                }
            });

            FindCandidatesRequestSerializer.Serialize(Console.Out, message);
            Console.WriteLine("\n");
            PRPA_IN101306NO01 result = client.FindCandidatesAsync(message).Result;
            FindCandidatesResponseSerializer.Serialize(Console.Out, result);
            Console.WriteLine("\n");

            Console.WriteLine("Found {0} persons:", result.controlActProcess.queryAck.resultTotalQuantity.value);
            if (result.controlActProcess.subject != null)
                foreach (var subject in result.controlActProcess.subject)
                    Console.WriteLine(PersonToString(subject.registrationEvent.subject1.identifiedPerson));
        }

        private static PRPA_IN101307NO01 CreateGetDemographicsRequest(II id)
        {
            return SetTopLevelFields(new PRPA_IN101307NO01
            {
                controlActProcess = new PRPA_IN101307NO01QUQI_MT021001UV01ControlActProcess
                {
                    queryByParameter = new PRPA_MT101307UV02QueryByParameter
                    {
                        parameterList = new PRPA_MT101307UV02ParameterList
                        {
                            identifiedPersonIdentifier = new[]
                            {
                                new PRPA_MT101307UV02IdentifiedPersonIdentifier {value = new[] {id}}
                            }
                        }
                    }
                }
            });
        }

        private static PRPA_MT101306NO01PersonName[] CreatePersonNameParameter(IEnumerable<ENXP> nameItems)
        {
            return new[] { new PRPA_MT101306NO01PersonName { value = new[] { new PN { Items = nameItems.ToArray() } } } };
        }

        private static PRPA_MT101306NO01IdentifiedPersonAddress[] CreateIdentifiedPersonAddressParameter(
            IEnumerable<ADXP> addressItems, PostalAddressUse typeAddress)
        {
            return new[]
            {
                new PRPA_MT101306NO01IdentifiedPersonAddress
                    {value = new[] {new AD {Items = addressItems.ToArray(), use = new[] {typeAddress}}}}
            };
        }

        private static PRPA_MT101306NO01PersonBirthTime[] CreatePersonBirthTimeParameter(string dateOfBirth)
        {
            return new[] { new PRPA_MT101306NO01PersonBirthTime { value = new[] { new IVL_TS { value = dateOfBirth } } } };
        }

        private static PRPA_MT101306NO01PersonAdministrativeGender[] CreatePersonAdministrativeGenderParameter(string genderValue)
        {
            return new[]
            {
                new PRPA_MT101306NO01PersonAdministrativeGender
                    {value = new[] {new CE {code = genderValue, codeSystem = IdGenderOid.HL7}}}
            };
        }

    }
}
