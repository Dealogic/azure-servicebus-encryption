namespace Dealogic.ServiceBus.Azure.Encryption
{
    /// <summary>
    /// Wrapped key
    /// </summary>
    internal class WrappedKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WrappedKey"/> class.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <param name="algorithm">The algorithm.</param>
        public WrappedKey(string keyId, byte[] encryptedKey, string algorithm)
        {
            this.KeyId = keyId;
            this.EncryptedKey = encryptedKey;
            this.Algorithm = algorithm;
        }

        /// <summary>
        /// Gets or sets the algorithm.
        /// </summary>
        /// <value>The algorithm.</value>
        public string Algorithm { get; set; }

        /// <summary>
        /// Gets or sets the encrypted key.
        /// </summary>
        /// <value>The encrypted key.</value>
        public byte[] EncryptedKey { get; set; }

        /// <summary>
        /// Gets or sets the key identifier.
        /// </summary>
        /// <value>The key identifier.</value>
        public string KeyId { get; set; }
    }
}