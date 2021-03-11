using System;
using System.Collections.Generic;

namespace asn.core
{
    public class AsnExtensions : AsnSequence
    {
        public List<AsnExtension> extensions;

        public AsnExtensions()
        {
            extensions = new List<AsnExtension>();
        }

        public override int Encode()
        {
            int length = 0;

            foreach (AsnExtension extension in extensions)
            {
                length += extension.Encode();
            }

            byte[] lengthBytes = EncodeLength(length);
            derValue = new byte[1 + lengthBytes.Length + length];

            derValue[0] = 0x30; // sequence

            int d = 1;
            Array.Copy(lengthBytes, 0, derValue, d, lengthBytes.Length);
            d += lengthBytes.Length;

            foreach (AsnExtension extension in extensions)
            {
                Array.Copy(extension.derValue, 0, derValue, d, extension.derValue.Length);
                d += extension.derValue.Length;
            }

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnExtensions Decode(byte[] source, ref int pos)
        {
            AsnExtensions instance = new AsnExtensions();

            //instance.CheckContextTag(source, ref pos);
            pos++;

            long len = instance.GetLength(source, ref pos);

            instance.extensions = new List<AsnExtension>();
            long start = pos;
            while (pos < start + len)
            {
                instance.extensions.Add(AsnExtension.Decode(source, ref pos));
            }

            return instance;
        }
    }
}
