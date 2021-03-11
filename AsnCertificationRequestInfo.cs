using System;
using System.Security.Cryptography;

namespace asn.core
{
    public class AsnCertificationRequestInfo : AsnSequence
    {
        //      CertificationRequestInfo        SEQUENCE
        //          version                     AsnInteger.cs
        //          subject                     AsnName.cs
        //          subjectPKInfo               AsnPublicKeyInfo.cs
        //          Attributes                  [0]

        // add element references back in if required
        // public AsnInteger version;
        // public AsnName subject;
        // public AsnPublicKeyInfo subjectPKInfo;
        // public AsnAttributes attributes;

        public AsnCertificationRequestInfo()
        {
            elements.Add(new AsnInteger(0));
            elements.Add(new AsnName());
            elements.Add(new AsnPublicKeyInfo());
            elements.Add(new AsnAttributes());
        }

        public static AsnCertificationRequestInfo Decode(byte[] source, ref int pos)
        {
            AsnCertificationRequestInfo instance = new AsnCertificationRequestInfo();
            //CheckContextTag(source, ref pos);
            pos++;

            // get the sequence length
            long len = instance.GetLength(source, ref pos);

            instance.elements.Add(AsnInteger.Decode(source, ref pos));
            instance.elements.Add(AsnName.Decode(source, ref pos));
            instance.elements.Add(AsnPublicKeyInfo.Decode(source, ref pos));
            instance.elements.Add(AsnAttributes.Decode(source, ref pos));

            return instance;
        }

        // RFC 2986
        public AsnCertificationRequest Sign(AsnPrivateKeyPair key)
        {
            AsnCertificationRequest request = new AsnCertificationRequest(this);

            Encode();

            RSA rsa = RSA.Create();
            rsa.ImportParameters(key.parameters);
			
            // TODO: fix this
            request.signatureAlgorithm.algorithmID.value = new Oid("1.2.840.113549.1.1.11"); // sha256withRSA
            request.signature = new AsnBitstring(rsa.SignData(derValue, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

            return request;
        }
    }
}
