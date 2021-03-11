using System;
using System.Numerics;
using System.Security.Cryptography;

namespace asn.core
{
    public class AsnPublicKeyPair : AsnBase
    {
        public AsnInteger modulus;
        public AsnInteger exponent;
        public RSAParameters parameters;

        public AsnPublicKeyPair()
        {
            RSACryptoServiceProvider prov = new RSACryptoServiceProvider(2048);
            parameters = prov.ExportParameters(false);

            modulus = new AsnInteger(parameters.Modulus);
            exponent = new AsnInteger(parameters.Exponent);
        }

        public static AsnPublicKeyPair FromRSA(RSAParameters rsa)
        {
            AsnPublicKeyPair kp = new AsnPublicKeyPair
            {
                modulus = new AsnInteger(rsa.Modulus),
                exponent = new AsnInteger(rsa.Exponent)
            };
            return kp;
        }

        public override int Encode()
        {
            int len = 0;

            // modulus needs an extra byte at the beginning to signify it's a positive value - so will be 257 bytes long instead of 256
            len += modulus.Encode();
            len += exponent.Encode();

            byte[] lengthBytes = EncodeLength(len);

            derValue = new byte[1 + lengthBytes.Length + len];
            derValue[0] = 0x30;

            int pos = 1;

            Array.Copy(lengthBytes, 0, derValue, pos, lengthBytes.Length);
            pos += lengthBytes.Length;

            Array.Copy(modulus.derValue, 0, derValue, pos, modulus.derValue.Length);
            pos += modulus.derValue.Length;

            Array.Copy(exponent.derValue, 0, derValue, pos, exponent.derValue.Length);
            pos += exponent.derValue.Length;

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnPublicKeyPair Decode(byte[] source, ref int pos)
        {
            AsnPublicKeyPair instance = new AsnPublicKeyPair();

            pos++;

            long len = instance.GetLength(source, ref pos);

            instance.modulus = AsnInteger.Decode(source, ref pos);
            instance.exponent = AsnInteger.Decode(source, ref pos);

            // bring the parameters into an RSA format
            instance.parameters.Modulus = instance.modulus.myValue.ToByteArray();
            instance.parameters.Exponent = instance.exponent.myValue.ToByteArray();

            return instance;
        }
    }
}
