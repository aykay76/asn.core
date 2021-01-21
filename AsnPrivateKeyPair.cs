using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;

namespace asn.core
{
    public class AsnPrivateKeyPair : AsnBase
    {
        public AsnInteger version;
        public AsnInteger modulus;
        public AsnInteger exponent;
        public AsnInteger privateExponent;
        public AsnInteger prime1;
        public AsnInteger prime2;
        public AsnInteger exp1;
        public AsnInteger exp2;
        public AsnInteger coefficient;
        public RSAParameters parameters;

        public AsnPrivateKeyPair(int keyLength)
        {
            RSACryptoServiceProvider prov = new RSACryptoServiceProvider(keyLength);
            parameters = prov.ExportParameters(true);

            version = new AsnInteger(0); 
            modulus = new AsnInteger(parameters.Modulus);
            exponent = new AsnInteger(parameters.Exponent);
            privateExponent = new AsnInteger(parameters.D);
            prime1 = new AsnInteger(parameters.P);
            prime2 = new AsnInteger(parameters.Q);
            exp1 = new AsnInteger(parameters.DP);
            exp2 = new AsnInteger(parameters.DQ);
            coefficient = new AsnInteger(parameters.InverseQ);
        }

        public AsnPrivateKeyPair(int keyLength, bool encode) : this(keyLength)
        {
            Encode();
        }

        public AsnPublicKeyPair GetPublicKey()
        {
            AsnPublicKeyPair publicKey = AsnPublicKeyPair.FromRSA(parameters);

            return publicKey;
        }

        public AsnPrivateKeyPair() => new AsnPrivateKeyPair(2048);

        public int Encode()
        {
            int len = 0;

            len += version.Encode();
            len += modulus.Encode();
            len += exponent.Encode();
            len += privateExponent.Encode();
            len += prime1.Encode();
            len += prime2.Encode();
            len += exp1.Encode();
            len += exp2.Encode();
            len += coefficient.Encode();

            byte[] lengthBytes = EncodeLength(len);

            derValue = new byte[1 + lengthBytes.Length + len];
            derValue[0] = 0x30;
            int d = 1;
            Array.Copy(lengthBytes, 0, derValue, d, lengthBytes.Length);
            d += lengthBytes.Length;

            Array.Copy(version.derValue, 0, derValue, d, version.derValue.Length);
            d += version.derValue.Length;
            Array.Copy(modulus.derValue, 0, derValue, d, modulus.derValue.Length);
            d += modulus.derValue.Length;
            Array.Copy(exponent.derValue, 0, derValue, d, exponent.derValue.Length);
            d += exponent.derValue.Length;
            Array.Copy(privateExponent.derValue, 0, derValue, d, privateExponent.derValue.Length);
            d += privateExponent.derValue.Length;
            Array.Copy(prime1.derValue, 0, derValue, d, prime1.derValue.Length);
            d += prime1.derValue.Length;
            Array.Copy(prime2.derValue, 0, derValue, d, prime2.derValue.Length);
            d += prime2.derValue.Length;
            Array.Copy(exp1.derValue, 0, derValue, d, exp1.derValue.Length);
            d += exp1.derValue.Length;
            Array.Copy(exp2.derValue, 0, derValue, d, exp2.derValue.Length);
            d += exp2.derValue.Length;
            Array.Copy(coefficient.derValue, 0, derValue, d, coefficient.derValue.Length);
            d += coefficient.derValue.Length;

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnPrivateKeyPair Decode(byte[] source, ref int pos)
        {
            AsnPrivateKeyPair instance = new AsnPrivateKeyPair();

            pos++;

            long len = instance.GetLength(source, ref pos);

            instance.version = AsnInteger.Decode(source, ref pos);
            instance.modulus = AsnInteger.Decode(source, ref pos);
            instance.exponent = AsnInteger.Decode(source, ref pos);
            instance.privateExponent = AsnInteger.Decode(source, ref pos);
            instance.prime1 = AsnInteger.Decode(source, ref pos);
            instance.prime2 = AsnInteger.Decode(source, ref pos);
            instance.exp1 = AsnInteger.Decode(source, ref pos);
            instance.exp2 = AsnInteger.Decode(source, ref pos);
            instance.coefficient = AsnInteger.Decode(source, ref pos);

            // bring the parameters into an RSA format
            instance.parameters.Modulus = instance.modulus.myValue.ToByteArray();
            instance.parameters.Exponent = instance.exponent.myValue.ToByteArray();
            instance.parameters.D = instance.privateExponent.myValue.ToByteArray();
            instance.parameters.P = instance.prime1.myValue.ToByteArray();
            instance.parameters.Q = instance.prime2.myValue.ToByteArray();
            instance.parameters.DP = instance.exp1.myValue.ToByteArray();
            instance.parameters.DQ = instance.exp2.myValue.ToByteArray();
            instance.parameters.InverseQ = instance.coefficient.myValue.ToByteArray();

            return instance;
        }

        public IEnumerable<string> ToPEM()
        {
            List<string> lines = new List<string>();
            lines.Add("-----BEGIN RSA PRIVATE KEY-----");

            string pem = Convert.ToBase64String(derValue);
            while (pem.Length > 64)
            {
                lines.Add(pem.Substring(0, 64));
                pem = pem.Substring(64);
            }
            lines.Add(pem);
            lines.Add("-----END RSA PRIVATE KEY-----");
            
            return lines;
        }
    }
}
