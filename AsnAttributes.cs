using System;
using System.Collections.Generic;

namespace asn.core
{
    public class AsnAttributes : AsnSet
    {
        // Attributes ::= SET OF Attribute

        public AsnAttributes()
        {
            this.contextTag = 0;
        }

        public static AsnAttributes Decode(byte[] source, ref int pos)
        {
            AsnAttributes instance = new AsnAttributes();

            // TODO: do this.

            return instance;
        }
    }
}