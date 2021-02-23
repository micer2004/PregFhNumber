namespace PregFhNumber.Constants
{
    public static class IdNumberOid
    {
        public const string FNumber = "2.16.578.1.12.4.1.4.1";  // "Fødselsnummer"
        public const string DNumber = "2.16.578.1.12.4.1.4.2";  // "D-nummer"
        public const string FhNumber = "2.16.578.1.12.4.1.4.3"; // "Felles hjelpenummer"
    }

    public static class IdGenderOid
    {
        public const string Volven = "2.16.578.1.12.4.1.1.3101"; // HL7 Gender OID
        public const string HL7 = "2.16.840.1.113883.5.1"; // HL7 Gender OID
    }
}
