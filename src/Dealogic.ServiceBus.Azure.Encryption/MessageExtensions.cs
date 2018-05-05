namespace Microsoft.Azure.ServiceBus
{
    using System.Threading;
    using System.Threading.Tasks;
    using Dealogic.ServiceBus.Azure.Encryption;

    /// <summary>
    /// Brokered message policy extensions
    /// </summary>
    public static class MessageExtensions
    {
        /// <summary>
        /// Encrypts the brokered message's content asynchronously. If no encryption policy is set,
        /// the message wont be modified.
        /// </summary>
        /// <param name="brokeredMessage">The brokered message.</param>
        /// <param name="encryptionPolicy">The encryption policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The encrypted message.</returns>
        public static Task<Message> EncryptAsync(this Message brokeredMessage, EncryptionPolicy encryptionPolicy, CancellationToken cancellationToken)
        {
            if (encryptionPolicy != null)
            {
                return encryptionPolicy.EncryptMessageAsync(brokeredMessage, cancellationToken);
            }

            return Task.FromResult(brokeredMessage);
        }

        /// <summary>
        /// Decrypts the brokered message's content asynchronously If no encryption policy is set,
        /// the message wont be modified.
        /// </summary>
        /// <param name="brokeredMessage">The brokered message.</param>
        /// <param name="encryptionPolicy">The encryption policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decrypted message.</returns>
        public static Task<Message> DecryptAsync(this Message brokeredMessage, EncryptionPolicy encryptionPolicy, CancellationToken cancellationToken)
        {
            if (encryptionPolicy != null)
            {
                return encryptionPolicy.DecryptMessageAsync(brokeredMessage, cancellationToken);
            }

            return Task.FromResult(brokeredMessage);
        }
    }
}