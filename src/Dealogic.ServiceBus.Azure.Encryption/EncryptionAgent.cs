namespace Dealogic.ServiceBus.Azure.Encryption
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Encryption agent metadata
    /// </summary>
    internal sealed class EncryptionAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionAgent"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <param name="algorithm">The algorithm.</param>
        public EncryptionAgent(string protocol, EncryptionAlgorithm algorithm)
        {
            this.Protocol = protocol;
            this.EncryptionAlgorithm = algorithm;
        }

        /// <summary>
        /// Gets or sets the encryption algorithm.
        /// </summary>
        /// <value>The encryption algorithm.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public EncryptionAlgorithm EncryptionAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the protocol.
        /// </summary>
        /// <value>The protocol.</value>
        public string Protocol { get; set; }
    }
}