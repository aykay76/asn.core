using System;

namespace asn.core
{
    public class AsnAttributeTypeAndValue : AsnBase
    {
        //  AttributeTypeAndValue ::= SEQUENCE {
        //      type AsnOid.cs,
        //      value    AsnString.cs
        //  }

        public AsnOid type;
        public AsnString value;

        public AsnAttributeTypeAndValue()
        {

        }

        public AsnAttributeTypeAndValue(AsnOid oid, AsnString newValue)
        {
            type = oid;
            value = newValue;
        }

        public AsnAttributeTypeAndValue(string oid, string newValue, AsnType.UniversalTag tag)
        {
            type = new AsnOid(oid);
            value = new AsnString(newValue, tag);
        }

        public int Encode()
        {
            int l = 0;

            l += type.Encode();
            l += value.Encode();

            // TODO: encode self as sequence - constructed
            byte[] lengthBytes = EncodeLength(l);

            derValue = new byte[3 + lengthBytes.Length + l];
            derValue[0] = 0x31;
            derValue[1] = (byte)(l + 2);
            derValue[2] = 0x30;
            Array.Copy(lengthBytes, 0, derValue, 3, lengthBytes.Length);
            Array.Copy(type.derValue, 0, derValue, 3 + lengthBytes.Length, type.derValue.Length);
            Array.Copy(value.derValue, 0, derValue, 3 + lengthBytes.Length + type.derValue.Length, value.derValue.Length);

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnAttributeTypeAndValue Decode(byte[] source, ref int pos)
        {
            AsnAttributeTypeAndValue instance = new AsnAttributeTypeAndValue();
           
            // skip the 0x31
            pos++;

            // sequence tag
            long setLength = instance.GetLength(source, ref pos);

            // skip the 0x30
            pos++;

            long length = instance.GetLength(source, ref pos);

            instance.type = AsnOid.Decode(source, ref pos);
            instance.value = AsnString.Decode(source, ref pos);

            return instance;
        }
    }
}
