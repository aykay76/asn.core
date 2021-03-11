using System;

namespace asn.core
{
    public class AsnCertificate : AsnBase
    {
        public AsnToBeSignedCertificate tbsCertificate;
        public AsnAlgorithmIdentifier signatureAlgorithm;
        public AsnBitstring signature;

        public AsnCertificate()
        {
        }

        public AsnCertificate(AsnToBeSignedCertificate tbs)
        {
            tbsCertificate = tbs;
            signatureAlgorithm = new AsnAlgorithmIdentifier();
            signature = new AsnBitstring();
        }

        public override int Encode()
        {
            int length = 0;

            length += tbsCertificate.Encode();
            length += signatureAlgorithm.Encode();
            length += signature.Encode();

            byte[] lengthBytes = EncodeLength(length);
            derValue = new byte[1 + lengthBytes.Length + length];

            derValue[0] = 0x30; // sequence

            int d = 1;
            Array.Copy(lengthBytes, 0, derValue, d, lengthBytes.Length);
            d += lengthBytes.Length;

            Array.Copy(tbsCertificate.derValue, 0, derValue, d, tbsCertificate.derValue.Length);
            d += tbsCertificate.derValue.Length;
            Array.Copy(signatureAlgorithm.derValue, 0, derValue, d, signatureAlgorithm.derValue.Length);
            d += signatureAlgorithm.derValue.Length;
            Array.Copy(signature.derValue, 0, derValue, d, signature.derValue.Length);
            d += signature.derValue.Length;

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnCertificate Decode(byte[] source, ref int pos)
        {
            AsnCertificate instance = new AsnCertificate();

            //instance.CheckContextTag(source, ref pos);
            pos++;

            long len = instance.GetLength(source, ref pos);

            instance.tbsCertificate = AsnToBeSignedCertificate.Decode(source, ref pos);
            instance.signatureAlgorithm = AsnAlgorithmIdentifier.Decode(source, ref pos);
            instance.signature = AsnBitstring.Decode(source, ref pos);

            return instance;
        }
    }
}
