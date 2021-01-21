namespace asn.core
{
    public class AsnNull : AsnBase
    {
        public int Encode()
        {
            derValue = new byte[2];

            derValue[0] = 5;
            derValue[1] = 0;

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnNull Decode(byte[] source, ref int pos)
        {
            AsnNull instance = new AsnNull();

            //CheckContextTag(source, ref pos);
            pos++;

            // ignore length
            pos++;

            return instance;
        }
    }
}
