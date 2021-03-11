using System;

namespace asn.core
{
    public class AsnCertificationRequest : AsnSequence
    {
        //  ASN.1 type CertificationRequest     SEQUENCE
        //      CertificationRequestInfo        AsnCertificationRequestInfo.cs
        //      signatureAlgorithm              SignatureAlgorithm.cs
        //      Signature                       AsnBitstring.cs

        // elements list - add in as refs if required
        // public AsnCertificationRequestInfo requestInfo;
        // public AsnAlgorithmIdentifier signatureAlgorithm;
        // public AsnBitstring signature;

        public AsnCertificationRequest()
        {
        }

        public AsnCertificationRequest(AsnCertificationRequestInfo info)
        {
            elements.Add(info);
            elements.Add(new AsnAlgorithmIdentifier());
            elements.Add(new AsnBitstring());
        }

        public static AsnCertificationRequest Decode(byte[] source, ref int pos)
        {
            AsnCertificationRequest instance = new AsnCertificationRequest();

            //CheckContextTag(source, ref pos);
            pos++;

            long length = instance.GetLength(source, ref pos);

            instance.elements.Add(AsnCertificationRequestInfo.Decode(source, ref pos));
            instance.elements.Add(AsnAlgorithmIdentifier.Decode(source, ref pos));
            instance.elements.Add(AsnBitstring.Decode(source, ref pos));

            return instance;
        }
    }
}
