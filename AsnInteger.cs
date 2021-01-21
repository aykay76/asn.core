using System;
using System.Numerics;

namespace asn.core
{
    public class AsnInteger : AsnBase
    {
        public BigInteger myValue;

        public AsnInteger(int newValue)
        {
            myValue = new BigInteger(newValue);
        }

        public AsnInteger(byte[] newValue)
        {
            myValue = new BigInteger(newValue);
        }

        public int Encode()
        {            
            byte[] valueBytes = null;

			if (myValue.Sign < 0)
			{
                byte[] absValue = myValue.ToByteArray();
                valueBytes = new byte[absValue.Length + 1];
                valueBytes[0] = 0;
                Array.Copy(absValue, 0, valueBytes, 1, absValue.Length);
			}
            else
            {
                valueBytes = myValue.ToByteArray();
            }
			
            byte[] lengthBytes = EncodeLength(valueBytes.Length);

            derValue = new byte[1 + lengthBytes.Length + valueBytes.Length];

            derValue[0] = 0x2;
            Array.Copy(lengthBytes, 0, derValue, 1, lengthBytes.Length);
            Array.Copy(valueBytes, 0, derValue, 1 + lengthBytes.Length, valueBytes.Length);

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnInteger Decode(byte[] source, ref int pos)
        {
            //CheckContextTag(source, ref pos);
            pos++;

            // length and value in subsequent bytes
            int length = AsnType.GetLength(source, ref pos);

            byte[] raw = new byte[length];
            Array.Copy(source, pos, raw, 0, length);

            AsnInteger instance = new AsnInteger(raw);

            pos += length;

            return instance;

            //foreach (byte b in value.ToByteArray())
            //{
            //    Console.Write("{0:X2} ", b);
            //}
            //Console.WriteLine(value);
        }
    }
}
