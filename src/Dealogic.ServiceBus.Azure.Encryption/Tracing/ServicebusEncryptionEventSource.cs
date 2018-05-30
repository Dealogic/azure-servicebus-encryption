namespace Dealogic.ServiceBus.Azure.Encryption.Tracing
{
    using System;
    using System.Diagnostics.Tracing;

    /// <summary>
    /// Service bus event source
    /// </summary>
    /// <seealso cref="System.Diagnostics.Tracing.EventSource"/>
    [EventSource(Name = Configuration.EventSourceName)]
    internal sealed partial class ServiceBusEncryptionEventSource : EventSource
    {
        private static readonly Lazy<ServiceBusEncryptionEventSource> Instance = new Lazy<ServiceBusEncryptionEventSource>(() => new ServiceBusEncryptionEventSource());

        /// <summary>
        /// Prevents a default instance of the <see cref="ServiceBusEncryptionEventSource"/> class
        /// from being created.
        /// </summary>
        private ServiceBusEncryptionEventSource()
        {
        }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <value>The log.</value>
        public static ServiceBusEncryptionEventSource Log => Instance.Value;

        /// <summary>
        /// Encryptings the message started.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        [Event(1, Level = EventLevel.Informational, Message = "Encrypting message.")]
        public void EncryptingMessageStarted(string messageId)
        {
            if (this.IsEnabled(EventLevel.Informational, EventKeywords.All))
            {
                this.WriteEvent(1, messageId);
            }
        }

        /// <summary>
        /// Usings the key for encryption.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        [Event(2, Level = EventLevel.Verbose, Message = "Using key {0}")]
        public void UsingKeyForEncryption(string keyId)
        {
            if (this.IsEnabled(EventLevel.Verbose, EventKeywords.All))
            {
                this.WriteEvent(2, keyId);
            }
        }

        /// <summary>
        /// Encryptings the message finished.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        [Event(3, Level = EventLevel.Informational, Message = "Encrypting message finished.")]
        public void EncryptingMessageFinished(string messageId)
        {
            if (this.IsEnabled(EventLevel.Informational, EventKeywords.All))
            {
                this.WriteEvent(3, messageId);
            }
        }

        /// <summary>
        /// Decryptings the message started.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        [Event(4, Level = EventLevel.Informational, Message = "Decrypting message.")]
        public void DecryptingMessageStarted(string messageId)
        {
            if (this.IsEnabled(EventLevel.Informational, EventKeywords.All))
            {
                this.WriteEvent(4, messageId);
            }
        }

        /// <summary>
        /// Tries the using key resolver.
        /// </summary>
        [Event(6, Level = EventLevel.Verbose, Message = "Try using key resolver.")]
        public void TryUsingKeyResolver()
        {
            if (this.IsEnabled(EventLevel.Verbose, EventKeywords.All))
            {
                this.WriteEvent(6);
            }
        }

        /// <summary>
        /// Decryptions the key found.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        [Event(7, Level = EventLevel.Verbose, Message = "Key found {0}")]
        public void DecryptionKeyFound(string keyId)
        {
            if (this.IsEnabled(EventLevel.Verbose, EventKeywords.All))
            {
                this.WriteEvent(7, keyId);
            }
        }

        /// <summary>
        /// Tries the using original encryption key.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        [Event(8, Level = EventLevel.Verbose, Message = "Using original encryption key {0}")]
        public void TryUsingOriginalEncryptionKey(string keyId)
        {
            if (this.IsEnabled(EventLevel.Verbose, EventKeywords.All))
            {
                this.WriteEvent(8, keyId);
            }
        }

        /// <summary>
        /// Detecteds the encryption algorithm.
        /// </summary>
        /// <param name="algorithm">The algorithm.</param>
        [Event(9, Level = EventLevel.Verbose, Message = "Detected encryption algorithm {0}")]
        public void DetectedEncryptionAlgorithm(string algorithm)
        {
            if (this.IsEnabled(EventLevel.Verbose, EventKeywords.All))
            {
                this.WriteEvent(9, algorithm);
            }
        }

        /// <summary>
        /// Decryptings the message finished.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        [Event(10, Level = EventLevel.Informational, Message = "Decrypting message finished.")]
        public void DecryptingMessageFinished(string messageId)
        {
            if (this.IsEnabled(EventLevel.Informational, EventKeywords.All))
            {
                this.WriteEvent(10, messageId);
            }
        }

        /// <summary>
        /// Skips the body decryption.
        /// </summary>
        [Event(11, Level = EventLevel.Warning, Message = "No encrypted message media type found. Skip body decryption.")]
        public void NoMediaTypeSkipBodyDecryption()
        {
            if (this.IsEnabled(EventLevel.Warning, EventKeywords.All))
            {
                this.WriteEvent(11);
            }
        }

        /// <summary>
        /// Noes the encryption data skip body decryption.
        /// </summary>
        [Event(12, Level = EventLevel.Warning, Message = "No encryption data found. Skip body decryption.")]
        public void NoEncryptionDataSkipBodyDecryption()
        {
            if (this.IsEnabled(EventLevel.Warning, EventKeywords.All))
            {
                this.WriteEvent(12);
            }
        }
    }
}