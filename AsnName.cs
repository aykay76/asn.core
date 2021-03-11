using System;
using System.Collections.Generic;

namespace asn.core
{
    public class AsnName : AsnSequence
    {
        //  Name                    SEQUENCE
        //      RDN                     SET <List>
        //          TypeandValue        AttributeTypeAndValue.cs
        //              Type            AsnOid.cs
        //              Value           AsnString.cs 

        public AsnName() : base()
        {
        }

        // public AsnName(string DN)
        // {
        //     elements.Add(new AsnRelativeDistinguishedName());

        //     string[] parts = DN.Split(',', StringSplitOptions.None);
        //     foreach (string part in parts)
        //     {
        //         string[] sections = part.Split('=', StringSplitOptions.None);
        //         AsnAttributeTypeAndValue rdn = new AsnAttributeTypeAndValue(AsnOid.FromFriendlyName(sections[0]), new AsnString(sections[1], AsnType.UniversalTag.PrintableString));
        //         set.Add(rdn);
        //         RDN.
        //     }
        // }

        public override int Encode()
        {
            base.Encode();

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnName Decode(byte[] source, ref int pos)
        {
            AsnName instance = new AsnName();

            //CheckContextTag(source, ref pos);

            // skip the 0x30 (SEQUENCE)
            pos++;

            // sequence length
            long length = instance.GetLength(source, ref pos);

            // now we should see a set and length of the set
            long start = pos;

            // decode the set
            while (pos < start + length)
            {
                AsnRelativeDistinguishedName name = AsnRelativeDistinguishedName.Decode(source, ref pos);
                instance.elements.Add(name);
            }

            return instance;
        }
    }
}
