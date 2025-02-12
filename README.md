# Evoq.Ethereum.EAS

A lightweight .NET library providing an easy-to-use implementation of the Ethereum Attestation Service (EAS). This package builds upon Evoq.Blockchain and Evoq.Ethereum to provide a simplified interface for working with EAS.

## Installation

```
dotnet add package Evoq.Ethereum.EAS
```

## Features

- Simple interface for creating and managing attestations
- Type-safe EAS primitives
- Easy integration with existing Ethereum applications
- Built on top of Evoq.Blockchain and Evoq.Ethereum

## Target Frameworks

This package targets .NET Standard 2.1 for maximum compatibility across:
- .NET 6.0+
- .NET Framework 4.6.1+
- .NET Core 2.0+
- Xamarin
- Unity

## Dependencies

- Evoq.Blockchain (1.0.0)
- Evoq.Ethereum (1.0.0)

## Usage

```csharp
// Initialize EAS with your endpoint and sender
var endpoint = SepoliaEndpointURL.Google(projectId, apiKey, loggerFactory);
var sender = new Sender(privateKey, nonceStore);

// Create schema registry instance
var registry = new SchemaRegistryNethereum(endpoint, sender, loggerFactory);

// Define and register a schema
var schemaString = "string name, uint8 age";
var schemaUID = SchemaRegistryNethereum.GetSchemaUID(schemaString, revocable: true, EthereumAddress.Zero);

// Check if schema exists, register if it doesn't
var schema = await registry.GetSchema(schemaUID);
if (schema == null)
{
    var registeredUID = await registry.Register(schemaString, resolver: null, revocable: true);
    Console.WriteLine($"Registered new schema with UID: {registeredUID}");
}

// Create an attestation
var eas = new EASNethereum(endpoint, sender, loggerFactory);

// Prepare attestation data
var data = new SchemaEncoder(schemaString).AbiEncode(
    new object[] { "Alice", 25 }
);

var requestData = new AttestationRequestData(
    Recipient: recipientAddress,
    ExpirationUnixTimestamp: UInt64.MaxValue,
    Revocable: true,
    RefUID: Hex.Zero,
    Data: new Hex(data),
    Value: 0
);

var attestationRequest = new AttestationRequest(
    Schema: schemaUID.ToHexStruct(),
    Data: requestData
);

// Submit the attestation
var attestationUID = await eas.AttestAsync(attestationRequest);
Console.WriteLine($"Created attestation with UID: {attestationUID}");
```

## Building

```bash
dotnet build
dotnet test
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

Luke Puplett

## Project Links

- [GitHub Repository](https://github.com/lukepuplett/evoq-ethereum-eas)
- [NuGet Package](https://www.nuget.org/packages/Evoq.Ethereum.EAS)