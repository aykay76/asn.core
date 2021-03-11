using System;
using System.Collections.Generic;

namespace asn.core
{
    /// <summary>
    /// Base class for all AsnTypes, any common methods will be here (encode/decode probably)
    /// </summary>
    public class AsnBase
    {
        public int contextTag = int.MaxValue;
        public byte contextTagType = 0xa0;
        public byte[] originalDer;
        public byte[] derValue;

        protected void CheckContextTag(byte[] source, ref int pos)
        {
            if ((source[pos] & 0xc0) == 0x80)
            {
                pos++;

                // get context specific tag
                contextTag = (int)source[pos];
                pos++;
            }
        }

        protected void PrependContextTag()
        {
            if (contextTag != int.MaxValue)
            {
                byte[] temp = derValue;
                derValue = new byte[2 + temp.Length];
                derValue[0] = contextTagType;
                derValue[1] = (byte)contextTag;
                Array.Copy(temp, 0, derValue, 2, temp.Length);
            }
        }

        protected byte[] EncodeLength(int length)
        {
            byte[] lengthBytes = null;

            if (length <= 127)
            {
                lengthBytes = new byte[1];
                lengthBytes[0] = (byte)length;
            }
            else
            {
                byte[] temp = BitConverter.GetBytes(length);
                
                Queue<byte> significant = new Queue<byte>();
                bool found = false;
                for (int i = 3; i >= 0; i--)
                {
                    if (temp[i] > 0)
                    {
                        found = true;
                    }
                    if (found)
                    {
                        significant.Enqueue(temp[i]);
                    }
                }

                lengthBytes = new byte[1 + significant.Count];
                lengthBytes[0] = (byte)(0x80 | significant.Count);
                int pos = 1;
                while (significant.Count > 0)
                {
                    byte b = significant.Dequeue();
                    lengthBytes[pos] = b;
                    pos++;
                }
            }

            return lengthBytes;
        }

        public virtual int Encode()
        {
            return 0;
        }

        protected int GetLength(byte[] array, ref int index)
        {
            int length = 0;

            if ((array[index] & 0x80) == 0x80)
            {
                // long form - bits 7-1 tell how many bytes follow containing length
                int lengthBytes = array[index] & 0x7f;
                index++;
                if (lengthBytes > 8)
                {
                    throw new FormatException("Can only handle 64-bit lengths right now");
                }
                else
                {
                    for (int i = 0; i < lengthBytes; i++)
                    {
                        length <<= 8;
                        length += array[index];
                        index++;
                    }
                }
            }
            else
            {
                // short term - bits 7-1 contain length
                length = array[index] & 0x7f;
                index++;
            }

            return length;
        }
    }
}
