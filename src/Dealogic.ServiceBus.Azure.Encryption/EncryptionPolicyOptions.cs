namespace Dealogic.ServiceBus.Azure.Encryption
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.KeyVault.Core;

    /// <summary>
    /// Encryption policy configuration options
    /// </summary>
    public class EncryptionPolicyOptions
    {
        /// <summary>
        /// Gets or sets the key resolver. This property has to be set for decryption.
        /// </summary>
        /// <value>The key resolver.</value>
        public IKeyResolver KeyResolver { get; set; }

        /// <summary>
        /// Gets or sets the encryption key. This property has to be set for encryption. If no key
        /// resolver has been set, the logic will try to use this key for decryption as well.
        /// </summary>
        /// <value>The encryption key.</value>
        public Func<CancellationToken, Task<IKey>> EncryptionKey { get; set; }

        /// <summary>
        /// Validates the options.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Neither EncryptionKey nor KeyResovler has been set
        /// </exception>
        public virtual void Validate()
        {
            if (this.EncryptionKey == null & this.KeyResolver == null)
            {
                throw new ArgumentException($"Neither {nameof(this.EncryptionKey)} nor {nameof(this.KeyResolver)} has been set.");
            }
        }
    }
}