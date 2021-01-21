using System;

namespace asn.core
{
    public class AsnExtension : AsnBase
    {
        public AsnOid extnID;
        public AsnBoolean critical;
        public AsnOctetstring extnValue;

        public AsnExtension()
        {
            
        }

        public AsnExtension(AsnOid oid, bool isCritical, byte[] value)
        {
			extnID = oid;
			critical = new AsnBoolean(isCritical);
			extnValue = new AsnOctetstring(value);
		}

        public AsnExtension(string oid, bool isCritical, byte[] value)
        {
            extnID = new AsnOid(oid);
            critical = new AsnBoolean(isCritical);
            extnValue = new AsnOctetstring(value);
        }

        public int Encode()
        {
            int length = 0;

            length += extnID.Encode();
            if (critical != null)
            {
                length += critical.Encode();
            }
            length += extnValue.Encode();

            byte[] lengthBytes = EncodeLength(length);
            derValue = new byte[1 + lengthBytes.Length + length];

            derValue[0] = 0x30; // sequence

            int d = 1;
            Array.Copy(lengthBytes, 0, derValue, d, lengthBytes.Length);
            d += lengthBytes.Length;

            Array.Copy(extnID.derValue, 0, derValue, d, extnID.derValue.Length);
            d += extnID.derValue.Length;
            if (critical != null)
            {
                Array.Copy(critical.derValue, 0, derValue, d, critical.derValue.Length);
                d += critical.derValue.Length;
            }
            Array.Copy(extnValue.derValue, 0, derValue, d, extnValue.derValue.Length);
            d += extnValue.derValue.Length;

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnExtension Decode(byte[] source, ref int pos)
        {
            AsnExtension instance = new AsnExtension();

            pos++;

            long len = instance.GetLength(source, ref pos);

            instance.extnID = AsnOid.Decode(source, ref pos);

            if (source[pos] == 0x1)
            {
                instance.critical = AsnBoolean.Decode(source, ref pos);
            }

            instance.extnValue = AsnOctetstring.Decode(source, ref pos);

            return instance;
        }
    }
}
