using PdfSharp.Pdf.Internal;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PdfSharp.Pdf.Security
{
    internal sealed class AESEncryptor : EncryptorBase, IEncryptor
    {
        private Random rand = new Random((int)DateTime.UtcNow.Ticks);

        /// <summary>
        /// Initializes the encryptor for encryption
        /// </summary>
        /// <param name="document"></param>
        /// <param name="mode"></param>
        public AESEncryptor(PdfDocument document, EncryptionMode mode)
        {
            Mode = mode;

            Initialize(document, document.SecurityHandler);

            if (mode == EncryptionMode.Encrypt)
            {
                PrepareEncryptionKeys(document.SecuritySettings.SecurityHandler._ownerPassword,
                    document.SecuritySettings.SecurityHandler._userPassword,
                    (int)document.SecuritySettings.SecurityHandler.Permission, false);

                document.SecurityHandler.Elements.Clear();
                document.SecurityHandler.Elements.SetName(PdfSecurityHandler.Keys.Filter, "/Standard");
                document.SecurityHandler.Elements.SetInteger(PdfSecurityHandler.Keys.V, 5);
                document.SecurityHandler.Elements.SetInteger(PdfSecurityHandler.Keys.Length, 256);
                document.SecurityHandler.Elements.SetInteger(PdfStandardSecurityHandler.Keys.R, 6);
                document.SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.O, PdfEncoders.RawEncoding.GetString(ownerValue));
                document.SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.U, PdfEncoders.RawEncoding.GetString(userValue));
                document.SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.UE, PdfEncoders.RawEncoding.GetString(ueValue));
                document.SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.OE, PdfEncoders.RawEncoding.GetString(oeValue));
                document.SecurityHandler.Elements.SetString(PdfStandardSecurityHandler.Keys.Perms, PdfEncoders.RawEncoding.GetString(permsValue));

                var cfDict = new PdfDictionary(doc);
                var filterDict = new PdfDictionary(doc);
                filterDict.Elements.SetName(PdfSecurityHandler.Keys.CFM, "/AESV3");
                filterDict.Elements.SetInteger(PdfSecurityHandler.Keys.Length, 256);
                filterDict.Elements.SetName("/AuthEvent", "DocOpen");
                cfDict.Elements["/StdCF"] = filterDict;
                document.SecurityHandler.Elements[PdfSecurityHandler.Keys.CF] = cfDict;
                document.SecurityHandler.Elements.SetName(PdfSecurityHandler.Keys.StrF, "/StdCF");
                document.SecurityHandler.Elements.SetName(PdfSecurityHandler.Keys.StmF, "/StdCF");
            }
        }

        /// <summary>
        /// Prepares decryption of a document using the supplied password
        /// </summary>
        /// <param name="password"></param>
        public void InitEncryptionKey(string password)
        {
            PasswordValid = HaveOwnerPermission = false;

            if (rValue == 4)
                InitR4(password);
            else if (rValue == 5)
                InitR5(password);
            else if (rValue == 6)
                InitR6(password);
            else
                throw new PdfSharpException("Unsupported encryption revision " + rValue);
        }

        private void InitR4(string password)
        {
            // didn't want to copy code and also didn't want to inherit from RC4Encryptor, so use this kinda "hackish" way... TODO: cleanup
            var rce = new RC4Encryptor(doc, EncryptionMode.Decrypt);
            rce.ValidatePassword(password);
            computedOwnerValue = rce.computedOwnerValue;
            computedUserValue = rce.computedUserValue;
            encryptionKey = rce.encryptionKey;
            HaveOwnerPermission = rce.HaveOwnerPermission;
            PasswordValid = rce.PasswordValid;
        }

        /// <summary>
        /// Pdf Reference 1.7 Extension Level 3, Chapter 3.5.2, Algorithm 3.2a
        /// </summary>
        /// <param name="password"></param>
        private void InitR5(string password)
        {
            // TODO: apply SASLprep to the password (RFC 4013, RFC 3454)
            var pwdBytes = Encoding.UTF8.GetBytes(password).Take(127).ToArray();
            // split O and U into their components
            var oHash = new byte[32];
            var oValidation = new byte[8];
            var oSalt = new byte[8];
            var uHash = new byte[32];
            var uValidation = new byte[8];
            var uSalt = new byte[8];

            Array.Copy(ownerValue, oHash, 32);
            Array.Copy(ownerValue, 32, oValidation, 0, 8);
            Array.Copy(ownerValue, 40, oSalt, 0, 8);
            Array.Copy(userValue, uHash, 32);
            Array.Copy(userValue, 32, uValidation, 0, 8);
            Array.Copy(userValue, 40, uSalt, 0, 8);

            var oKeyBytes = new byte[pwdBytes.Length + 8 + 48];
            Array.Copy(pwdBytes, oKeyBytes, pwdBytes.Length);
            Array.Copy(oValidation, 0, oKeyBytes, pwdBytes.Length, 8);
            Array.Copy(userValue, 0, oKeyBytes, pwdBytes.Length + 8, 48);

            computedOwnerValue = new byte[32];
            var haveOwnerPassword = PasswordMatchR5(oKeyBytes, ownerValue, computedOwnerValue);
            if (haveOwnerPassword)
            {
                PasswordValid = true;
                HaveOwnerPermission = true;
                CreateEncryptionKeyR5(pwdBytes, oSalt, userValue, oeValue);
            }
            else
            {
                oKeyBytes = new byte[pwdBytes.Length + 8];
                Array.Copy(pwdBytes, oKeyBytes, pwdBytes.Length);
                Array.Copy(uValidation, 0, oKeyBytes, pwdBytes.Length, 8);

                computedUserValue = new byte[32];
                // if the result matches the first 32 bytes of userValue, we have the user password
                var haveUserPassword = PasswordMatchR5(oKeyBytes, userValue, computedUserValue);
                if (haveUserPassword)
                {
                    PasswordValid = true;
                    CreateEncryptionKeyR5(pwdBytes, uSalt, null, ueValue);
                }
            }
        }

        private void CreateEncryptionKeyR5(byte[] password, byte[] salt, byte[] uservalue, byte[] decryptThis)
        {
            using (var sha = SHA256.Create())
            {
                using (var aes256Cbc = new AesCryptoServiceProvider { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None })
                {
                    var bufLen = password.Length + salt.Length + (uservalue != null ? 48 : 0);
                    var buf = new byte[bufLen];
                    Array.Copy(password, buf, password.Length);
                    Array.Copy(salt, 0, buf, password.Length, salt.Length);
                    if (uservalue != null)
                        Array.Copy(uservalue, 0, buf, password.Length + salt.Length, 48);
                    var shaKey = sha.ComputeHash(buf);
                    using (var decryptor = aes256Cbc.CreateDecryptor(shaKey, new byte[16]))
                    {
                        encryptionKey = new byte[32];
                        decryptor.TransformBlock(decryptThis, 0, decryptThis.Length, encryptionKey, 0);
                    }
                }
            }
        }

        private void InitR6(string password)
        {
            // split O and U into their components
            var oSalt = new byte[8];
            var uSalt = new byte[8];
            var uKeySalt = new byte[8];
            var oKeySalt = new byte[8];
            var oKey = new byte[48];
            Array.Copy(ownerValue, 32, oSalt, 0, 8);
            Array.Copy(userValue, 32, uSalt, 0, 8);
            Array.Copy(userValue, 40, uKeySalt, 0, 8);
            Array.Copy(ownerValue, 40, oKeySalt, 0, 8);
            Array.Copy(userValue, oKey, 48);

            computedUserValue = new byte[32];
            computedOwnerValue = new byte[32];
            HardenedHashR6(password, uSalt, null, computedUserValue);
            HardenedHashR6(password, oSalt, oKey, computedOwnerValue);

            byte[] keyToDecrypt = null;
            byte[] salt = null;
            byte[] hashKey = null;
            if (CompareArrays(computedOwnerValue, ownerValue, 32))
            {
                keyToDecrypt = oeValue;
                salt = oKeySalt;
                hashKey = oKey;
                PasswordValid = true;
                HaveOwnerPermission = true;
            }
            else if (CompareArrays(computedUserValue, userValue, 32))
            {
                keyToDecrypt = ueValue;
                salt = uKeySalt;
                PasswordValid = true;
            }

            var iv = new byte[16];
            if (keyToDecrypt != null)
            {
                encryptionKey = new byte[32];
                var hash = new byte[32];
                HardenedHashR6(password, salt, hashKey, hash);
                using (var aes256 = new AesCryptoServiceProvider { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None })
                {
                    using (var decryptor = aes256.CreateDecryptor(hash, iv))
                    {
                        decryptor.TransformBlock(keyToDecrypt, 0, 32, encryptionKey, 0);
                    }
                }
            }
            // decrypt the Perms value
            var permsDec = new byte[16];
            using (var aes256 = new AesCryptoServiceProvider { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None })
            {
                using (var decryptor = aes256.CreateDecryptor(encryptionKey, iv))
                {
                    decryptor.TransformBlock(permsValue, 0, 16, permsDec, 0);
                }
            }
            Debug.Assert(permsDec[9] == (byte)'a');
            Debug.Assert(permsDec[10] == (byte)'d');
            Debug.Assert(permsDec[11] == (byte)'b');
            var pTemp = BitConverter.ToInt32(permsDec, 0);
            Debug.Assert(pTemp == pValue);
        }

        // http://esec-lab.sogeti.com/posts/2011/09/14/the-undocumented-password-validation-algorithm-of-adobe-reader-x.html
        /// <summary>
        /// Compute a hardened hash for revision 6 documents
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <param name="salt">Salt value for the encryption</param>
        /// <param name="userData">Additional data to be hashed, may be null or an 48-byte array</param>
        /// <param name="hash">Receives the computed hash (32 bytes)</param>
        private void HardenedHashR6(string password, byte[] salt, byte[] userData, byte[] hash)
        {
            var data = new byte[(128 + 64 + 48) * 64];  // (max pwd length + max hash size + userData size) * rounds
            var block = new byte[64];
            var blockSize = 32;
            var dataLen = 0;

            using (var aes128 = new AesCryptoServiceProvider { BlockSize = 16 * 8, Mode = CipherMode.CBC, Padding = PaddingMode.None })
            {
                var pwdBytes = Encoding.UTF8.GetBytes(password).Take(127).ToArray();
                var iv = new byte[16];
                var aesKey = new byte[16];
                int i = 0, j, sum;

                using (var sha256 = SHA256.Create())
                {
                    sha256.TransformBlock(pwdBytes, 0, pwdBytes.Length, pwdBytes, 0);
                    sha256.TransformBlock(salt, 0, salt.Length, salt, 0);
                    if (userData != null)
                        sha256.TransformBlock(userData, 0, userData.Length, userData, 0);
                    sha256.TransformFinalBlock(salt, 0, 0);
                    Array.Copy(sha256.Hash, block, sha256.HashSize / 8);
                }
                while (i < 64 || i < data[dataLen * 64 - 1] + 32)
                {
                    Array.Copy(pwdBytes, data, pwdBytes.Length);
                    Array.Copy(block, 0, data, pwdBytes.Length, blockSize);
                    if (userData != null)
                        Array.Copy(userData, 0, data, pwdBytes.Length + blockSize, 48);
                    dataLen = pwdBytes.Length + blockSize + (userData != null ? 48 : 0);
                    // instead of looping 64 times, we expand our data 64 times and transform that once
                    for (j = 1; j < 64; j++)
                        Array.Copy(data, 0, data, j * dataLen, dataLen);

                    Array.Copy(block, 16, iv, 0, 16);
                    Array.Copy(block, 0, aesKey, 0, 16);
                    using (var aesEnc = aes128.CreateEncryptor(aesKey, iv))
                    {
                        aesEnc.TransformBlock(data, 0, dataLen * 64, data, 0);
                    }

                    for (j = 0, sum = 0; j < 16; j++)
                        sum += data[j];

                    blockSize = 32 + (sum % 3) * 16;
                    switch (blockSize)
                    {
                        case 32:
                            using (var sha256 = SHA256.Create())
                            {
                                sha256.TransformBlock(data, 0, dataLen * 64, data, 0);
                                sha256.TransformFinalBlock(data, 0, 0);
                                Array.Copy(sha256.Hash, block, sha256.HashSize / 8);
                            }
                            break;
                        case 48:
                            using (var sha384 = SHA384.Create())
                            {
                                sha384.TransformBlock(data, 0, dataLen * 64, data, 0);
                                sha384.TransformFinalBlock(data, 0, 0);
                                Array.Copy(sha384.Hash, block, sha384.HashSize / 8);
                            }
                            break;
                        case 64:
                            using (var sha512 = SHA512.Create())
                            {
                                sha512.TransformBlock(data, 0, dataLen * 64, data, 0);
                                sha512.TransformFinalBlock(data, 0, 0);
                                Array.Copy(sha512.Hash, block, sha512.HashSize / 8);
                            }
                            break;
                    }
                    i++;
                }
            }
            Array.Copy(block, hash, 32);
        }

        private static bool PasswordMatchR5(byte[] key, byte[] comparand, byte[] outHash)
        {
            var sha = SHA256.Create();
            var hash = sha.ComputeHash(key);
            for (var i = 0; i < 32; i++)
            {
                if (hash[i] != comparand[i])
                {
                    return false;
                }
            }
            Array.Copy(hash, outHash, 32);
            return true;
        }

        public bool ValidatePassword(string password)
        {
            // password has already been verified during init
            return HaveOwnerPermission | PasswordValid;
        }

        /// <summary>
        /// Creates a key to encrypt a particular object.
        /// Pdf Reference 1.7, Chapter 7.6.2, Algorithm #1
        /// </summary>
        /// <param name="id"></param>
        public void CreateHashKey(PdfObjectID id)
        {
            if (rValue >= 5)
            {
                // encryption key is used "as is"
                if (key == null)
                {
                    key = new byte[encryptionKey.Length];
                    Array.Copy(encryptionKey, key, encryptionKey.Length);
                }
                return;
            }
            var objectId = new byte[5];
            md5.Initialize();
            // Split the object number and generation
            objectId[0] = (byte)id.ObjectNumber;
            objectId[1] = (byte)(id.ObjectNumber >> 8);
            objectId[2] = (byte)(id.ObjectNumber >> 16);
            objectId[3] = (byte)id.GenerationNumber;
            objectId[4] = (byte)(id.GenerationNumber >> 8);
            var salt = new byte[] { 0x73, 0x41, 0x6C, 0x54 };       // "sAlT"
            var k = new byte[encryptionKey.Length + 9];
            Array.Copy(encryptionKey, k, encryptionKey.Length);
            Array.Copy(objectId, 0, k, encryptionKey.Length, objectId.Length);
            Array.Copy(salt, 0, k, encryptionKey.Length + objectId.Length, salt.Length);
            key = md5.ComputeHash(k);
            keySize = encryptionKey.Length + 5;
            if (keySize > 16)
                keySize = 16;
        }

        public byte[] Encrypt(byte[] bytes)
        {
            if (Mode == EncryptionMode.Decrypt)
                return DecryptInternal(bytes);
            return EncryptInternal(bytes);
        }

        private byte[] EncryptInternal(byte[] bytes)
        {
            // create random init vector            
            var iv = new byte[16];
            rand.NextBytes(iv);

            var buf = new byte[bytes.Length + iv.Length];
            Array.Copy(iv, buf, iv.Length);
            // Padding is described in Chapter 7.6.2 of PdfReference 1.7
            using (var aes = new AesCryptoServiceProvider { BlockSize = 16 * 8, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
            {
                using (var encryptor = aes.CreateEncryptor(encryptionKey, iv))
                {
                    var offset = 0;
                    // make sure, we have a multiple of the block size
                    var maxLen = (bytes.Length / encryptor.InputBlockSize) * encryptor.InputBlockSize;
                    if (maxLen > 0)
                        offset = encryptor.TransformBlock(bytes, 0, maxLen, buf, 16) + 16;
                    var suffix = encryptor.TransformFinalBlock(bytes, maxLen, bytes.Length - maxLen);
                    var dataLength = offset + suffix.Length;
                    if (dataLength > buf.Length)
                    {
                        var tmp = new byte[dataLength];
                        Array.Copy(buf, tmp, buf.Length);
                        buf = tmp;
                    }
                    Array.Copy(suffix, 0, buf, offset, suffix.Length);
                }
            }
            return buf;
        }

        private byte[] DecryptInternal(byte[] bytes)
        {
            if (bytes.Length <= 16)
                return bytes;

            var iv = new byte[16];
            Array.Copy(bytes, iv, 16);
            // Pdf Reference 1.7, Section 7.6.2 :
            // "Strings and streams encrypted with AES shall use a padding scheme that is described in Internet RFC 2898, PKCS #5"
            var output = new byte[bytes.Length - 16];
            var dataLength = output.Length;
            // try other padding modes as well, just to accomodate for pdf-writers that take the spec not too seriously
            using (var aes = new AesCryptoServiceProvider { Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
            {
                using (var decryptor = aes.CreateDecryptor(key, iv))
                {
                    try
                    {
                        //var offset = bytes.Length - 16 > 16 ? decryptor.TransformBlock(bytes, 16, bytes.Length - 16, output, 0) : 0;
                        //var suffix = bytes.Length - 16 > 16 ? decryptor.TransformFinalBlock(bytes, 0, 0) : decryptor.TransformFinalBlock(bytes, 16, bytes.Length - 16);
                        var offset = decryptor.TransformBlock(bytes, 16, bytes.Length - 16, output, 0);
                        var suffix = decryptor.TransformFinalBlock(bytes, 0, 0);
                        Array.Copy(suffix, 0, output, offset, suffix.Length);
                        dataLength = offset + suffix.Length;
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                        return bytes;       // return unencrypted
                    }
                }
            }
            return output.Take(dataLength).ToArray();
        }

        /// <summary>
        /// Creates the keys for Revision 6 encryption (U,O,UE,OE,...)
        /// Pdf Reference 1.7, Extension Level 3, algorithms 3.8 to 3.10
        /// </summary>
        /// <param name="ownerPassword"></param>
        /// <param name="userPassword"></param>
        /// <param name="permission"></param>
        /// <param name="encryptMetadata"></param>
        public void PrepareEncryptionKeys(string ownerPassword, string userPassword, int permission, bool encryptMetadata)
        {
            var oSalt = new byte[8];
            var uSalt = new byte[8];
            var uKey = new byte[8];
            var oKey = new byte[8];
            rand.NextBytes(oSalt);
            rand.NextBytes(uSalt);
            rand.NextBytes(oKey);
            rand.NextBytes(uKey);

            // Correct permission bits
            permission |= (int)(rValue >= 3 ? (uint)0xfffff0c0 : (uint)0xffffffc0);
            permission &= unchecked((int)0xfffffffc);
            pValue = permission;

            this.encryptMetadata = encryptMetadata;

            if (String.IsNullOrEmpty(userPassword))
                userPassword = "";
            if (String.IsNullOrEmpty(ownerPassword))
                ownerPassword = userPassword;
            userValue = new byte[48];
            ownerValue = new byte[48];
            ueValue = new byte[32];
            oeValue = new byte[32];
            // random file encrption key
            encryptionKey = new byte[32];
            rand.NextBytes(encryptionKey);

            // compute /U value
            HardenedHashR6(userPassword, uSalt, null, userValue);
            Array.Copy(uSalt, 0, userValue, 32, 8);
            Array.Copy(uKey, 0, userValue, 40, 8);
            // compute /O value
            HardenedHashR6(ownerPassword, oSalt, userValue, ownerValue);
            Array.Copy(oSalt, 0, ownerValue, 32, 8);
            Array.Copy(oKey, 0, ownerValue, 40, 8);

            var iv = new byte[16];
            var uk = new byte[32];
            var ok = new byte[32];
            HardenedHashR6(userPassword, uKey, null, uk);
            HardenedHashR6(ownerPassword, oKey, userValue, ok);

            // compute /UE value
            using (var aes256 = new AesCryptoServiceProvider { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None })
            {
                using (var decryptor = aes256.CreateEncryptor(uk, iv))
                {
                    decryptor.TransformBlock(encryptionKey, 0, 32, ueValue, 0);
                }
            }
            // compute /OE value
            using (var aes256 = new AesCryptoServiceProvider { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None })
            {
                using (var decryptor = aes256.CreateEncryptor(ok, iv))
                {
                    decryptor.TransformBlock(encryptionKey, 0, 32, oeValue, 0);
                }
            }
            // compute /Perms value
            permsValue = new byte[16];
            var perms = new byte[16];
            var pval = BitConverter.GetBytes(permission);
            Array.Copy(pval, perms, pval.Length);
            perms[4] = perms[5] = perms[6] = perms[7] = 0xff;
            // The specification (1.7 Extension Level 3, Chapter 3.5.2) is contradictory on this one
            // Algorithm 3.10 states, this should be the ASCII values 'T' or 'F'
            // Algorithm 3.13 states, this should be a boolean value
            // It seems, algo 3.13 is the correct one...
            perms[8] = encryptMetadata ? (byte)1 : (byte)0;
            perms[9] = (byte)'a';
            perms[10] = (byte)'d';
            perms[11] = (byte)'b';
            using (var aes256 = new AesCryptoServiceProvider { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None })
            {
                using (var decryptor = aes256.CreateEncryptor(encryptionKey, iv))
                {
                    decryptor.TransformBlock(perms, 0, 16, permsValue, 0);
                }
            }
        }
    }
}