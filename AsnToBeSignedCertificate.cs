using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace asn.core
{
    public class AsnToBeSignedCertificate : AsnBase
    {
        // ASN.1
        //  ToBeSignedCertificate           SEQUENCE
        //      version                     [0] AsnInteger.cs
        //      serialNumber                AsnInteger.cs
        //      signature                   AsnAlgorithmIdentifier.cs
        //      issuer                      AsnName.cs
        //      validity                    AsnValidity.cs
        //      subject                     AsnName.cs
        //      subjectPKInfo               AsnPublicKeyInfo.cs
        //      issuerUniqueId              [1] AsnBitstring.cs OPTIONAL
        //      subjectUniqueID             [2] AsnBitstring.cs OPTIONAL
        //      extensions                  [3] AsnExtensions.cs OPTIONAL
        public AsnInteger version;
        public AsnInteger serialNumber;
        public AsnAlgorithmIdentifier signature;
        public AsnName issuer;
        public AsnValidity validity;
        public AsnName subject;
        public AsnPublicKeyInfo subjectPKInfo;
        public AsnBitstring issuerUniqueID;
        public AsnBitstring subjectUniqueID;
        public AsnExtensions extensions;

        public AsnToBeSignedCertificate()
        {
            version = new AsnInteger(2);

            issuer = new AsnName();
            subject = new AsnName();

            //issuerUniqueID = new AsnBitstring();
            //issuerUniqueID.contextTag = 1;

            //subjectUniqueID = new AsnBitstring();
            //subjectUniqueID.contextTag = 2;

            //extensions = new AsnExtensions();
            //extensions.contextTag = 3;
        }

        public static AsnToBeSignedCertificate FromRequest(AsnCertificationRequest request)
        {
            AsnToBeSignedCertificate tbsCert = new AsnToBeSignedCertificate
            {
                subject = request.requestInfo.subject,
                subjectPKInfo = request.requestInfo.subjectPKInfo
            };

            // TODO: fill in the rest

            return tbsCert;
        }

        public int Encode()
        {
            int length = 0;
            int extensionsLength = 0;
            byte[] extensionsLengthBytes = null;

            int versionLength = version.Encode();
            byte[] versionLengthBytes = EncodeLength(versionLength);
            length += 1 + versionLengthBytes.Length;

            length += versionLength;
            length += serialNumber.Encode();
            length += signature.Encode();
            length += issuer.Encode();
            length += validity.Encode();
            length += subject.Encode();
            length += subjectPKInfo.Encode();

            // optional
            if (issuerUniqueID != null)
            {
                length += 1 + issuerUniqueID.Encode();
            }
            if (subjectUniqueID != null)
            {
                length += 1 + subjectUniqueID.Encode();
            }
            if (extensions != null)
            {
                extensionsLength = extensions.Encode();
                extensionsLengthBytes = EncodeLength(extensionsLength);
                length += 1 + extensionsLengthBytes.Length + extensionsLength;
            }

            byte[] lengthBytes = EncodeLength(length);
            derValue = new byte[1 + lengthBytes.Length + length];

            int d = 0;

            derValue[d] = 0x30; // sequence
            d++;

            Array.Copy(lengthBytes, 0, derValue, d, lengthBytes.Length);
            d += lengthBytes.Length;

            derValue[d] = 0xa0; // explicit tag
            d++;
            Array.Copy(versionLengthBytes, 0, derValue, d, versionLengthBytes.Length);
            d += versionLengthBytes.Length;

            Array.Copy(version.derValue, 0, derValue, d, version.derValue.Length);
            d += version.derValue.Length;
            Array.Copy(serialNumber.derValue, 0, derValue, d, serialNumber.derValue.Length);
            d += serialNumber.derValue.Length;
            Array.Copy(signature.derValue, 0, derValue, d, signature.derValue.Length);
            d += signature.derValue.Length;
            Array.Copy(issuer.derValue, 0, derValue, d, issuer.derValue.Length);
            d += issuer.derValue.Length;
            Array.Copy(validity.derValue, 0, derValue, d, validity.derValue.Length);
            d += validity.derValue.Length;
            Array.Copy(subject.derValue, 0, derValue, d, subject.derValue.Length);
            d += subject.derValue.Length;
            Array.Copy(subjectPKInfo.derValue, 0, derValue, d, subjectPKInfo.derValue.Length);
            d += subjectPKInfo.derValue.Length;

            if (issuerUniqueID != null)
            {
                derValue[d] = 0xa1;
                d++;

                Array.Copy(issuerUniqueID.derValue, 0, derValue, d, issuerUniqueID.derValue.Length);
                d += issuerUniqueID.derValue.Length;
            }
            if (subjectUniqueID != null)
            {
                derValue[d] = 0xa2;
                d++;

                Array.Copy(subjectUniqueID.derValue, 0, derValue, d, subjectUniqueID.derValue.Length);
                d += subjectUniqueID.derValue.Length;
            }
            if (extensions != null)
            {
                derValue[d] = 0xa3;
                d++;

                Array.Copy(extensionsLengthBytes, 0, derValue, d, extensionsLengthBytes.Length);
                d += extensionsLengthBytes.Length;

                Array.Copy(extensions.derValue, 0, derValue, d, extensions.derValue.Length);
                d += extensions.derValue.Length;
            }

            PrependContextTag();

            return derValue.Length;
        }

        public static AsnToBeSignedCertificate Decode(byte[] source, ref int pos)
        {
            AsnToBeSignedCertificate instance = new AsnToBeSignedCertificate();

            //instance.CheckContextTag(source, ref pos);
            pos++;

            int len = instance.GetLength(source, ref pos);

            // peek into the next byte to see if we have an explicit tag (should be there)
            if (source[pos] == 0xa0)
            {
                pos++;
                len = instance.GetLength(source, ref pos);
            }

            instance.version = AsnInteger.Decode(source, ref pos);
            instance.serialNumber = AsnInteger.Decode(source, ref pos);
            instance.signature = AsnAlgorithmIdentifier.Decode(source, ref pos);
            instance.issuer = AsnName.Decode(source, ref pos);
            instance.validity = AsnValidity.Decode(source, ref pos);
            instance.subject = AsnName.Decode(source, ref pos);
            instance.subjectPKInfo = AsnPublicKeyInfo.Decode(source, ref pos);

            if (source[pos] == 0xa1)
            {
                pos++;
                instance.GetLength(source, ref pos);
                instance.issuerUniqueID = AsnBitstring.Decode(source, ref pos);
            }
            if (source[pos] == 0xa2)
            {
                pos++;
                instance.GetLength(source, ref pos);
                instance.subjectUniqueID = AsnBitstring.Decode(source, ref pos);
            }
            if (source[pos] == 0xa3)
            {
                pos++;
                instance.GetLength(source, ref pos);
                instance.extensions = AsnExtensions.Decode(source, ref pos);
            }

            return instance;
        }

        // sign the TBS certificate with the CA key or intermediate key, returns the signed certificate ready for deployment
		public AsnCertificate Sign(AsnPrivateKeyPair key)
		{
            AsnCertificate cert = new AsnCertificate(this);

			Encode();

			RSA rsa = RSA.Create();
			rsa.ImportParameters(key.parameters);

			cert.signatureAlgorithm.algorithmID.value = new Oid("1.2.840.113549.1.1.11"); // sha256withRSA
			cert.signature = new AsnBitstring(rsa.SignData(derValue, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

			return cert;
		}

        public void ExtensionSubjectKeyIdentifier()
        {
            if (extensions == null) extensions = new AsnExtensions();

            subjectPKInfo.Encode();
            byte[] hash = SHA1.Create().ComputeHash(subjectPKInfo.publicKey.value);

            byte[] der = new byte[2 + hash.Length];
            der[0] = 0x04; // it's an octet string
            der[1] = (byte)hash.Length;
            Array.Copy(hash, 0, der, 2, hash.Length);

            AsnExtension extension = new AsnExtension(new AsnOid("2.5.29.14"), false, der);
            extensions.extensions.Add(extension);
        }

        public void ExtensionKeyUsage(bool signature, bool nonRepudiation, bool keyEncipherment, bool dataEncipherment, bool keyAgreement, bool keyCertSign, bool crlSign, bool encipherOnly, bool decipherOnly)
        {
            if (extensions == null) extensions = new AsnExtensions();

            ushort val = 0;
            byte unused = 0;

            // construct an octet string to put in the value of this extension
            if (signature) val |= 0x8000;
            if (nonRepudiation) val |= 0x4000;
            if (keyEncipherment) val |= 0x2000;
            if (dataEncipherment) val |= 0x1000;
            if (keyAgreement) val |= 0x0800;
            if (keyCertSign) val |= 0x0400;
            if (crlSign) val |= 0x0200;
            if (encipherOnly) val |= 0x0100;
            if (decipherOnly) val |= 0x0080;

            // find unused bits bit masking first bit and shift right
            int bitpos = 0;
            int bytesToCopy = 2;
            while ((val & (1 << bitpos)) == 0)
            {
                unused++;
                bitpos++;
            }

            if (unused > 8)
            {
                unused = (byte)(unused % 8);
                bytesToCopy--;
            }

            // store value as octet string
            byte[] bytes = new byte[bytesToCopy + 3];
            bytes[0] = 0x03; // this is a bitstring
            bytes[1] = (byte)(bytesToCopy + 1); // it's two bytes long
            bytes[2] = unused; // number of unused bits
            if (bytesToCopy == 1)
            {
				bytes[3] = (byte)((val & 0xff00) >> 8);
			}
            else
            {
                bytes[3] = (byte)((val & 0xff00) >> 8);
                bytes[4] = (byte)(val & 0xff);
            }

            AsnExtension extension = new AsnExtension(new AsnOid("2.5.29.15"), true, bytes);
            extensions.extensions.Add(extension);
        }

		// TODO: need to cater for multiple alternative names, and different types
		public void ExtensionSubjectAltName(string name)
		{
			if (extensions == null) extensions = new AsnExtensions();

            int pos = name.IndexOf(':');
            string typeName = name.Substring(0, pos);
            string nameValue = name.Substring(pos + 1);

            byte type = 0;
            switch (typeName)
            {
                case "Other":
                    type = 0;
                    break;
                case "rfc822":
                case "email":
                    type = 1;
                    break;
                case "DNS":
                    type = 2;
                    break;
                case "x400":
                    type = 3;
                    break;
                case "dirName":
                    type = 4;
                    break;
                case "ediParty":
                    type = 5;
                    break;
                case "URI":
                    type = 6;
                    break;
                case "IP":
                    type = 7;
                    break;
                case "OID":
                    type = 8;
                    break;
            }

			byte[] lengthBytes = EncodeLength(nameValue.Length);
			byte[] valueBytes = System.Text.Encoding.UTF8.GetBytes(nameValue);
			int length = nameValue.Length;

			byte[] nameDER = new byte[1 + lengthBytes.Length + valueBytes.Length];
			nameDER[0] = (byte)(0x80 | type);
			Array.Copy(lengthBytes, 0, nameDER, 1, lengthBytes.Length);
			Array.Copy(valueBytes, 0, nameDER, 1 + lengthBytes.Length, valueBytes.Length);

			byte[] bytes = new byte[nameDER.Length + 2];
			bytes[0] = 0x30; // it's a sequence
			bytes[1] = (byte)nameDER.Length;
			Array.Copy(nameDER, 0, bytes, 2, nameDER.Length);

			AsnExtension extension = new AsnExtension(new AsnOid("2.5.29.17"), false, bytes);
			extensions.extensions.Add(extension);
		}

		public void ExtensionIssuerAltName(byte type, string name)
		{
			if (extensions == null) extensions = new AsnExtensions();

			byte[] lengthBytes = EncodeLength(name.Length);
			byte[] valueBytes = System.Text.Encoding.UTF8.GetBytes(name);
			int length = name.Length;

			byte[] nameDER = new byte[1 + lengthBytes.Length + valueBytes.Length];
			nameDER[0] = (byte)(0x80 | type);
			Array.Copy(lengthBytes, 0, nameDER, 1, lengthBytes.Length);
			Array.Copy(valueBytes, 0, nameDER, 1 + lengthBytes.Length, valueBytes.Length);

			byte[] bytes = new byte[nameDER.Length + 2];
			bytes[0] = 0x30; // it's a sequence
			bytes[1] = (byte)nameDER.Length;
			Array.Copy(nameDER, 0, bytes, 2, nameDER.Length);

			AsnExtension extension = new AsnExtension(new AsnOid("2.5.29.18"), false, bytes);
			extensions.extensions.Add(extension);
		}

		// TODO: note 4.1.2.9 of RFC5280 - there are some additional requirements on CAs
		// especially regarding the optional path length stuff
		public void ExtensionBasicConstraints(bool ca)
        {
            if (extensions == null) extensions = new AsnExtensions();

            AsnBoolean caFlag = new AsnBoolean(ca);
            int length = caFlag.Encode();

			byte[] bytes = new byte[length + 2];
			bytes[0] = 0x30; // it's a sequence
			bytes[1] = (byte)length;
			Array.Copy(caFlag.derValue, 0, bytes, 2, caFlag.derValue.Length);

			AsnExtension extension = new AsnExtension(new AsnOid("2.5.29.19"), true, bytes);
            extensions.extensions.Add(extension);
        }

        public void ExtensionExtendedKeyUsage(bool serverAuth, bool clientAuth, bool codeSigning, bool emailProtection, bool timeStamping, bool ocspSigning)
        {
			if (extensions == null) extensions = new AsnExtensions();
            			
            AsnOid oid = new AsnOid("2.5.29.37");

            // maintain a list of OIDs for the uses
            List<AsnOid> uses = new List<AsnOid>();

            if (serverAuth) uses.Add(new AsnOid("1.3.6.1.5.5.7.3.1"));
			if (clientAuth) uses.Add(new AsnOid("1.3.6.1.5.5.7.3.2"));
			if (codeSigning) uses.Add(new AsnOid("1.3.6.1.5.5.7.3.3"));
			if (emailProtection) uses.Add(new AsnOid("1.3.6.1.5.5.7.3.4"));
            if (timeStamping) uses.Add(new AsnOid("1.3.6.1.5.5.7.3.8"));
			if (ocspSigning) uses.Add(new AsnOid("1.3.6.1.5.5.7.3.9"));

			int length = 0;
            foreach (AsnOid use in uses)
            {
                length += use.Encode();
            }

            byte[] lengthBytes = EncodeLength(length);

            byte[] der = new byte[1 + lengthBytes.Length + length];
            der[0] = 0x30; // it's a sequence
            int pos = 1;
            Array.Copy(lengthBytes, 0, der, pos, lengthBytes.Length);
            pos += lengthBytes.Length;
            foreach (AsnOid use in uses)
            {
                Array.Copy(use.derValue, 0, der, pos, use.derValue.Length);
                pos += use.derValue.Length;
            }

            AsnExtension extension = new AsnExtension(oid, false, der);
            extensions.extensions.Add(extension);
        }

        public void ExtensionAuthorityKeyIdentifier()
        {
			if (extensions == null) extensions = new AsnExtensions();
			AsnOid oid = new AsnOid("2.5.29.35");

            subjectPKInfo.Encode();
			byte[] hash = SHA1.Create().ComputeHash(subjectPKInfo.publicKey.value);

            byte[] der = new byte[4 + hash.Length];
            der[0] = 0x30; //it's a sequence
            der[1] = (byte)(hash.Length + 2);
            der[2] = 0x80; // context tag indicating option 0 (see 4.2.1.1 of RFC 5280)
            // I feel there should be a 0x04 here to indicate octet string but OpenSSL omits this
            der[3] = (byte)hash.Length;
            Array.Copy(der, 4, hash, 0, hash.Length);

			AsnExtension extension = new AsnExtension(oid, false, der);
			extensions.extensions.Add(extension);
		}
	}
}
