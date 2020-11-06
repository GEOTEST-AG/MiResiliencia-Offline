namespace PdfSharp.Pdf.Security
{
    internal enum EncryptionMode
    {
        Encrypt,
        Decrypt
    }

    internal interface IEncryptor
    {
        bool PasswordValid { get; }

        bool HaveOwnerPermission { get; }

        EncryptionMode Mode { get; set; }

        void Initialize(PdfDocument document, PdfDictionary encryptionDict);

        void InitEncryptionKey(string password);

        bool ValidatePassword(string password);

        void CreateHashKey(PdfObjectID objectId);

        byte[] Encrypt(byte[] bytes);
    }
}