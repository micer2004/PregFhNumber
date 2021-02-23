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

        private static readonly XmlSerializer AddPersonRequestSerializer =
            new XmlSerializer(typeof(PRPA_IN101311NO01), new XmlRootAttribute { Namespace = RootNamespace });

        internal static void AddPerson(PersonRegistryClient client)
        {
            var info = ReadPersonalInformation(false, true, true, true);

            var innerPerson = new PRPA_MT101311NO01Person();


            // Registrerer navn
            var nameItems = CreateNameItems(info);
            var enumerable = nameItems.ToList();
            if (enumerable.Any())
                innerPerson.name = new[] { new PN(enumerable) };

            // Registrerer fødselsdato
            if (IsDateSpecifiedAndValid(info.dateOfBirth))
                innerPerson.birthTime = new TS(info.dateOfBirth);
            
            // Registrerer adresser
            var addressItems = CreateAddressItems(info);
            var items = addressItems.ToList();
            if (items.Any())
            {
                var addressList = new List<AD>();

                foreach (var (key, value) in items)
                {
                    switch (key)
                    {
                        case "private":
                            addressList.Add(new AD(value, new[] { PostalAddressUse.H }));
                            break;
                        case "temp":
                            addressList.Add(new AD(value, new[] { PostalAddressUse.TMP }));
                            break;
                    }
                }
                innerPerson.addr = addressList.ToArray();
            }

            // Registrerer kjønn
            if (info.gender != "")
                innerPerson.administrativeGenderCode = CreateAdministrativeGenderCode(info.gender);

            // Registrerer kontaktmåte - mobil
            var telecomItems = CreateTelecomItems(info);
            var tel = telecomItems.ToList();
            if (tel.Any())
            {
                innerPerson.telecom = tel.ToArray();
            }

            // OtherID
            if (info.passNr != "")
            {
                innerPerson.asOtherIDs = new[]
                {
                    new PRPA_MT101311NO01OtherIDs()
                    {
                        id = new[] {new II {root = GetNationalityOid(info.passNrIssuerNationality), extension = info.passNr}}
                    }
                };
            }

            var request = SetTopLevelFields(new PRPA_IN101311NO01
            {
                controlActProcess = new PRPA_IN101311NO01MFMI_MT700721UV01ControlActProcess
                {
                    subject = new PRPA_IN101311NO01MFMI_MT700721UV01Subject1
                    {
                        registrationRequest = new PRPA_IN101311NO01MFMI_MT700721UV01RegistrationRequest
                        {
                            subject1 = new PRPA_IN101311NO01MFMI_MT700721UV01Subject2
                            {
                                identifiedPerson = new PRPA_MT101311NO01IdentifiedPerson
                                {
                                    identifiedPerson = innerPerson
                                }
                            }
                        }
                    }
                }
            });

            AddPersonRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine("\n");
            PRPA_IN101319NO01 response = client.AddPersonAsync(request).Result;
            AddPersonOrRevisePersonRecordResponseSerializer.Serialize(Console.Out, response);
            Console.WriteLine("\n");

            var pathToFirstNull = new List<string>();
            var subject = NullSafeObjectPathTraverser.Traverse(response, r => r.controlActProcess.subject, pathToFirstNull);
            
            if (subject == null || subject.Length <= 0) return;
            var id = NullSafeObjectPathTraverser.Traverse(subject[0], s => s.registrationEvent.subject1.identifiedPerson.id, pathToFirstNull);
            if (id != null && id.Length > 0)
                Console.WriteLine("The person has been given the FH-number " + id[0].extension);
        }

        private static IEnumerable<ENXP> CreateNameItems(PersonalInformation info)
        {
            var nameItems = new List<ENXP>();
            nameItems.AddRange(info.firstName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(fn => new engiven(fn)));
            nameItems.AddRange(info.middleName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(mn => new enfamily(mn, true)));
            nameItems.AddRange(info.lastName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ln => new enfamily(ln)));
            return nameItems;
        }

        private static CE CreateAdministrativeGenderCode(string infoGender)
        {
            return new CE { code = infoGender, codeSystem = IdGenderOid.HL7 };
        }

        private static IEnumerable<KeyValuePair<string, IEnumerable<ADXP>>> CreateAddressItems(PersonalInformation info)
        {
            var addressItems = new List<KeyValuePair<string, IEnumerable<ADXP>>>();

            if ((info.streetAddressLineHome != "") || (info.cityHome != "") || (info.landCodeHome != "") ||
                (info.zipCodeHome != ""))
            {
                var addressItemsHome = new List<ADXP>
                {
                    (info.cityHome != "") ? new adxpcity {Text = new[] {info.cityHome}} : null,
                    (info.streetAddressLineHome != "")
                        ? new adxpstreetAddressLine {Text = new[] {info.streetAddressLineHome}}
                        : null,
                    (info.zipCodeHome != "") ? new adxppostalCode {Text = new[] {info.zipCodeHome}} : null,
                    (info.landCodeHome != "") ? new adxpcountry {Text = new[] {info.landCodeHome}} : null
                };
                addressItems.Add(new KeyValuePair<string, IEnumerable<ADXP>>("private", addressItemsHome));
            }

            if ((info.streetAddressLineTemp != "") || (info.cityTemp != "") || (info.countyTemp != "") ||
                (info.zipCodeTemp != ""))
            {
                var addressItemsTemp = new List<ADXP>
                {
                    (info.cityTemp != "") ? new adxpcity {Text = new[] {info.cityTemp}} : null,
                    (info.streetAddressLineTemp != "")
                        ? new adxpstreetAddressLine {Text = new[] {info.streetAddressLineTemp}}
                        : null,
                    (info.countyTemp != "") ? new adxpcounty {Text = new[] {info.countyTemp}} : null,
                    (info.zipCodeTemp != "") ? new adxppostalCode {Text = new[] {info.zipCodeTemp}} : null,
                };
                addressItems.Add(new KeyValuePair<string, IEnumerable<ADXP>>("temp", addressItemsTemp));
            }

            return addressItems;
        }

        private static IEnumerable<TEL> CreateTelecomItems(PersonalInformation info)
        {
            var telecomItems = new List<TEL>();
            if (info.mobile != "tel:")
                telecomItems.Add(new TEL { value = info.mobile, use = new[] { TelecommunicationAddressUse.MC }, nullFlavor = NullFlavor.NI, nullFlavorSpecified = true});
            if (info.epostPrivate != "mailto:")
                telecomItems.Add(new TEL { value = info.epostPrivate, use = new[] { TelecommunicationAddressUse.H } });
            if (info.epostWork != "mailto:")
                telecomItems.Add(new TEL { value = info.epostWork, use = new[] { TelecommunicationAddressUse.WP } });

            return telecomItems;
        }
    }
}
