using System;

namespace asn.core
{
    public class AsnPrivateKeyInfo : AsnSequence
    {
        // PKCS #1 format private key
        //      subjectPKInfo               SEQUENCE
        //          Algorithm               AsnAlgorithmIdentifier.cs
        //          Public Key              AsnBitstring.cs

        public AsnAlgorithmIdentifier algorithm;
        public AsnBitstring publicKey;
        public AsnPrivateKeyPair keys;

        public AsnPrivateKeyInfo()
        {
            algorithm = new AsnAlgorithmIdentifier();
            publicKey = new AsnBitstring();
            keys = new AsnPrivateKeyPair();
        }

        public override int Encode()
        {
            int len = 0;

            // first need to encode keys into bitstring
            keys.Encode();
            publicKey.value = keys.derValue;

            len += algorithm.Encode();
            len += publicKey.Encode();

            byte[] lengthBytes = EncodeLength(len);

            derValue = new byte[1 + lengthBytes.Length + len];
            derValue[0] = 0x30; // sequence
            int d = 1;
            Array.Copy(lengthBytes, 0, derValue, d, lengthBytes.Length);
            d += lengthBytes.Length;
            Array.Copy(algorithm.derValue, 0, derValue, d, algorithm.derValue.Length);
            d += algorithm.derValue.Length;
            Array.Copy(publicKey.derValue, 0, derValue, d, publicKey.derValue.Length);

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnPrivateKeyInfo Decode(byte[] source, ref int pos)
        {
            AsnPrivateKeyInfo instance = new AsnPrivateKeyInfo();

            //CheckContextTag(source, ref pos);
            pos++;

            int len = instance.GetLength(source, ref pos);

            instance.algorithm = AsnAlgorithmIdentifier.Decode(source, ref pos);
            instance.publicKey = AsnBitstring.Decode(source, ref pos);

            // TODO: further decode publicKey into AsnKeyPair
            int bi = 0;
            instance.keys = AsnPrivateKeyPair.Decode(instance.publicKey.value, ref bi);

            return instance;
        }
    }
}
