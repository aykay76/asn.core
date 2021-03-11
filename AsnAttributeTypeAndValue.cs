using System;

namespace asn.core
{
    public class AsnAttributeTypeAndValue : AsnSequence
    {
        //  AttributeTypeAndValue ::= SEQUENCE {
        //      type AsnOid.cs,
        //      value    AsnString.cs
        //  }

        // TODO: add these back in as refs if needed, otherwise just use the elements list
        // public AsnOid type;
        // public AsnString value;

        public AsnAttributeTypeAndValue()
        {

        }

        public AsnAttributeTypeAndValue(AsnOid oid, AsnString newValue)
        {
            elements.Add(oid);
            elements.Add(newValue);
        }

        public AsnAttributeTypeAndValue(string oid, string newValue, AsnType.UniversalTag tag)
        {
            elements.Add(new AsnOid(oid));
            elements.Add(new AsnString(newValue, tag));
        }

        public static AsnAttributeTypeAndValue Decode(byte[] source, ref int pos)
        {
            AsnAttributeTypeAndValue instance = new AsnAttributeTypeAndValue();

            // skip the 0x30 (SEQUENCE)
            pos++;           

            long length = instance.GetLength(source, ref pos);

            instance.elements.Add(AsnOid.Decode(source, ref pos));
            instance.elements.Add(AsnString.Decode(source, ref pos));

            return instance;
        }
    }
}
