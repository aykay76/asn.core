using System;
using System.Collections.Generic;

namespace asn.core
{
    public class AsnRelativeDistinguishedName : AsnSet
    {
        public AsnRelativeDistinguishedName()
        {

        }

        public static AsnRelativeDistinguishedName Decode(byte[] source, ref int pos)
        {
            AsnRelativeDistinguishedName instance = new AsnRelativeDistinguishedName();

            // skip the 0x31 (SET)
            pos++;

            long length = instance.GetLength(source, ref pos);

            long start = pos;

            // each entry in the set is comprised of an attribute type and value pair
            while (pos < start + length)
            {
                AsnAttributeTypeAndValue tv = AsnAttributeTypeAndValue.Decode(source, ref pos);
                instance.elements.Add(tv);
            }

            return instance;
        }
    }
}