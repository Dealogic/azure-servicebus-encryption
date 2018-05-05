namespace Dealogic.ServiceBus.Azure.Encryption
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;

    /// <summary>
    /// Encryption plugin
    /// </summary>
    /// <seealso cref="Microsoft.Azure.ServiceBus.Core.ServiceBusPlugin"/>
    public class MessageBodyEncryptionPlugin : ServiceBusPlugin
    {
        private readonly EncryptionPolicy encryptionPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBodyEncryptionPlugin"/> class.
        /// </summary>
        /// <param name="encryptionPolicy">The encryption policy.</param>
        /// <exception cref="System.ArgumentNullException">encryptionPolicy is null.</exception>
        public MessageBodyEncryptionPlugin(EncryptionPolicy encryptionPolicy)
        {
            this.encryptionPolicy = encryptionPolicy ?? throw new ArgumentNullException(nameof(encryptionPolicy));
        }

        /// <inheritdoc/>
        public override string Name => "Message body encryption plugin";

        /// <inheritdoc/>
        public override Task<Message> BeforeMessageSend(Message message) => message.EncryptAsync(this.encryptionPolicy, CancellationToken.None);

        /// <inheritdoc/>
        public override Task<Message> AfterMessageReceive(Message message) => message.DecryptAsync(this.encryptionPolicy, CancellationToken.None);
    }
}