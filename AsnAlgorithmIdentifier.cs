using System;
using System.Security.Cryptography;

namespace asn.core
{
    public class AsnAlgorithmIdentifier : AsnSequence
    {
        //      signatureAlgorithm              SEQUENCE
        //          AlgorithmID                 OID
        //          Parameters                  NULL
        public AsnOid algorithmID;
        public AsnNull parameters;

        public AsnAlgorithmIdentifier()
        {
            elements.Add(new AsnOid("1.2.840.113549.1.1.1"));
            elements.Add(new AsnNull());
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
