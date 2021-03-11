using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace asn.core
{
    public class AsnOid : AsnBase
    {
        public Oid value;

        public AsnOid()
        {
        }

        public AsnOid(Oid newValue)
        {
            value = newValue;
        }

        public AsnOid(string oidValue)
        {
            value = new Oid(oidValue);
        }

        public static AsnOid FromFriendlyName(string name)
        {
            AsnOid val = null;
            try
            {
                val = new AsnOid(Oid.FromFriendlyName(name, OidGroup.All));
            }
            catch
            {
            }

            if (val == null)
            {
                switch (name)
                {
                    case "X509v3 Key Usage":
                        val = new AsnOid(new Oid("2.5.29.15"));
                        break;
                    case "X509v3 Subject Key Identifier":
                        val = new AsnOid(Oid.FromFriendlyName("Subject Key Identifier", OidGroup.All));
                        break;
                }
            }

            return val;
        }

        public override int Encode()
        {
            string[] parts = value.Value.Split(new char[] { '.' });
            List<byte> bytes = new List<byte>
            {
                (byte)(int.Parse(parts[0]) * 0x28 + int.Parse(parts[1]))
            };
            for (int p = 2; p < parts.Length; p++)
            {
                int intPart = int.Parse(parts[p]);

                Stack<byte> valueStack = new Stack<byte>();
                bool last = true;
                while (intPart > 0)
                {
                    byte bits = (byte)(intPart & 0x7f);
                    if (last)
                    {
                        last = false;
                    }
                    else
                    {
                        bits |= 0x80;
                    }
                    valueStack.Push(bits);
                    intPart = intPart >> 7;
                }
                // unwrap the stack onto the list
                while (valueStack.Count > 0)
                {
                    bytes.Add(valueStack.Pop());
                }
            }

            byte[] lengthBytes = EncodeLength(bytes.Count);
            derValue = new byte[1 + lengthBytes.Length + bytes.Count];
            derValue[0] = 0x06;
            Array.Copy(lengthBytes, 0, derValue, 1, lengthBytes.Length);
            Array.Copy(bytes.ToArray(), 0, derValue, 1 + lengthBytes.Length, bytes.Count);

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnOid Decode(byte[] source, ref int pos)
        {
            AsnOid instance = new AsnOid();

            //CheckContextTag(source, ref pos);
            pos++;
            //Console.WriteLine("Object ID");
            // length and value in subsequent bytes
            int length = instance.GetLength(source, ref pos);
            byte[] raw = new byte[length];
            Array.Copy(source, pos, raw, 0, length);

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

            instance.value = new System.Security.Cryptography.Oid(oid);
            //Console.WriteLine(oid);
            //Console.WriteLine(value.FriendlyName);

            pos += length;

            return instance;
        }
    }
}
