using System;
using System.Collections.Generic;

namespace asn.core
{
    public class AsnSequence : AsnBase
    {
        public AsnSequence()
        {
            elements = new List<AsnBase>();
        }

        public override int Encode()
        {
            int length = 0;

            foreach (AsnBase child in elements)
            {
                length += child.Encode();
            }

            byte[] lengthBytes = EncodeLength(length);
            derValue = new byte[1 + lengthBytes.Length + length];
            derValue[0] = 0x30; // sequence
            int d = 1;
            Array.Copy(lengthBytes, 0, derValue, d, lengthBytes.Length);
            d += lengthBytes.Length;

            foreach (AsnBase child in elements)
            {
                Array.Copy(child.derValue, 0, derValue, d, child.derValue.Length);
                d += child.derValue.Length;
            }

            PrependContextTag();

            return derValue.Length;
        }
    }
}