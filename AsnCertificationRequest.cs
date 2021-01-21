using System;

namespace asn.core
{
    public class AsnCertificationRequest : AsnBase
    {
        //  ASN.1 type CertificationRequest     SEQUENCE
        //      CertificationRequestInfo        AsnCertificationRequestInfo.cs
        //      signatureAlgorithm              SignatureAlgorithm.cs
        //      Signature                       AsnBitstring.cs
        public AsnCertificationRequestInfo requestInfo;
        public AsnAlgorithmIdentifier signatureAlgorithm;
        public AsnBitstring signature;

        public AsnCertificationRequest()
        {
        }

        public AsnCertificationRequest(AsnCertificationRequestInfo info)
        {
            requestInfo = info;
            signatureAlgorithm = new AsnAlgorithmIdentifier();
        }

        public int Encode()
        {
            int len = 0;

            len += requestInfo.Encode();
            len += signatureAlgorithm.Encode();
            len += signature.Encode();

            byte[] lengthBytes = EncodeLength(len);

            derValue = new byte[lengthBytes.Length + len + 1];
            derValue[0] = 0x30;

            int d = 1;

            Array.Copy(lengthBytes, 0, derValue, d, lengthBytes.Length);
            d += lengthBytes.Length;
            Array.Copy(requestInfo.derValue, 0, derValue, d, requestInfo.derValue.Length);
            d += requestInfo.derValue.Length;
            Array.Copy(signatureAlgorithm.derValue, 0, derValue, d, signatureAlgorithm.derValue.Length);
            d += signatureAlgorithm.derValue.Length;
            Array.Copy(signature.derValue, 0, derValue, d, signature.derValue.Length);
            d += signature.derValue.Length;

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnCertificationRequest Decode(byte[] source, ref int pos)
        {
            AsnCertificationRequest instance = new AsnCertificationRequest();

            //CheckContextTag(source, ref pos);
            pos++;

            long length = instance.GetLength(source, ref pos);

            instance.requestInfo = AsnCertificationRequestInfo.Decode(source, ref pos);
            instance.signatureAlgorithm = AsnAlgorithmIdentifier.Decode(source, ref pos);
            instance.signature = AsnBitstring.Decode(source, ref pos);

            return instance;
        }
    }
}
