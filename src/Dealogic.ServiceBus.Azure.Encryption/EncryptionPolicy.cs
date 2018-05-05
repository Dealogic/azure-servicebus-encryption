namespace Dealogic.ServiceBus.Azure.Encryption
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Dealogic.ServiceBus.Azure.Encryption.Tracing;
    using Microsoft.Azure.KeyVault.Core;
    using Microsoft.Azure.ServiceBus;
    using Newtonsoft.Json;

    /// <summary>
    /// Service Bus brokered message encryption policy
    /// </summary>
    public class EncryptionPolicy
    {
        /// <summary>
        /// The encryption data key for message transport header
        /// </summary>
        public const string EncryptionHeaderDataKey = "encryptiondata";

        private readonly EncryptionPolicyOptions encryptionPolicyOptions;
        private readonly SemaphoreSlim semaphoreSlim;
        private readonly JsonSerializerSettings defaultSettings;
        private IKey cachedEncryptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionPolicy"/> class. If the encryption
        /// key is set, encryption and decryption with the specified key will be available. If
        /// decryption key resolver is set, decryption will be available. Both parameters can be specified.
        /// </summary>
        /// <param name="encryptionKey">The encryption key.</param>
        /// <param name="decryptionKeyResolver">The decryption key resolver.</param>
        /// <exception cref="ArgumentException">Both parameters are null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Justification = "Culture invariant message")]
        public EncryptionPolicy(IKey encryptionKey, IKeyResolver decryptionKeyResolver)
        {
            if (encryptionKey == null & decryptionKeyResolver == null)
            {
                throw new ArgumentException($"Neither {nameof(encryptionKey)} nor {nameof(decryptionKeyResolver)} has been set.");
            }

            this.cachedEncryptionKey = encryptionKey;
            this.encryptionPolicyOptions = new EncryptionPolicyOptions
            {
                KeyResolver = decryptionKeyResolver
            };

            if (encryptionKey != null)
            {
                this.encryptionPolicyOptions.EncryptionKey = (token) => Task.FromResult(encryptionKey);
            }

            this.encryptionPolicyOptions.Validate();
            this.semaphoreSlim = new SemaphoreSlim(1);
            this.defaultSettings = new JsonSerializerSettings();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionPolicy"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public EncryptionPolicy(Action<EncryptionPolicyOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.encryptionPolicyOptions = new EncryptionPolicyOptions();
            options(this.encryptionPolicyOptions);

            this.encryptionPolicyOptions.Validate();
            this.semaphoreSlim = new SemaphoreSlim(1);
            this.defaultSettings = new JsonSerializerSettings();
        }

        /// <summary>
        /// Gets the decryption key resolver.
        /// </summary>
        /// <value>The decryption key resolver.</value>
        public IKeyResolver DecryptionKeyResolver => this.encryptionPolicyOptions.KeyResolver;

        /// <summary>
        /// Encrypts the message asynchronous.
        /// </summary>
        /// <param name="brokeredMessage">The brokered message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The brokered message with encrypted data and metadata.</returns>
        /// <exception cref="ArgumentNullException">brokeredMessage is null</exception>
        /// <exception cref="InvalidOperationException">No encrpytion key has been initialized.</exception>
        public virtual async Task<Message> EncryptMessageAsync(Message brokeredMessage, CancellationToken cancellationToken)
        {
            if (brokeredMessage == null)
            {
                throw new ArgumentNullException(nameof(brokeredMessage));
            }

            if (this.encryptionPolicyOptions.EncryptionKey == null)
            {
                throw new InvalidOperationException("No encrpytion key has been initialized.");
            }

            ServiceBusEncryptionEventSource.Log.EncryptingMessageStarted(brokeredMessage.MessageId);
            brokeredMessage.Body = await this.EncryptBodyAsync(brokeredMessage.Body, brokeredMessage.UserProperties, cancellationToken).ConfigureAwait(false);
            ServiceBusEncryptionEventSource.Log.EncryptingMessageFinished(brokeredMessage.MessageId);
            return brokeredMessage;
        }

        /// <summary>
        /// Decrypts the message asynchronous.
        /// </summary>
        /// <param name="brokeredMessage">The brokered message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The brokered message with decrypted data.</returns>
        /// <exception cref="ArgumentNullException">brokeredMessage is null</exception>
        public virtual async Task<Message> DecryptMessageAsync(Message brokeredMessage, CancellationToken cancellationToken)
        {
            if (brokeredMessage == null)
            {
                throw new ArgumentNullException(nameof(brokeredMessage));
            }

            ServiceBusEncryptionEventSource.Log.DecryptingMessageStarted(brokeredMessage.MessageId);
            brokeredMessage.Body = await this.DecryptBodyAsync(brokeredMessage.Body, brokeredMessage.UserProperties, cancellationToken).ConfigureAwait(false);
            ServiceBusEncryptionEventSource.Log.DecryptingMessageFinished(brokeredMessage.MessageId);

            return brokeredMessage;
        }

        /// <summary>
        /// Decrypts the body asynchronous.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decrypted body.</returns>
        /// <exception cref="ArgumentNullException">body or properties is null</exception>
        /// <exception cref="InvalidOperationException">
        /// IV not found. or Encryption key not found. or Invalid Encryption Agent. This version of
        /// the client library does not understand the Encryption Agent set on the message.
        /// </exception>
        /// <exception cref="Exception">
        /// No decryption key could be resolved or Could not resolve a decryption key or Decryption
        /// logic threw error. Please check the inner exception for more details.
        /// </exception>
        /// <exception cref="CryptographicException">
        /// Key mismatch. The key id stored on the service does not match the specified key. or
        /// Invalid Encryption Algorithm found on the resource. This version of the client library
        /// does not support the specified encryption algorithm.
        /// </exception>
        /// <exception cref="SerializationException">
        /// Error while de-serializing the encryption metadata string from the wire.
        /// </exception>
        internal async Task<byte[]> DecryptBodyAsync(byte[] body, IDictionary<string, object> properties, CancellationToken cancellationToken)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            try
            {
                if (properties.TryGetValue(EncryptionHeaderDataKey, out object encryptionData) && encryptionData != null)
                {
                    var data = JsonConvert.DeserializeObject<EncryptionData>(encryptionData as string, this.defaultSettings);
                    if (data.ContentEncryptionIV == null)
                    {
                        throw new InvalidOperationException("IV not found.");
                    }

                    if (data.WrappedContentKey.EncryptedKey == null)
                    {
                        throw new InvalidOperationException("Encryption key not found.");
                    }

                    if (data.EncryptionAgent.Protocol != "1.0")
                    {
                        throw new InvalidOperationException("Invalid Encryption Agent. This version of the client library does not understand the Encryption Agent set on the message.");
                    }

                    byte[] dataEncryptionkey = null;
                    if (this.DecryptionKeyResolver != null)
                    {
                        ServiceBusEncryptionEventSource.Log.TryUsingKeyResolver();
                        var decryptionKey = await this.DecryptionKeyResolver.ResolveKeyAsync(data.WrappedContentKey.KeyId, cancellationToken).ConfigureAwait(false);
                        if (decryptionKey == null)
                        {
                            throw new Exception("No decryption key could be resolved");
                        }

                        ServiceBusEncryptionEventSource.Log.DecryptionKeyFound(decryptionKey.Kid);
                        dataEncryptionkey = await decryptionKey.UnwrapKeyAsync(data.WrappedContentKey.EncryptedKey, data.WrappedContentKey.Algorithm, cancellationToken).ConfigureAwait(false);
                    }
                    else if (this.encryptionPolicyOptions.EncryptionKey != null)
                    {
                        var encryptionKey = await this.GetEncryptionKeyAsync(cancellationToken).ConfigureAwait(false);
                        if (string.Equals(encryptionKey.Kid, data.WrappedContentKey.KeyId, StringComparison.Ordinal))
                        {
                            ServiceBusEncryptionEventSource.Log.TryUsingOriginalEncryptionKey(encryptionKey.Kid);
                            dataEncryptionkey = await encryptionKey.UnwrapKeyAsync(data.WrappedContentKey.EncryptedKey, data.WrappedContentKey.Algorithm, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            throw new CryptographicException("Key mismatch. The key id stored on the service does not match the specified key.");
                        }
                    }

                    if (dataEncryptionkey == null)
                    {
                        throw new Exception("Could not resolve a decryption key");
                    }

                    ServiceBusEncryptionEventSource.Log.DetectedEncryptionAlgorithm(data.EncryptionAgent.EncryptionAlgorithm.ToString());
                    if (data.EncryptionAgent.EncryptionAlgorithm == EncryptionAlgorithm.AES_CBC_256)
                    {
                        using (var provider = new AesCryptoServiceProvider())
                        {
                            provider.IV = data.ContentEncryptionIV;
                            provider.Key = dataEncryptionkey;

                            using (var transform = provider.CreateDecryptor())
                            {
                                return transform.TransformFinalBlock(body, 0, body.Length);
                            }
                        }
                    }
                    else
                    {
                        throw new CryptographicException("Invalid Encryption Algorithm found on the resource. This version of the client library does not support the specified encryption algorithm.");
                    }
                }
                else
                {
                    ServiceBusEncryptionEventSource.Log.NoEncryptionDataSkipBodyDecryption();
                }
            }
            catch (JsonException ex)
            {
                throw new SerializationException("Error while de-serializing the encryption metadata string from the wire.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Decryption logic threw error. Please check the inner exception for more details.", ex);
            }

            return body;
        }

        /// <summary>
        /// Encrypts the entity asynchronous.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Encrypted message body</returns>
        /// <exception cref="System.ArgumentNullException">body or properties is null</exception>
        internal async Task<byte[]> EncryptBodyAsync(byte[] body, IDictionary<string, object> properties, CancellationToken cancellationToken)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            using (var transform = await this.CreateAndSetEncryptionContext(properties, cancellationToken).ConfigureAwait(false))
            {
                return transform.TransformFinalBlock(body, 0, body.Length);
            }
        }

        /// <summary>
        /// Gets the encryption key asynchronous. This method will be used to get the key when
        /// encrypting a message. The key will be cached after the first use.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The encryption key</returns>
        /// <exception cref="InvalidOperationException">Encryption key could not be resolved</exception>
        protected virtual async Task<IKey> GetEncryptionKeyAsync(CancellationToken cancellationToken)
        {
            if (this.cachedEncryptionKey == null)
            {
                try
                {
                    await this.semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
                    if (this.cachedEncryptionKey == null)
                    {
                        this.cachedEncryptionKey = await this.encryptionPolicyOptions.EncryptionKey(cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    this.semaphoreSlim.Release();
                }
            }

            return this.cachedEncryptionKey ?? throw new InvalidOperationException("Encryption key could not be resolved");
        }

        private async Task<ICryptoTransform> CreateAndSetEncryptionContext(IDictionary<string, object> metadata, CancellationToken cancellationToken)
        {
            using (var provider = new AesCryptoServiceProvider())
            {
                var encryptionKey = await this.GetEncryptionKeyAsync(cancellationToken).ConfigureAwait(false);
                ServiceBusEncryptionEventSource.Log.UsingKeyForEncryption(encryptionKey?.Kid ?? "Not set!");

                var result = await encryptionKey.WrapKeyAsync(provider.Key, null, cancellationToken).ConfigureAwait(false);

                var data = new EncryptionData
                {
                    EncryptionAgent = new EncryptionAgent("1.0", EncryptionAlgorithm.AES_CBC_256),
                    WrappedContentKey = new WrappedKey(encryptionKey.Kid, result.Item1, result.Item2),
                    ContentEncryptionIV = provider.IV
                };

                metadata[EncryptionHeaderDataKey] = JsonConvert.SerializeObject(data, this.defaultSettings);
                return provider.CreateEncryptor();
            }
        }
    }
}