using System;
using System.Collections.Generic;

namespace asn.core
{
    public class AsnName : AsnBase
    {
        //  Name                    SEQUENCE
        //      RDN                     SET <List>
        //          TypeandValue        AttributeTypeAndValue.cs
        //              Type            AsnOid.cs
        //              Value           AsnString.cs 

        public List<AsnAttributeTypeAndValue> set;

        public AsnName()
        {
            set = new List<AsnAttributeTypeAndValue>();
        }

        public AsnName(string DN)
        {
            set = new List<AsnAttributeTypeAndValue>();

            string[] parts = DN.Split(',', StringSplitOptions.None);
            foreach (string part in parts)
            {
                string[] sections = part.Split('=', StringSplitOptions.None);
                AsnAttributeTypeAndValue rdn = new AsnAttributeTypeAndValue(AsnOid.FromFriendlyName(sections[0]), new AsnString(sections[1], AsnType.UniversalTag.PrintableString));
                set.Add(rdn);
            }
        }

        public override int Encode()
        {
            int setLength = 0;
            foreach (AsnAttributeTypeAndValue tv in set)
            {
                setLength += tv.Encode();
            }
            byte[] setLengthBytes = EncodeLength(setLength);
            byte[] setBytes = new byte[setLength];

            int pos = 0;

            Array.Copy(setLengthBytes, 0, setBytes, pos, setLengthBytes.Length);
            foreach (AsnAttributeTypeAndValue tv in set)
            {
                Array.Copy(tv.derValue, 0, setBytes, pos, tv.derValue.Length);
                pos += tv.derValue.Length;
            }

            int sequenceLength = setBytes.Length;
            byte[] sequenceLengthBytes = EncodeLength(sequenceLength);

            derValue = new byte[1 + sequenceLengthBytes.Length + setBytes.Length];
            derValue[0] = 0x30; // constructed sequence
            Array.Copy(sequenceLengthBytes, 0, derValue, 1, sequenceLengthBytes.Length);
            Array.Copy(setBytes, 0, derValue, 1 + sequenceLengthBytes.Length, setBytes.Length);

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnName Decode(byte[] source, ref int pos)
        {
            AsnName instance = new AsnName();

            //CheckContextTag(source, ref pos);
            pos++;

            instance.set = new List<AsnAttributeTypeAndValue>();

            // sequence length
            long length = instance.GetLength(source, ref pos);

            // now we should see a set and length of the set
            long start = pos;

            // decode the set
            while (pos < start + length)
            {
                // each entry in the set is comprised of an attribute type and value pair
                AsnAttributeTypeAndValue tv = AsnAttributeTypeAndValue.Decode(source, ref pos);
                instance.set.Add(tv);
            }

            return instance;
        }
    }
}
