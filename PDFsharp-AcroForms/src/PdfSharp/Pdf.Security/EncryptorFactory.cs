using System;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Security
{
    internal class EncryptorFactory
    {
        /// <summary>
        /// Prepares decryption of a loaded document
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="dict"></param>
        /// <param name="stringEncryptor"></param>
        /// <param name="streamEncryptor"></param>
        public static void InitDecryption(PdfDocument doc, PdfDictionary dict, out IEncryptor stringEncryptor, out IEncryptor streamEncryptor)
        {
            stringEncryptor = streamEncryptor = null;

            var filter = dict.Elements.GetName(PdfSecurityHandler.Keys.Filter);
            var v = dict.Elements.GetInteger(PdfSecurityHandler.Keys.V);
            if (filter != "/Standard" || !(v >= 1 && v <= 5))
                throw new PdfReaderException(PSSR.UnknownEncryption);

            foreach (var keyName in new[] { PdfSecurityHandler.Keys.StrF, PdfSecurityHandler.Keys.StmF })
            {
                IEncryptor encryptor = null;
                if (v >= 4)
                {
                    var cf = dict.Elements.GetDictionary(PdfSecurityHandler.Keys.CF);
                    if (cf != null)
                    {
                        var filterName = dict.Elements.GetName(keyName);
                        if (!String.IsNullOrEmpty(filterName))
                        {
                            /*
                             * Pdf Reference 1.7 Chapter 7.6.5 (Crypt Filters)
                             * 
                             * None:  The application shall not decrypt data but shall direct the input stream to the security handler for decryption.
                             * V2:    The application shall ask the security handler for the encryption key and shall implicitly decrypt data with 
                             *        "Algorithm 1: Encryption of data using the RC4 or AES algorithms", using the RC4 algorithm.
                             * AESV2: (PDF 1.6)The application shall ask the security handler for the encryption key and shall implicitly decrypt data with 
                             *        "Algorithm 1: Encryption of data using the RC4 or AES algorithms", using the AES algorithm in Cipher Block 
                             *        Chaining (CBC) mode with a 16-byte block size and an initialization vector that shall be randomly generated and 
                             *        placed as the first 16 bytes in the stream or string.
                             * AESV3: (PDF 1.7, ExtensionLevel 3) The application asks the security handler for the encryption key and implicitly decrypts data with 
                             *        Algorithm 3.1a, using the AES-256 algorithm in Cipher Block Chaining (CBC) with padding mode with a 16-byte block size and an 
                             *        initialization vector that is randomly generated and placed as the first 16 bytes in the stream or string. 
                             *        The key size (Length) shall be 256 bits.
                             */
                            var filterDict = cf.Elements.GetDictionary(filterName);
                            if (filterDict != null)
                            {
                                var cfm = filterDict.Elements.GetName(PdfSecurityHandler.Keys.CFM);
                                if (!String.IsNullOrEmpty(cfm) && cfm.StartsWith("/AESV")) // AESV2(PDF 1.6), AESV3(PDF 1.7, ExtensionLevel 3)
                                    encryptor = new AESEncryptor(doc, EncryptionMode.Decrypt);
                            }
                        }
                    }
                }
                // If CFM is "V2", use RC4 encryption
                if (encryptor == null)
                    encryptor = new RC4Encryptor(doc, EncryptionMode.Decrypt);
                if (keyName == PdfSecurityHandler.Keys.StrF)
                    stringEncryptor = encryptor;
                else
                    streamEncryptor = encryptor;
            }
        }

        /// <summary>
        /// Prepares encryption of a document that is about to be saved
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static IEncryptor InitEncryption(PdfDocument doc, PdfDictionary dict)
        {
            if (doc.SecuritySettings.DocumentSecurityLevel < PdfDocumentSecurityLevel.EncryptedAES256)
                return new RC4Encryptor(doc, EncryptionMode.Encrypt);
            return new AESEncryptor(doc, EncryptionMode.Encrypt);
        }
    }
}
