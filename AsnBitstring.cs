using System;

namespace asn.core
{
    public class AsnBitstring : AsnBase
    {
        public byte[] value;
        public byte unused = 0;

        public AsnBitstring()
        {
            
        }

        public AsnBitstring(byte[] newValue)
        {
            value = newValue;
        }

        public override int Encode()
        {
            byte[] lengthBytes = EncodeLength(value.Length + 1); // add one byte for unused flag

            // explicit addition to break out the different part of the encoded values
            derValue = new byte[1 + lengthBytes.Length + 1 + value.Length];

            int pos = 0;
            derValue[pos] = 0x3;
            pos++;

            Array.Copy(lengthBytes, 0, derValue, pos, lengthBytes.Length);
            pos += lengthBytes.Length;

            derValue[pos] = unused;
            pos++;

            Array.Copy(value, 0, derValue, pos, value.Length);

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnBitstring Decode(byte[] source, ref int pos)
        {
            AsnBitstring instance = new AsnBitstring();

            //CheckContextTag(source, ref pos);
            pos++;

            long length = instance.GetLength(source, ref pos);

            // first byte of a bit string denotes how many unused bits are at the end of the bit string
            instance.unused = source[pos];
            pos++;
            length--;

            //Console.WriteLine("Bit string, length = {0}, unused = {1}", length, unused);
            instance.value = new byte[length];
            Array.Copy(source, pos, instance.value, 0, (int)length);
            //foreach (var b in value)
            //{
            //    Console.Write(b.ToString("X2"));
            //}

            //Console.WriteLine("");
            pos += (int)length;

            return instance;
        }
    }
}
