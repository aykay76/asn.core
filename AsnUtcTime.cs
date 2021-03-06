﻿using System;
using System.Globalization;

namespace asn.core
{
    public class AsnUtcTime : AsnBase
    {
        public DateTime value;

        public override int Encode()
        {
            string text = value.ToString("yyMMddHHmmssZ");

            byte[] lengthBytes = EncodeLength(text.Length);
            byte[] valueBytes = System.Text.Encoding.UTF8.GetBytes(text);

            derValue = new byte[1 + lengthBytes.Length + valueBytes.Length];
            derValue[0] = (byte)AsnType.UniversalTag.UTCTime;
            Array.Copy(lengthBytes, 0, derValue, 1, lengthBytes.Length);
            Array.Copy(valueBytes, 0, derValue, 1 + lengthBytes.Length, valueBytes.Length);

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnUtcTime Decode(byte[] source, ref int pos)
        {
            AsnUtcTime instance = new AsnUtcTime();

            //CheckContextTag(source, ref pos);
            pos++;

            int length = instance.GetLength(source, ref pos);
            byte[] raw = new byte[length];
            Array.Copy(source, pos, raw, 0, length);
            string decoded = System.Text.Encoding.ASCII.GetString(raw);

            //value = DateTime.ParseExact(decoded, "yyMMddhhmmssZ", CultureInfo.CurrentCulture);

            int offsetIndex = decoded.IndexOfAny(new char[] { '+', '-', 'Z' });

            instance.value = DateTime.ParseExact(decoded.Substring(0, 6), "yyMMdd", CultureInfo.CurrentCulture);
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
            instance.value = instance.value.Add(time);

            if (decoded.Substring(offsetIndex + 1).Length > 0)
            {
                TimeSpan offset = TimeSpan.ParseExact(decoded.Substring(offsetIndex + 1), "hhmm", CultureInfo.CurrentCulture);
                instance.value = instance.value.Add(offset);
            }

            //Console.WriteLine(date);
            pos += length;

            return instance;
        }
    }
}
