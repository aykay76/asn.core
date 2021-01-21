using System;

namespace asn.core
{
    public class AsnValidity : AsnBase
    {
        public AsnGeneralizedTime notBefore;
        public AsnGeneralizedTime notAfter;

        public int Encode()
        {
            int length = 0;

            length += notBefore.Encode();
            length += notAfter.Encode();

            byte[] lengthBytes = EncodeLength(length);
            derValue = new byte[1 + lengthBytes.Length + length];

            derValue[0] = 0x30; // sequence

            int d = 1;
            Array.Copy(lengthBytes, 0, derValue, d, lengthBytes.Length);
            d += lengthBytes.Length;
            Array.Copy(notBefore.derValue, 0, derValue, d, notBefore.derValue.Length);
            d += notBefore.derValue.Length;
            Array.Copy(notAfter.derValue, 0, derValue, d, notAfter.derValue.Length);

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnValidity Decode(byte[] source, ref int pos)
        {
            AsnValidity instance = new AsnValidity();

            int start = pos;

            pos++;

            int len = instance.GetLength(source, ref pos);

            instance.notBefore = AsnGeneralizedTime.Decode(source, ref pos);
            instance.notAfter = AsnGeneralizedTime.Decode(source, ref pos);

            instance.originalDer = new byte[pos - start];
            Array.Copy(source, start, instance.originalDer, 0, pos - start);

            return instance;
        }
    }
}