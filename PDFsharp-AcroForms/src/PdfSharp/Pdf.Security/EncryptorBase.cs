using PdfSharp.Pdf.Internal;
using System;
using System.Security.Cryptography;

namespace PdfSharp.Pdf.Security
{
    internal abstract class EncryptorBase
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly MD5 md5 = new MD5CryptoServiceProvider();

        /// <summary>
        /// The encryption key for a particular object/generation.
        /// </summary>
        internal byte[] key;

        /// <summary>
        /// The global encryption key.
        /// </summary>
        internal byte[] encryptionKey;

        internal byte[] computedOwnerValue;

        internal byte[] computedUserValue;

        /// <summary>
        /// The /O value as read from the input document
        /// </summary>
        internal byte[] ownerValue;

        /// <summary>
        /// The /U value as read from the input document
        /// </summary>
        internal byte[] userValue;

        internal byte[] documentId;

        internal byte[] oeValue;

        internal byte[] ueValue;

        internal byte[] permsValue;

        internal int pValue;

        internal int rValue;

        internal int vValue;

        protected bool encryptMetadata;

        protected PdfDictionary cf;

        protected string stmF;

        protected string strF;

        protected int keyLength;

        protected byte[] passwordPadding = new byte[] // 32 bytes password padding defined by Adobe
            {
                0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08,
                0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A,
            };


        /// <summary>
        /// The encryption key length for a particular object/generation.
        /// </summary>
        protected int keySize;

        protected PdfDocument doc;

        protected PdfDictionary encryptionDict;

        public bool PasswordValid { get; protected set; }

        public bool HaveOwnerPermission { get; protected set; }

        /// <summary>
        /// Gets or sets the current encryption mode
        /// </summary>
        public EncryptionMode Mode { get; set; }


        public void Initialize(PdfDocument document, PdfDictionary encryptionDictionary)
        {
            doc = document;
            encryptionDict = encryptionDictionary;

            documentId = PdfEncoders.RawEncoding.GetBytes(doc.Internals.FirstDocumentID);
            ownerValue = PdfEncoders.RawEncoding.GetBytes(encryptionDict.Elements.GetString(PdfStandardSecurityHandler.Keys.O));
            userValue = PdfEncoders.RawEncoding.GetBytes(encryptionDict.Elements.GetString(PdfStandardSecurityHandler.Keys.U));
            oeValue = PdfEncoders.RawEncoding.GetBytes(encryptionDict.Elements.GetString(PdfStandardSecurityHandler.Keys.OE));
            ueValue = PdfEncoders.RawEncoding.GetBytes(encryptionDict.Elements.GetString(PdfStandardSecurityHandler.Keys.UE));
            permsValue = PdfEncoders.RawEncoding.GetBytes(encryptionDict.Elements.GetString(PdfStandardSecurityHandler.Keys.Perms));
            pValue = encryptionDict.Elements.GetInteger(PdfStandardSecurityHandler.Keys.P);
            rValue = encryptionDict.Elements.GetInteger(PdfStandardSecurityHandler.Keys.R);
            vValue = encryptionDict.Elements.GetInteger(PdfSecurityHandler.Keys.V);
            encryptMetadata = !encryptionDict.Elements.ContainsKey(PdfStandardSecurityHandler.Keys.EncryptMetadata) || encryptionDict.Elements.GetBoolean(PdfStandardSecurityHandler.Keys.EncryptMetadata);
            cf = encryptionDict.Elements.GetDictionary(PdfSecurityHandler.Keys.CF);
            stmF = encryptionDict.Elements.GetString(PdfSecurityHandler.Keys.StmF);
            strF = encryptionDict.Elements.GetString(PdfSecurityHandler.Keys.StrF);
            keyLength = encryptionDict.Elements.GetInteger(PdfSecurityHandler.Keys.Length) / 8;     // specified in Bits
            keySize = keyLength;
        }

        /// <summary>
        /// Pads a password to a 32 byte array.
        /// </summary>
        protected byte[] PadPassword(string password)
        {
            var padded = new byte[32];
            if (password == null)
                Array.Copy(passwordPadding, 0, padded, 0, 32);
            else
            {
                int length = password.Length;
                Array.Copy(PdfEncoders.RawEncoding.GetBytes(password), 0, padded, 0, Math.Min(length, 32));
                if (length < 32)
                    Array.Copy(passwordPadding, 0, padded, length, 32 - length);
            }
            return padded;
        }

        /// <summary>
        /// Compares the first 'length' bytes of two byte-arrays
        /// </summary>
        /// <returns></returns>
        protected bool CompareArrays(byte[] left, byte[] right, int length)
        {
            for (var i = 0; i < length; i++)
            {
                if (left[i] != right[i])
                    return false;
            }
            return true;
        }
    }
}
