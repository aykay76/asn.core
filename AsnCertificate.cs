using System;

namespace asn.core
{
    public class AsnCertificate : AsnSequence
    {
        // elements list in order
        // public AsnToBeSignedCertificate tbsCertificate;
        // public AsnAlgorithmIdentifier signatureAlgorithm;
        // public AsnBitstring signature;

        public AsnCertificate()
        {
            elements = new System.Collections.Generic.List<AsnBase>(3);
        }

        public AsnCertificate(AsnToBeSignedCertificate tbs)
        {
            elements.Add(tbs);
            elements.Add(new AsnAlgorithmIdentifier());
            elements.Add(new AsnBitstring());
        }

        public static AsnCertificate Decode(byte[] source, ref int pos)
        {
            AsnCertificate instance = new AsnCertificate();

            //instance.CheckContextTag(source, ref pos);
            pos++;

            long len = instance.GetLength(source, ref pos);

            instance.elements.Add(AsnToBeSignedCertificate.Decode(source, ref pos));
            instance.elements.Add(AsnAlgorithmIdentifier.Decode(source, ref pos));
            instance.elements.Add(AsnBitstring.Decode(source, ref pos));

            return instance;
        }
    }
}
