# Evoq.Ethereum.EAS

A lightweight .NET library providing an easy-to-use implementation of the Ethereum Attestation Service (EAS). This package builds upon Evoq.Blockchain and Evoq.Ethereum to provide a simplified interface for working with EAS.

## Installation

```bash
# Using the .NET CLI
dotnet add package Evoq.Ethereum.EAS

# Using the Package Manager Console
Install-Package Evoq.Ethereum.EAS

# Or add directly to your .csproj file
<PackageReference Include="Evoq.Ethereum.EAS" Version="2.0.0" />
```

## Features

- Simple interface for creating and managing attestations
- Type-safe EAS primitives
- Easy integration with existing Ethereum applications
- Built on top of Evoq.Blockchain and Evoq.Ethereum
- Support for revocable attestations
- Comprehensive attestation querying and validation
- Built-in timestamping support
- Schema registry integration

## Target Frameworks

This package targets .NET Standard 2.1 for maximum compatibility across:
- .NET 6.0+
- .NET Framework 4.7.2+ (Note: .NET Framework 4.6.1 and earlier are not supported)
- .NET Core 2.1+ (Note: .NET Core 2.0 is not supported)
- Xamarin
- Unity

## Dependencies

- Evoq.Blockchain (1.0.8)
- Evoq.Ethereum (2.1.0)
- System.Text.Json (8.0.5)
- SimpleBase (4.0.2)

## Usage

### Basic Setup

```csharp
// Initialize EAS with the contract address
var eas = new EAS(easContractAddress);

// Create an interaction context (example using Hardhat testnet)
InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

// For production use, you'll need to set up your own context with proper configuration
var endpoint = new Endpoint(
    networkName: ChainNames.Mainnet,
    displayName: ChainNames.Mainnet,
    url: "https://mainnet.infura.io/v3/YOUR-PROJECT-ID",
    loggerFactory: loggerFactory
);

var chain = endpoint.CreateChain();
var getTransactionCount = () => chain.GetTransactionCountAsync(yourAddress, "latest");
var nonces = new InMemoryNonceStore(loggerFactory, getTransactionCount);

var account = new SenderAccount(privateKey, yourAddress);
var sender = new Sender(account, nonces);

var context = new InteractionContext(endpoint, sender, UseSuggestedGasOptions);
```

### Understanding InteractionContext

The `InteractionContext` is the core configuration object that manages your connection to the Ethereum network. It's composed of several key components:

1. **Endpoint**: Manages the network connection and chain configuration
2. **Sender**: Handles transaction signing and nonce management
3. **Gas Options**: Configures how gas fees are calculated

#### Creating an InteractionContext

There are two main approaches:

1. **Using Test Context (for development)**
```csharp
// Creates a context connected to a local Hardhat node
// This handles all the setup internally using environment variables
InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);
```

2. **Manual Configuration (for production)**
```csharp
// 1. Set up logging
var loggerFactory = LoggerFactory.Create(builder => 
    builder.AddConsole().SetMinimumLevel(LogLevel.Information)
);

// 2. Create the endpoint
var endpoint = new Endpoint(
    networkName: ChainNames.Mainnet,
    displayName: ChainNames.Mainnet,
    url: "your-rpc-url",
    loggerFactory: loggerFactory
);

// 3. Create the chain
var chain = endpoint.CreateChain();

// 4. Set up nonce management
var getTransactionCount = () => chain.GetTransactionCountAsync(yourAddress, "latest");
var nonces = new InMemoryNonceStore(loggerFactory, getTransactionCount);

// 5. Configure the sender account
var account = new SenderAccount(privateKey, yourAddress);
var sender = new Sender(account, nonces);

// 6. Create the context with gas options
var context = new InteractionContext(endpoint, sender, UseSuggestedGasOptions);

// Helper function for gas options
static GasOptions UseSuggestedGasOptions(ITransactionFeeEstimate estimate)
{
    return estimate.ToSuggestedGasOptions();
}
```

#### Available Chain Names

The library includes predefined chain names for common networks:
```csharp
ChainNames.Mainnet    // Ethereum Mainnet
ChainNames.Goerli     // Goerli Testnet
ChainNames.Sepolia    // Sepolia Testnet
ChainNames.Hardhat    // Local Hardhat Network
```

#### Environment Variables for Testing

When using the test context, the following environment variables are required:
```bash
Blockchain__Ethereum__Addresses__Hardhat1PrivateKey=your-private-key
Blockchain__Ethereum__Addresses__Hardhat1Address=your-address
```

### Creating Attestations

```csharp
try
{
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
    else
    {
        Console.WriteLine($"Failed to create attestation: {result.Error}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error creating attestation: {ex.Message}");
}
```

### Advanced Features

#### Schema Registry Integration
```csharp
// Initialize the schema registry
var schemaRegistry = new SchemaRegistry(schemaRegistryAddress);

// Register a new schema
var schema = "bool isAHuman";
var result = await schemaRegistry.Register(context, schema);

// Get an existing schema
var schemaRecord = await schemaRegistry.GetSchemaAsync(context, schema);

// Get schema version
var version = await schemaRegistry.GetVersionAsync(context);
```

#### Revocation
```csharp
// On-chain revocation
var revocationRequest = new RevocationRequest(attestationUID);
await eas.RevokeAsync(context, revocationRequest);
```

#### Timestamping
```csharp
// Single timestamp
var timestamp = await eas.TimestampAsync(context, data);

// Get timestamp for data
var timestamp = await eas.GetTimestampAsync(context, data);
```

#### Attestation Validation
```csharp
// Check if an attestation is valid
var isValid = await eas.IsAttestationValidAsync(context, attestationUID);

// Get attestation details
var attestation = await eas.GetAttestationAsync(context, attestationUID);

// Get schema registry address
var registryAddress = await eas.GetSchemaRegistryAsync(context);
```

## Security Considerations

1. **Private Key Management**
   - Never store private keys in source code or configuration files
   - Use secure key management solutions
   - Consider using hardware wallets for production environments

2. **Network Security**
   - Use HTTPS for RPC endpoints
   - Validate chain IDs before transactions
   - Consider using multiple RPC providers for redundancy

3. **API Keys**
   - Store API keys securely
   - Use environment variables or secure secret management
   - Rotate keys regularly

## Testing

### Prerequisites
1. Install Hardhat for local Ethereum testing
2. Clone the repository
3. Run `npm install` in the `contracts` directory
4. Start Hardhat node: `npx hardhat node`

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Evoq.Ethereum.EAS.Tests

# Run with logging
dotnet test --logger "console;verbosity=detailed"
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