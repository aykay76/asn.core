using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace asn.core
{
    /// <summary>
    /// General case ASN decoder, build specific cases on the back of this to handle special context cases
    /// </summary>
    public class AsnType
    {
        public enum TagType { Universal = 0, Application = 1, Context = 2, Private = 3 }
        public enum UniversalTag { Boolean = 1, Integer = 2, BitString = 3, OctetString = 4, Null = 5, ObjectID = 6, UTF8String = 12, Sequence = 16, Set = 17, PrintableString = 19, T61String = 20, IA5String = 22, UTCTime = 23, GeneralisedTime = 24, BMPString = 0x1e }

        public int Encode()
        {
            // TLV - Tag, Length, Value
            return 0;
        }

        public static int GetLength(byte[] array, ref int index)
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

        public BigInteger GetLengthBig(byte[] array, ref int index)
        {
            BigInteger length = BigInteger.MinusOne;

            if ((array[index] & 0x80) == 0x80)
            {
                // long form - bits 7-1 tell how many bytes follow containing length
                int lengthBytes = array[index] & 0x7f;
                index++;
                byte[] raw = new byte[lengthBytes];
                Array.Copy(array, index, raw, 0, lengthBytes);
                index += lengthBytes;

                length = new BigInteger(raw);
            }
            else
            {
                // short term - bits 7-1 contain length
                length = new BigInteger(array[index] & 0x7f);
                index++;
            }

            return length;
        }

        public BigInteger GetValue(byte[] array, ref int index, int length)
        {
            BigInteger value = BigInteger.MinusOne;

            byte[] raw = new byte[length];
            Array.Copy(array, index, raw, 0, length);
            index += length;

            value = new BigInteger(raw);

            return value;
        }

        public void Decode(byte[] test, ref int bi)
        {
            TagType tagType = (TagType)((test[bi] & 0xc0) >> 6);
            bool constructed = ((test[bi] & 0x20) == 0x20);
            int tagNum = (test[bi] & 0x1f);

            // move on past the tag byte
            bi++;

            // test bits 7&8 of the first byte are zero - universal tag known from above enum
            if (tagType == TagType.Universal)
            {
                if (tagNum == 0)
                {
                    Console.WriteLine("[0]");
                    if (constructed)
                    {
                        // length and value in subsequent bytes
                        long length = GetLength(test, ref bi);

                        int start = bi;
                        while (bi < start + length)
                        {
                            Decode(test, ref bi);
                        }
                    }
                }
                else if (tagNum == (int)UniversalTag.Boolean)
                {
                    // length and value in subsequent bytes
                    long length = GetLength(test, ref bi);
                    bool value = (bool)(test[bi] != 0);

                    Console.WriteLine("Boolean: {0}", value);

                    bi++;
                }
                else if (tagNum == (int)UniversalTag.Integer)
                {
                    Console.WriteLine("Integer");
                    // length and value in subsequent bytes
                    long length = GetLength(test, ref bi);
                    // TODO: UNFINISHED, need to get head around twos-complement and do this properly
                    BigInteger value = GetValue(test, ref bi, (int)length);
                    foreach (byte b in value.ToByteArray())
                    {
                        Console.Write("{0:X2} ", b);
                    }
                    Console.WriteLine(value);
                }
                else if (tagNum == (int)UniversalTag.Null)
                {
                    bi++;
                    Console.WriteLine("NULL");
                }
                else if (tagNum == (int)UniversalTag.BitString)
                {
                    // length and value in subsequent bytes
                    int length = GetLength(test, ref bi);

                    if (constructed)
                    {
                        int start = bi;
                        while (bi < start + length)
                        {
                            Decode(test, ref bi);
                        }
                    }
                    else
                    {
                        // first byte of a bit string denotes how many unused bits are at the end of the bit string
                        byte unused = test[bi];
                        bi++;
                        length--;

                        Console.WriteLine("Bit string, length = {0}, unused = {1}", length, unused);
                        byte[] raw = new byte[length];
                        Array.Copy(test, bi, raw, 0, length);
                        Console.WriteLine("Base64 encoded BitString:\r\n{0}", Convert.ToBase64String(raw));
                        foreach (var b in raw)
                        {
                            Console.Write(b.ToString("X2"));
                        }

                        Console.WriteLine("");
                        bi += (int)length;
                    }
                }
                else if (tagNum == (int)UniversalTag.OctetString)
                {
                    // length and value in subsequent bytes
                    int length = GetLength(test, ref bi);

                    Console.WriteLine("Octet string, length = {0} bytes", length);

                    // TODO: some octet strings are decomposable but i think it depends on the use
                    //int start = bi;
                    //while (bi < start + length)
                    //{
                    //    Decode(test, ref bi);
                    //}

                    byte[] raw = new byte[length];
                    Array.Copy(test, bi, raw, 0, length);
                    foreach (var b in raw)
                    {
                        Console.Write(b.ToString("X2"));
                    }

                    Console.WriteLine("");
                    bi += (int)length;
                }
                else if (tagNum == (int)UniversalTag.ObjectID)
                {
                    Console.WriteLine("Object ID");
                    // length and value in subsequent bytes
                    int length = GetLength(test, ref bi);
                    byte[] raw = new byte[length];
                    Array.Copy(test, bi, raw, 0, length);

                    // special case, first byte
                    int id1 = (raw[0] / 40);
                    int id2 = (raw[0] % 40);
                    bool cont = false;
                    long biggun = 0;
                    string oid = id1.ToString() + "." + id2.ToString();
                    for (int i = 1; i < raw.Length; i++)
                    {
                        if ((raw[i] & 0x80) == 0x80)
                        {
                            cont = true;
                            biggun <<= 7;
                            biggun += raw[i] & 0x7f;
                        }
                        else
                        {
                            if (cont)
                            {
                                biggun <<= 7;
                                biggun += raw[i] & 0x7f;
                                oid += "." + biggun.ToString();
                                cont = false;
                                biggun = 0;
                            }
                            else
                            {
                                oid += "." + raw[i].ToString();
                            }
                        }
                    }

                    System.Security.Cryptography.Oid o = new System.Security.Cryptography.Oid(oid);
                    Console.WriteLine(oid);
                    Console.WriteLine(o.FriendlyName);

                    bi += (int)length;
                }
                else if (tagNum == (int)UniversalTag.UTF8String)
                {
                    Console.WriteLine("UTF8 String");
                    // length and value in subsequent bytes
                    int length = GetLength(test, ref bi);
                    byte[] raw = new byte[length];
                    Array.Copy(test, bi, raw, 0, length);
                    string decoded = System.Text.Encoding.UTF8.GetString(raw);
                    Console.WriteLine(decoded);
                    bi += (int)length;
                }
                else if (tagNum == (int)UniversalTag.Sequence)
                {
                    long length = GetLength(test, ref bi);
                    Console.WriteLine("Sequence contained in {0} bytes", (int)length);

                    // because sequence is a constructed type we can recurse into this function
                    if (constructed)
                    {
                        int start = bi;
                        while (bi < start + length)
                        {
                            Decode(test, ref bi);
                        }
                    }
                }
                else if (tagNum == (int)UniversalTag.Set)
                {
                    long length = GetLength(test, ref bi);
                    Console.WriteLine("Set contained in {0} bytes", (int)length);

                    // because sequence is a constructed type we can recurse into this function
                    if (constructed)
                    {
                        int start = bi;
                        while (bi < start + length)
                        {
                            Decode(test, ref bi);
                        }
                    }
                }
                else if (tagNum == (int)UniversalTag.PrintableString)
                {
                    Console.WriteLine("Printable String");
                    // length and value in subsequent bytes
                    int length = GetLength(test, ref bi);
                    byte[] raw = new byte[length];
                    Array.Copy(test, bi, raw, 0, length);
                    string decoded = System.Text.Encoding.ASCII.GetString(raw);
                    Console.WriteLine(decoded);
                    bi += (int)length;
                }
                else if (tagNum == (int)UniversalTag.T61String)
                {
                    Console.WriteLine("T61 String");
                    // length and value in subsequent bytes
                    int length = GetLength(test, ref bi);
                    byte[] raw = new byte[length];
                    Array.Copy(test, bi, raw, 0, length);
                    string decoded = System.Text.Encoding.UTF7.GetString(raw);
                    Console.WriteLine(decoded);
                    bi += (int)length;
                }
                else if (tagNum == (int)UniversalTag.IA5String)
                {
                    Console.WriteLine("IA5 String");
                    // length and value in subsequent bytes
                    int length = GetLength(test, ref bi);
                    byte[] raw = new byte[(int)length];
                    Array.Copy(test, bi, raw, 0, (int)length);
                    string decoded = System.Text.Encoding.ASCII.GetString(raw);
                    Console.WriteLine(decoded);
                    bi += (int)length;
                }
				else if (tagNum == (int)UniversalTag.UTCTime)
				{
					Console.WriteLine("UTC Time");

					long length = GetLength(test, ref bi);
					byte[] raw = new byte[(int)length];
					Array.Copy(test, bi, raw, 0, (int)length);
					string decoded = System.Text.Encoding.ASCII.GetString(raw);

					int offsetIndex = decoded.IndexOfAny(new char[] { '+', '-', 'Z' });

					DateTime date = DateTime.ParseExact(decoded.Substring(0, 6), "yyMMdd", CultureInfo.CurrentCulture);
					string timeString = decoded.Substring(6, offsetIndex - 6);
					TimeSpan time = TimeSpan.MinValue;
					if (timeString.Length == 4)
					{
						time = TimeSpan.ParseExact(timeString, "hhmm", CultureInfo.CurrentCulture);
					}
					else if (timeString.Length == 6)
					{
						time = TimeSpan.ParseExact(timeString, "hhmmss", CultureInfo.CurrentCulture);
					}
					else
					{
						throw new FormatException();
					}
					date = date.Add(time);

					if (decoded.Substring(offsetIndex + 1).Length > 0)
					{
						TimeSpan offset = TimeSpan.ParseExact(decoded.Substring(offsetIndex + 1), "hhmm", CultureInfo.CurrentCulture);
						date = date.Add(offset);
					}

					Console.WriteLine(date);
					bi += (int)length;
				}
                else if (tagNum == (int)UniversalTag.GeneralisedTime)
				{
					Console.WriteLine("Generalised Time");

					long length = GetLength(test, ref bi);
					byte[] raw = new byte[(int)length];
					Array.Copy(test, bi, raw, 0, (int)length);
					string decoded = System.Text.Encoding.ASCII.GetString(raw);

					int offsetIndex = decoded.IndexOfAny(new char[] { '+', '-', 'Z' });

					DateTime date = DateTime.ParseExact(decoded.Substring(0, 8), "yyyyMMdd", CultureInfo.CurrentCulture);
					string timeString = decoded.Substring(8, offsetIndex - 8);
					TimeSpan time = TimeSpan.MinValue;
					if (timeString.Length == 4)
					{
						time = TimeSpan.ParseExact(timeString, "hhmm", CultureInfo.CurrentCulture);
					}
					else if (timeString.Length == 6)
					{
						time = TimeSpan.ParseExact(timeString, "hhmmss", CultureInfo.CurrentCulture);
					}
					else
					{
						throw new FormatException();
					}
					date = date.Add(time);

					if (decoded.Substring(offsetIndex + 1).Length > 0)
					{
						TimeSpan offset = TimeSpan.ParseExact(decoded.Substring(offsetIndex + 1), "hhmm", CultureInfo.CurrentCulture);
						date = date.Add(offset);
					}

					Console.WriteLine(date);
					bi += (int)length;
				}
				else if (tagNum == (int)UniversalTag.BMPString)
                {
                    int length = GetLength(test, ref bi);
                    byte[] raw = new byte[length];
                    Array.Copy(test, bi, raw, 0, length);
                    string decoded = System.Text.Encoding.Unicode.GetString(raw);
                    Console.WriteLine("BMPString ({1}): {0}", decoded, length);
                }
                else
                {
                    throw new InvalidOperationException("Unknown tag: " + test[bi].ToString("x2"));
                }
            }
            else if (tagType == TagType.Context)
            {
                if (constructed)
                {
                    // length and value in subsequent bytes
                    long length = GetLength(test, ref bi);

                    Console.WriteLine("Context tag: " + tagNum + " - length " + length);

                    int start = bi;
                    while (bi < start + length)
                    {
                        Decode(test, ref bi);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Primitive context specific tag, I don't know what to do with this");
                }
            }
            else
            {
                throw new InvalidOperationException("Unknown tag type, I don't know what to do with this");
            }
        }
    }
}
