namespace Dealogic.ServiceBus.Azure.Encryption.UnitTest
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Core;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EncryptionTests
    {
        [TestMethod]
        public async Task EncryptMessageTest()
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

                    Assert.IsTrue(encryptedMessage.UserProperties.ContainsKey(EncryptionPolicy.EncryptionHeaderDataKey));
                    Assert.IsNotNull(encryptedMessage.UserProperties[EncryptionPolicy.EncryptionHeaderDataKey]);
                    Assert.AreNotEqual(testMessage, Encoding.Default.GetString(encryptedMessage.Body));
                }
            }
        }

        [TestMethod]
        public async Task MultithreadEncryptionTest()
        {
            var testMessageBody = "Test message";
            var testMessages = GetTestMessages(testMessageBody);

            using (var rsaProvider = new RSACryptoServiceProvider(4_096))
            {
                rsaProvider.PersistKeyInCsp = false;

                using (var rsaKey = new RsaKey("Test Key", rsaProvider))
                {
                    var encryptionPolicy = new EncryptionPolicy(options =>
                    {
                        options.EncryptionKey = (token) => Task.FromResult<IKey>(rsaKey);
                    });

                    var encryptionTasks = testMessages.Select(message => message.EncryptAsync(encryptionPolicy, CancellationToken.None));
                    var encryptedMessages = await Task.WhenAll(encryptionTasks).ConfigureAwait(false);

                    foreach (var encryptedMessage in encryptedMessages)
                    {
                        Assert.IsTrue(encryptedMessage.UserProperties.ContainsKey(EncryptionPolicy.EncryptionHeaderDataKey));
                        Assert.IsNotNull(encryptedMessage.UserProperties[EncryptionPolicy.EncryptionHeaderDataKey]);
                        Assert.AreNotEqual(testMessageBody, Encoding.Default.GetString(encryptedMessage.Body));
                    }
                }
            }

            IEnumerable<Message> GetTestMessages(string testMessage)
            {
                for (int i = 0; i < 10; i++)
                {
                    var body = Encoding.Default.GetBytes(testMessage);
                    var message = new Message(body);
                    yield return message;
                }
            }
        }
    }
}