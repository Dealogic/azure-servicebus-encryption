namespace Dealogic.ServiceBus.Azure.Encryption
{
    /// <summary>
    /// Encryption metadata
    /// </summary>
    internal class EncryptionData
    {
        /// <summary>
        /// Gets or sets the content encryption Initialization vector.
        /// </summary>
        /// <value>The content encryption Initialization vector.</value>
        public byte[] ContentEncryptionIV { get; set; }

        /// <summary>
        /// Gets or sets the encryption agent.
        /// </summary>
        /// <value>The encryption agent.</value>
        public EncryptionAgent EncryptionAgent { get; set; }

        /// <summary>
        /// Gets or sets the wrapped content (AES) key.
        /// </summary>
        /// <value>The wrapped content key.</value>
        public WrappedKey WrappedContentKey { get; set; }
    }
}