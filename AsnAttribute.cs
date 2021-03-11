using System;
using System.Collections.Generic;

namespace asn.core
{
    public class AsnAttribute : AsnSequence
    {
        // Attribute ::= SEQUENCE 
        // {
        //   type               OBJECT IDENTIFIER,
        //   values             AttributeSetValue
        // }

        public AsnAttribute() : base()
        {

        }

        public static AsnAttribute Decode(byte[] source, ref int pos)
        {
            AsnAttribute instance = new AsnAttribute();

            // TODO: do this.

            return instance;
        }
    }
}