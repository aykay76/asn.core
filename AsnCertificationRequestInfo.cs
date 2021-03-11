using System;
using System.Security.Cryptography;

namespace asn.core
{
    public class AsnCertificationRequestInfo : AsnBase
    {
        //      CertificationRequestInfo        SEQUENCE
        //          version                     AsnInteger.cs
        //          subject                     AsnName.cs
        //          subjectPKInfo               AsnPublicKeyInfo.cs
        //          Attributes                  [0]
        public AsnInteger version;
        public AsnName subject;
        public AsnPublicKeyInfo subjectPKInfo;

        public AsnCertificationRequestInfo()
        {
            version = new AsnInteger(0);
            subject = new AsnName();
            subjectPKInfo = new AsnPublicKeyInfo();
        }

        public override int Encode()
        {
            int len = 0;

            int versionLength = version.Encode();
            int subjectLength = subject.Encode();
            int pkinfoLength =  subjectPKInfo.Encode();

            len = versionLength + subjectLength + pkinfoLength + 2;

            byte[] lengthBytes = EncodeLength(len);

            derValue = new byte[1 + lengthBytes.Length + len];
            derValue[0] = 0x30;

            int d = 1;

            Array.Copy(lengthBytes, 0, derValue, d, lengthBytes.Length);
            d += lengthBytes.Length;
            Array.Copy(version.derValue, 0, derValue, d, version.derValue.Length);
            d += version.derValue.Length;
            Array.Copy(subject.derValue, 0, derValue, d, subject.derValue.Length);
            d += subject.derValue.Length;
            Array.Copy(subjectPKInfo.derValue, 0, derValue, d, subjectPKInfo.derValue.Length);
            d += subjectPKInfo.derValue.Length;

            derValue[d] = 0xa0;
            d++;
            derValue[d] = 0x00;
            d++;

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnCertificationRequestInfo Decode(byte[] source, ref int pos)
        {
            AsnCertificationRequestInfo instance = new AsnCertificationRequestInfo();
            //CheckContextTag(source, ref pos);
            pos++;

            // get the sequence length
            long len = instance.GetLength(source, ref pos);

            instance.version = AsnInteger.Decode(source, ref pos);
            instance.subject = AsnName.Decode(source, ref pos);
            instance.subjectPKInfo = AsnPublicKeyInfo.Decode(source, ref pos);

            // ignroe the attributes
            pos++; pos++;

            return instance;
        }

        // RFC 2986
        public AsnCertificationRequest Sign(AsnPrivateKeyPair key)
        {
            AsnCertificationRequest request = new AsnCertificationRequest(this);

            Encode();

            RSA rsa = RSA.Create();
            rsa.ImportParameters(key.parameters);
			
            request.signatureAlgorithm.algorithmID.value = new Oid("1.2.840.113549.1.1.11"); // sha256withRSA
            request.signature = new AsnBitstring(rsa.SignData(derValue, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

            return request;
        }
    }
}
