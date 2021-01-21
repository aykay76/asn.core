using System;
using System.Security.Cryptography;

namespace asn.core
{
    public class AsnAlgorithmIdentifier : AsnBase
    {
        //      signatureAlgorithm              SEQUENCE
        //          AlgorithmID                 OID
        //          Parameters                  NULL
        public AsnOid algorithmID;
        public AsnNull parameters;

        public AsnAlgorithmIdentifier()
        {
            algorithmID = new AsnOid("1.2.840.113549.1.1.1");
            parameters = new AsnNull();
        }

        public int Encode()
        {
            int len = 0;

            len += algorithmID.Encode();
            len += parameters.Encode();

            byte[] lengthBytes = EncodeLength(len);
            derValue = new byte[1 + lengthBytes.Length + len];
            derValue[0] = 0x30; // sequence
            int d = 1;
            Array.Copy(lengthBytes, 0, derValue, d, lengthBytes.Length);
            d += lengthBytes.Length;
            Array.Copy(algorithmID.derValue, 0, derValue, d, algorithmID.derValue.Length);
            d += algorithmID.derValue.Length;
            Array.Copy(parameters.derValue, 0, derValue, d, parameters.derValue.Length);

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnAlgorithmIdentifier Decode(byte[] source, ref int pos)
        {
            AsnAlgorithmIdentifier instance = new AsnAlgorithmIdentifier();

            pos++;

            long len = instance.GetLength(source, ref pos);

            instance.algorithmID = AsnOid.Decode(source, ref pos);
            instance.parameters = AsnNull.Decode(source, ref pos);

            return instance;
        }
    }
}
