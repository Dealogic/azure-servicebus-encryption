namespace Dealogic.ServiceBus.Azure.Encryption
{
    /// <summary>
    /// Encryption algorithm type
    /// </summary>
    internal enum EncryptionAlgorithm
    {
        /// <summary>
        /// The AES 256 encryption
        /// </summary>
        AES_CBC_256,

        /// <summary>
        /// The AES GCM 256 encryption
        /// </summary>
        AES_GCM_256
    }
}