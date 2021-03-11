using System;

namespace asn.core
{
    public class AsnBoolean : AsnBase
    {
        public bool value;

        public AsnBoolean()
        {
            
        }

        public AsnBoolean(bool newValue)
        {
            value = newValue;
        }

        public override int Encode()
        {
            byte[] lengthBytes = EncodeLength(1);

            derValue = new byte[1 + lengthBytes.Length + 1];

            // first byte is a tag
            derValue[0] = 0x01;

            // second byte is length - always one byte
            Array.Copy(lengthBytes, 0, derValue, 1, lengthBytes.Length);

            // third byte is value
            if (value)
            {
                derValue[1 + lengthBytes.Length] = 0xff;
            }
            else
            {
                derValue[1 + lengthBytes.Length] = 0;
            }

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnBoolean Decode(byte[] source, ref int pos)
        {
            AsnBoolean instance = new AsnBoolean();

            //CheckContextTag(source, ref pos);
            pos++;

            long length = instance.GetLength(source, ref pos);

            instance.value = (bool)(source[pos] != 0);
            pos++;

            return instance;
        }
    }
}
