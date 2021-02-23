using System;
using System.Globalization;
using System.Linq;
using System.Text;

using PregFhNumber.Constants;
using PregFhNumber.Interfaces;
using PregFhNumber.PersonRegistry;

namespace PregFhNumber
{
    internal static partial class Operations
    {
        private const string DateFormat = "yyyyMMdd";
        private static CS _processingCode = ProcessingCode.Test();

        internal static void ChangeProcessingCode()
        {
            Console.Write("Please choose (P)roduction, (T)est, or (D)ebugging: ");
            while (true)
            {
                string code = (Console.ReadLine() ?? "").ToUpper();
                if (code == "P")
                {
                    _processingCode = ProcessingCode.Production();
                    return;
                }
                if (code == "T")
                {
                    _processingCode = ProcessingCode.Test();
                    break;
                }
                if (code == "D")
                {
                    _processingCode = ProcessingCode.Debugging();
                    break;
                }
            }
        }

        private static PersonalInformation ReadPersonalInformation(bool askForIdNumber, bool askForGender,
            bool askForTelecom, bool askForPassnr)
        {
            var info = new PersonalInformation();
            if (askForIdNumber)
                info.fhNumber = ReadLineAndTrim("FH-number: ");
            info.firstName = ReadLineAndTrim("Fornavn(er): ");
            info.middleName = ReadLineAndTrim("Mellomnavn(er): ");
            info.lastName = ReadLineAndTrim("Etternavn(er): ");
            info.dateOfBirth = ReadLineAndTrim(string.Format($"Fødselsdato ({DateFormat}): "));
            info.streetAddressLineHome = ReadLineAndTrim("Hjemmeadresse - gate: ");
            info.cityHome = ReadLineAndTrim("Hjemmeadresse - by: ");
            info.zipCodeHome = ReadLineAndTrim("Hjemmeadresse - postkode: ");
            info.landCodeHome = ReadLineAndTrim("Hjemmeadresse - landkode: ");
            info.streetAddressLineTemp = ReadLineAndTrim("Kontaktadresse i Norge - gate: ");
            info.cityTemp = ReadLineAndTrim("Kontaktadresse i Norge - by: ");
            info.countyTemp = ReadLineAndTrim("Konntakadresse i Norge - oppholdskommune: ");
            info.zipCodeTemp = ReadLineAndTrim("Kontaktadresse i Norge - postkode: ");
            if (askForGender)
                info.gender = ReadLineAndTrim("Gender (M/F): ");
            if (askForTelecom)
            {
                info.mobile = "tel:" + ReadLineAndTrim("Mobilnr: ");
                info.epostPrivate = "mailto:" + ReadLineAndTrim("Privat e-post adresse: ");
                info.epostWork = "mailto:" + ReadLineAndTrim("Jobb e-post adresse: ");
            }

            if (askForPassnr)
            {
                info.passNr = ReadLineAndTrim("Pass nummer: ");
                info.passNrIssuerNationality = ReadLineAndTrim("Pass utsteder: ");
            }


            return info;
        }

        private static string PersonToString(IIdentifiedPerson identifiedPerson)
        {
            var sb = new StringBuilder();
            sb.Append(identifiedPerson.id[0].extension);
            sb.Append(": ");

            IPerson person = identifiedPerson.identifiedPerson;

            if (person.name != null && person.name.Length > 0 && person.name[0].Items != null)
                sb.Append(string.Join(" ", person.name[0].Items.Select(ni => ni.Text[0])));
            else
                sb.Append("(no name)");

            if (person.administrativeGenderCode != null)
            {
                sb.Append("; Gender: ");
                sb.Append(person.administrativeGenderCode.code);
            }

            if (person.birthTime != null)
            {
                sb.Append("; Date of birth: ");
                sb.Append(person.birthTime.value);
            }

            if (person.addr != null && person.addr.Length > 0)
            {
                sb.Append("; Address: ");
                sb.Append(string.Join(" ",
                    person.addr[0].Items.Select(ai => ai.Text != null && ai.Text.Length > 0 ? ai.Text[0] : "")));
            }

            return sb.ToString();
        }

        private static string ReadLineAndTrim(string message)
        {
            Console.Write(message);
            return (Console.ReadLine() ?? "").Trim();
        }

        private static TMessage SetTopLevelFields<TMessage>(TMessage message)
            where TMessage : IRequestMessage
        {
            message.id = CreateMessageId();
            message.interactionId = new II("2.16.840.1.113883.1.6", typeof(TMessage).Name);
            message.processingCode = _processingCode;
            message.processingModeCode = new CS("T");
            message.versionCode = new CS("NE2010NO");
            message.receiver = new ISenderOrReceiver[]
            {
                new MCCI_MT000100UV01Receiver
                {
                    typeCode = CommunicationFunctionType.RCV,
                    device = new MCCI_MT000100UV01Device
                    {
                        classCode = EntityClassDevice.DEV,
                        determinerCode = EntityDeterminerSpecific.INSTANCE,
                        id = new[]
                        {
                            new II("2.16.578.1.12.4.5.1.1", null),
                        }
                    }
                }
            };
            return message;
        }

        private static II CreateMessageId()
        {
            return new II("1.2.3.4", Guid.NewGuid().ToString());
        }

        

        private static string GetIdOid(string idNumber)
        {
            return idNumber[0] >= '8' ? IdNumberOid.FhNumber :
                idNumber[0] >= '4' ? IdNumberOid.DNumber : IdNumberOid.FNumber;
        }

        private static string GetNationalityOid(string issuerNationality)
        {
            if (issuerNationality == "NOR")
                return PassportNationalityOid.passportNumNS_NOR;
            if (issuerNationality == "POL")
                return PassportNationalityOid.passportNumNS_POL;
            if (issuerNationality == "USA")
                return PassportNationalityOid.passportNumNS_USA;
            return PassportNationalityOid.passportNumNS_IDN;
            //todo: implement better decision-logic
        }

        private static bool IsDateSpecifiedAndValid(string date)
        {
            if (date == "")
                return false;

            DateTime dummy;
            if (DateTime.TryParseExact(date, DateFormat, null, DateTimeStyles.None, out dummy))
                return true;

            Console.WriteLine("Warning: Date of birth is illegal; skipping");
            return false;
        }

        private static bool AppendChecksum(ref string number)
        {
            var e = number.Select(c => c - '0').ToArray();
            var k1 = 11 - ((3 * e[0] + 7 * e[1] + 6 * e[2] + 1 * e[3] + 8 * e[4] + 9 * e[5] + 4 * e[6] + 5 * e[7] + 2 * e[8]) % 11);
            k1 = (11 == k1) ? 0 : k1;
            var k2 = 11 - ((5 * e[0] + 4 * e[1] + 3 * e[2] + 2 * e[3] + 7 * e[4] + 6 * e[5] + 5 * e[6] + 4 * e[7] + 3 * e[8] + 2 * k1) % 11);
            k2 = (11 == k2) ? 0 : k2;
            if (k1 == 10 || k2 == 10)
                return false;
            number = number + k1 + k2;
            return true;
        }
    }
}
