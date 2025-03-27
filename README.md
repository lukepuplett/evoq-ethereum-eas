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
- Support for both regular and delegated attestations
- Comprehensive attestation querying and validation
- Built-in timestamping support
- EIP-712 structured data signing support
- Multi-attestation and revocation capabilities
- Off-chain revocation support

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

// Configure for different networks
var mainnetContext = new InteractionContext(
    rpcUrl: "https://mainnet.infura.io/v3/YOUR-PROJECT-ID",
    chainId: ChainIds.Mainnet,
    privateKey: "your-private-key" // Store securely, never commit to source control
);
```

### Understanding InteractionContext

The `InteractionContext` is the core configuration object that manages your connection to the Ethereum network. It handles:
- Network connection (RPC endpoint)
- Chain identification
- Account management
- Transaction signing
- Logging

#### Creating an InteractionContext

There are several ways to create an `InteractionContext`:

1. **Using Test Context (for development)**
```csharp
// Creates a context connected to a local Hardhat node
InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);
```

2. **Manual Configuration**
```csharp
// Create with all options
var context = new InteractionContext(
    rpcUrl: "https://mainnet.infura.io/v3/YOUR-PROJECT-ID",
    chainId: ChainIds.Mainnet,
    privateKey: "your-private-key",
    logger: logger,
    maxRetries: 3,
    timeout: TimeSpan.FromSeconds(30)
);

// Create with minimal options (uses defaults for logger, retries, and timeout)
var simpleContext = new InteractionContext(
    rpcUrl: "https://mainnet.infura.io/v3/YOUR-PROJECT-ID",
    chainId: ChainIds.Mainnet,
    privateKey: "your-private-key"
);
```

3. **Using Environment Variables**
```csharp
// Create from environment variables
var context = InteractionContext.FromEnvironment(
    rpcUrlEnvVar: "ETH_RPC_URL",
    privateKeyEnvVar: "ETH_PRIVATE_KEY",
    chainIdEnvVar: "ETH_CHAIN_ID"
);
```

#### Available Chain IDs

The library includes predefined chain IDs for common networks:
```csharp
ChainIds.Mainnet    // Ethereum Mainnet
ChainIds.Goerli     // Goerli Testnet
ChainIds.Sepolia    // Sepolia Testnet
ChainIds.Hardhat    // Local Hardhat Network
```

#### Logging

The context supports logging through an `ILogger` instance:
```csharp
// Create with custom logger
var logger = LoggerFactory.Create(builder => 
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug)
).CreateLogger<InteractionContext>();

var context = new InteractionContext(
    rpcUrl: "your-rpc-url",
    chainId: ChainIds.Mainnet,
    privateKey: "your-private-key",
    logger: logger
);
```

#### Error Handling and Retries

The context includes built-in retry logic for network operations:
```csharp
var context = new InteractionContext(
    rpcUrl: "your-rpc-url",
    chainId: ChainIds.Mainnet,
    privateKey: "your-private-key",
    maxRetries: 3,                    // Number of retry attempts
    timeout: TimeSpan.FromSeconds(30) // Timeout per operation
);
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

#### Multi-Attestation
```csharp
var multiRequest = new MultiAttestationRequest[]
{
    new MultiAttestationRequest(schemaUID1, data1),
    new MultiAttestationRequest(schemaUID2, data2)
};

var results = await eas.MultiAttestAsync(context, multiRequest);
```

#### Delegated Attestation
```csharp
var delegatedRequest = new DelegatedAttestationRequest(
    schemaUID: schemaUID,
    data: data,
    signature: signature,
    delegator: delegatorAddress
);

var result = await eas.AttestByDelegationAsync(context, delegatedRequest);
```

#### Revocation
```csharp
// On-chain revocation
var revocationRequest = new RevocationRequest(attestationUID);
await eas.RevokeAsync(context, revocationRequest);

// Off-chain revocation
var offchainData = new Hex("your-data");
var timestamp = await eas.RevokeOffchainAsync(context, offchainData);
```

#### Timestamping
```csharp
// Single timestamp
var timestamp = await eas.TimestampAsync(context, data);

// Multi-timestamp
var timestamps = await eas.MultiTimestampAsync(context, new[] { data1, data2 });
```

#### EIP-712 Support
```csharp
// Get domain separator for EIP-712 signing
var domainSeparator = await eas.GetDomainSeparatorAsync(context);

// Get attestation type hash
var attestTypeHash = await eas.GetAttestTypeHashAsync(context);
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