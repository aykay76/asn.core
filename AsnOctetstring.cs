using System;

namespace asn.core
{
    public class AsnOctetstring : AsnBase
    {
        public byte[] value;

        public AsnOctetstring()
        {
            
        }

        public AsnOctetstring(byte[] newValue)
        {
            value = newValue;
        }

        public override int Encode()
        {
            byte[] lengthBytes = EncodeLength(value.Length);
            derValue = new byte[1 + lengthBytes.Length + value.Length];

            Array.Copy(lengthBytes, 0, derValue, 1, lengthBytes.Length);
            Array.Copy(value, 0, derValue, 1 + lengthBytes.Length, value.Length);

            derValue[0] = 0x4;
            PrependContextTag();

            return derValue.Length;
        }

        public static AsnOctetstring Decode(byte[] source, ref int pos)
        {
            AsnOctetstring instance = new AsnOctetstring();

            //CheckContextTag(source, ref pos);
            pos++;

            int length = instance.GetLength(source, ref pos);
            instance.value = new byte[length];
            Array.Copy(source, pos, instance.value, 0, length);
            pos += length;

            return instance;
        }
    }
}
