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
- Support for both regular and delegated attestations
- Comprehensive attestation querying and validation
- Built-in timestamping support

## Target Frameworks

This package targets .NET Standard 2.1 for maximum compatibility across:
- .NET 6.0+
- .NET Framework 4.6.1+
- .NET Core 2.0+
- Xamarin
- Unity

## Dependencies

- Evoq.Blockchain (1.0.8)
- Evoq.Ethereum (2.1.0)
- System.Text.Json (8.0.5)
- SimpleBase (4.0.2)

## Usage

```csharp
// Initialize EAS with the contract address
var eas = new EAS(easContractAddress);

// Create an interaction context (example using Hardhat testnet)
InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

// Create a schema UID (example for a boolean schema)
var schemaUID = SchemaUID.FormatSchemaUID("bool isAHuman", EthereumAddress.Zero, true);

// Prepare attestation data
var data = new AttestationRequestData(
    Recipient: recipientAddress,
    ExpirationTime: DateTimeOffset.UtcNow.AddDays(1),
    Revocable: true,
    RefUID: Hex.Empty,
    Data: Hex.Empty,
    Value: EtherAmount.Zero
);

// Create the attestation request
var request = new AttestationRequest(schemaUID, data);

// Submit the attestation
var result = await eas.AttestAsync(context, request);

if (result.Success)
{
    var attestationUID = result.Result;
    Console.WriteLine($"Created attestation with UID: {attestationUID}");

    // Retrieve the attestation
    var attestation = await eas.GetAttestationAsync(context, attestationUID);
    Console.WriteLine($"Retrieved attestation: {attestation}");
}

// Query attestation validity
var isValid = await eas.IsAttestationValidAsync(context, attestationUID);

// Get attestation timestamp
var timestamp = await eas.GetTimestampAsync(context, attestationUID);
```

## Key Features

### Attestations
- Create single and multi-attestations
- Support for delegated attestations
- Revocable attestations
- Reference UIDs for linked attestations

### Queries
- Retrieve attestation details
- Validate attestation status
- Get attestation timestamps
- Query schema registry

### Timestamping
- On-chain timestamping support
- Multi-timestamping capabilities
- Off-chain revocation support

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