using System;

namespace asn.core
{
    public class AsnString : AsnBase
    {
        public string value;
        public AsnType.UniversalTag tag;

        public AsnString()
        {
            
        }

        public AsnString(string newValue, AsnType.UniversalTag newTag)
        {
            value = newValue;
            tag = newTag;
        }

        public int Encode()
        {
            byte[] lengthBytes = EncodeLength(value.Length);
            byte[] valueBytes = System.Text.Encoding.UTF8.GetBytes(value);

            derValue = new byte[1 + lengthBytes.Length + valueBytes.Length];
            derValue[0] = (byte)tag;
            Array.Copy(lengthBytes, 0, derValue, 1, lengthBytes.Length);
            Array.Copy(valueBytes, 0, derValue, 1 + lengthBytes.Length, valueBytes.Length);

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnString Decode(byte[] source, ref int pos)
        {
            AsnString instance = new AsnString()
            {
                tag = (AsnType.UniversalTag)source[pos]
            };

            pos++;

            // length and value in subsequent bytes
            int length = instance.GetLength(source, ref pos);
            byte[] raw = new byte[length];
            Array.Copy(source, pos, raw, 0, length);
            instance.value = System.Text.Encoding.UTF8.GetString(raw);
            pos += (int)length;

            return instance;
        }
    }
}
