# Dealogic Azure Service Bus Encryption

Adds Encryption policy and extensions methods to Brokered message for easy message body encryption.
For encrypting messages you have to set the Encryption key of the encryption policy. For decrypting messages
the DecryptionKeyResolver parameter has to be set or the same encryption key used for encryption.
The easyiest way to retrieve these object is through the `KeyVaultClient` class in the [Azure Key Vault Extensions package](https://www.nuget.org/packages/Microsoft.Azure.KeyVault.Extensions/)

## Build status

![build status](https://dealogic.visualstudio.com/_apis/public/build/definitions/4cd19643-db3a-4dcc-b481-76a7800dd64d/12979/badge)

## Content

* [Encrypt message](#encrypt-message)
* [Decrypt message](#decrypt-message)
* [Lazy initialization](#lazy-initialization)
* [Tracing](#tracing)
* [Implementation notes](#implementation-notes)

### <a id="encrypt-message" /> Encrypting messages

By registering the plugin

```csharp
var encryptionPolicy = new EncryptionPolicy(key, null);
var queueClient = new QueueClient("YOUR CONNECTION STRING", "YOUR QUEUE");
queueClient.RegisterPlugin(new MessageBodyEncryptionPlugin(encryptionPolicy));

var body = new byte[0];
var message = new Message(body);
await queueClient.SendAsync(message).ConfigureAwait(false);
```

By using extensions

```csharp
var encryptionPolicy = new EncryptionPolicy(key, null);
var encryptedMessage = await message.EncryptAsync(encryptionPolicy, cancellationToken).ConfigureAwait(false);
```

### <a id="decrypt-message"/>  Decrypting messages

By registering the plugin

```csharp
var encryptionPolicy = new EncryptionPolicy(null, keyResolver);
var queueClient = new QueueClient("YOUR CONNECTION STRING", "YOUR QUEUE");
queueClient.RegisterPlugin(new MessageBodyEncryptionPlugin(encryptionPolicy));

client.RegisterMessageHandler(SomeHandlerDelegate, MessageHandlerOptions);
```

By using extensions

```csharp
var decryptionPolicy = new EncryptionPolicy(null, keyResolver);
var decryptedMessage = await encryptedMessage.DecryptAsync(decryptionPolicy, cancellationToken).ConfigureAwait(false);
```

### <a id="lazy-initialization"/> Using lazy initialization

Encryption policy can be costructed with lazy encryption key initialization. The encryption key will be resolved
when it's first used. The default implementation caches the key. For example:

```csharp
var encryptionPolicy = new EncryptionPolicy(o =>
{
   o.EncryptionKey = (token) => keyResolver.ResolveKeyAsync("Key ID", token);
   o.ReyResolver = keyResolver
});
```

## <a id="tracing" /> Tracing
The component supports Event Source tracing out of the box. The Event Source name can be retreived from
`Dealogic.ServiceBus.Azure.Encryption.Tracing.EventSourceName`.

## <a id="implementation-notes" /> Implementation notes

- when encrypting a message two new custom values will be added to the Message's property bag:
  - **encryptiondata**: contains the nessesary metadata for decryption
- if the encryptiondata is not provided, the message wont be decrypted
- when encrypting the message, the original body will replaced with the encrypted body content
- when decrypting the message, the original body will replaced with the decrypted body content
- When encrypting messages `Wrap` permission is needed on the Key Vault Keys
- When decrypting messages `Unwrap` permission is needed on the Key Vault Keys
- When using `KeyResolver` for decrpytion `Get` permission is needed for the user on the Key Vault Keys
- if possible use `CachingKeyResolver` to avoid multiple roundtrips to the server
- try to cache the **access token** for the KeyVault access to avoid multiple roundtrips to the server

## Contribution

The packages uses VSTS pipeline for build and release. The versioning is done by GitVersion.
From all feature (features) branches a new pre-release pacakges will be automatically released.
**After releasing a stable version, the version Tag has to be added to the code with the released version number.**
