namespace Dealogic.ServiceBus.Azure.Encryption.UnitTest
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Core;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSubstitute;

    [TestClass]
    public class DecryptionTests
    {
        [TestMethod]
        public async Task DecryptMessageTest_WithKeyResolver()
        {
            var testMessage = "Fake Message";
            var testMessageBody = Encoding.Default.GetBytes(testMessage);
            var message = new Message(testMessageBody);
            using (var rsaProvider = new RSACryptoServiceProvider(4_096))
            {
                rsaProvider.PersistKeyInCsp = false;
                using (var rsaKey = new RsaKey("Test Key", rsaProvider))
                {
                    var mockKeyResolver = Substitute.For<IKeyResolver>();
                    mockKeyResolver.ResolveKeyAsync("Test Key", CancellationToken.None).Returns(rsaKey);

                    var encryptionPolicy = new EncryptionPolicy(o =>
                    {
                        o.EncryptionKey = (token) => mockKeyResolver.ResolveKeyAsync("Test Key", token);
                    });

                    var encryptedMessage = await message.EncryptAsync(encryptionPolicy, CancellationToken.None).ConfigureAwait(false);

                    var decryptionPolicy = new EncryptionPolicy(o =>
                    {
                        o.KeyResolver = mockKeyResolver;
                    });

                    var decryptedMessage = await encryptedMessage.DecryptAsync(decryptionPolicy, CancellationToken.None).ConfigureAwait(false);

                    Assert.AreEqual(testMessage, Encoding.Default.GetString(decryptedMessage.Body));
                }
            }
        }

        [TestMethod]
        public async Task DecryptMessageTest_WithOriginalKey()
        {
            var testMessage = "Fake Message";
            var testMessageBody = Encoding.Default.GetBytes(testMessage);
            var message = new Message(testMessageBody);
            using (var rsaProvider = new RSACryptoServiceProvider(4_096))
            {
                rsaProvider.PersistKeyInCsp = false;
                using (var rsaKey = new RsaKey("Test Key", rsaProvider))
                {
                    var encryptionPolicy = new EncryptionPolicy(rsaKey, null);
                    var encryptedMessage = await message.EncryptAsync(encryptionPolicy, CancellationToken.None).ConfigureAwait(false);

                    var decryptionPolicy = new EncryptionPolicy(rsaKey, null);
                    var decryptedMessage = await encryptedMessage.DecryptAsync(decryptionPolicy, CancellationToken.None).ConfigureAwait(false);

                    Assert.AreEqual(testMessage, Encoding.Default.GetString(decryptedMessage.Body));
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task DecryptMessageTest_KeyMismatch()
        {
            var testMessage = "Fake Message";
            var testMessageBody = Encoding.Default.GetBytes(testMessage);
            var message = new Message(testMessageBody);
            using (var rsaProvider = new RSACryptoServiceProvider(4_096))
            {
                rsaProvider.PersistKeyInCsp = false;
                using (var rsaKey = new RsaKey("Test Key", rsaProvider))
                using (var rsaKey2 = new RsaKey("Test Key 2", rsaProvider))
                {
                    var encryptionPolicy = new EncryptionPolicy(rsaKey, null);
                    var encryptedMessage = await message.EncryptAsync(encryptionPolicy, CancellationToken.None).ConfigureAwait(false);

                    var mockKeyResolver = Substitute.For<IKeyResolver>();
                    mockKeyResolver.ResolveKeyAsync("Test Key", CancellationToken.None).Returns(rsaKey2);

                    var decryptionPolicy = new EncryptionPolicy(o =>
                    {
                        o.EncryptionKey = (token) => mockKeyResolver.ResolveKeyAsync("Test Key", token);
                    });

                    var decryptedMessage = await encryptedMessage.DecryptAsync(decryptionPolicy, CancellationToken.None).ConfigureAwait(false);
                }
            }
        }
    }
}