# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET library (Evoq.Ethereum.EAS) that provides a simplified interface for interacting with the Ethereum Attestation Service (EAS). It's built on top of the Evoq.Blockchain and Evoq.Ethereum libraries and targets .NET Standard 2.1.

## Plan & Review

### Before starting work
- Always in plan mode to make a plan
- After making the plan, make sure you write the plan to ./claude/tasks/TASK_NAME.md.
- The plan should be a detailed implementation plan and the reasoning behind them, as well as tasks broken down.
- If the task requires external knowledge or certain packages, research to get latest knowledge using the Task tool
- Don't over plan it, always think MVP.
- Once you write the plan, firstly ask me to review it. Do not continue until I approve the plan.

### While implementing
- You should update the plan as you work.
- After you complete tasks in the plan, you should update and append detailed descriptions of the changes you made, so following tasks can be easily hand over to other engineers.

## Documentation

The repository contains several documentation files:

- **[README.md](./README.md)** - Main project documentation with installation, usage examples, and comprehensive API guide
- **[README_EAS.md](./README_EAS.md)** - Ethereum Attestation Service background, concepts, and technical details
- **[EAS_Signatures.md](./EAS_Signatures.md)** - Complete reference of EAS contract function signatures organized by category
- **[CLAUDE.md](./CLAUDE.md)** - This file; development guidance for Claude Code

## Architecture

The library follows a layered architecture:

### Core Components

- **EAS Class** (`src/Evoq.Ethereum.EAS/Ethereum.EAS/EAS.cs`): Main client for EAS contract interactions
  - Implements interfaces: IAttest, IRevoke, ITimestamp, IRevokeOffchain, IGetAttestation
  - Handles attestation creation, revocation, timestamping, and querying operations
  - Manages transaction execution and event log parsing

- **SchemaRegistry Class** (`src/Evoq.Ethereum.EAS/Ethereum.EAS/SchemaRegistry.cs`): Client for schema registry operations
  - Implements interfaces: IGetSchema, IGetVersion, IRegisterSchema
  - Handles schema registration and retrieval
  - Manages schema UID formatting and validation

- **Contracts Class** (`src/Evoq.Ethereum.EAS/Ethereum.EAS/Contracts.cs`): Static configuration for contract addresses
  - Contains hardcoded addresses for EAS and Schema Registry contracts across multiple networks
  - Supports mainnets, testnets, and development networks (Hardhat)
  - Provides ABI loading functionality

### Key Concepts

- **InteractionContext**: Core configuration object that manages network connection, transaction signing, and gas fee configuration
- **AttestationRequest/RevocationRequest**: Data structures for EAS operations
- **SchemaUID**: Utility for formatting and managing schema unique identifiers
- **TransactionResult<T>**: Wrapper for transaction receipts with typed results

## Development Commands

### Build and Test
```bash
# Build the project
dotnet build -c Release

# Run tests (requires Hardhat node with EAS contracts deployed)
dotnet test -c Release

# Run tests with detailed logging
dotnet test --logger "console;verbosity=detailed"

# Build and create NuGet package
./build.sh

# Publish to NuGet (requires NUGET_API_KEY environment variable)
./publish.sh
```

### Test Environment Setup

Tests require a local Hardhat node with EAS contracts deployed:

1. The EthereumTestContext expects these environment variables:
   - `Blockchain__Ethereum__Addresses__Hardhat1PrivateKey`
   - `Blockchain__Ethereum__Addresses__Hardhat1Address`

2. Contract addresses for Hardhat are hardcoded in Contracts.cs:
   - SchemaRegistry: `0x5FbDB2315678afecb367f032d93F642f64180aa3`
   - EAS: `0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512`

These addresses are deterministic when using the default Hardhat account (account #0) with fresh nonces.

## Development Guidelines

### General

- Ensure functionality is not already present before implementing; developers sometimes don't realise a thing is already possible
- Explore any URLs provided by the dev in the prompt
- Remember to try getting the llms.txt from provided domains, e.g. sometool.com/llms.txt
- Do not use mocking frameworks, prefer writing realistic fake implementations
- Comments must not simply say what is clearly readable in the code but be rare and only used to explain complex logic or workarounds
- You may use comments to break up sections or stages of a procedure
- Consider the order of dependent classes and members and build 'upwards'

### C# .NET Coding Guidelines
- Keep in mind the thoughts of Cwalina and Abrams design guidelines
- Use whitespace and blank lines to visually group related code
- `return` and `await` statements should appear on their own lines
- Append `Async` suffix to public async methods
- Consider making dedicated custom exception classes
- Prefer many smaller, focused classes
- Consider the behaviour of classes, functions and method and plan tests
- Tests should be quite minimal for private or internal classes but much deeper for public interfaces esp. in SDKs and libraries
- Follow C# coding conventions
- Add XML documentation for public APIs  
- Write unit tests for new functionality using MSTest
- Use meaningful test names e.g. `AttestedMerkleExchangeReader__when__valid_jws__then__returns_valid_result`
- Avoid general names like Helper which are indicative of an unfocused class
- Order class members: fields, ctors, props, methods, functions; then by public, private; then by members, statics.
- Use `this` to make clear when referring to own members

## Documentation Guidelines

- Use Markdown for documentation
- Follow DRY principles, and one responsilbity per page (.md file)
- Use page linking as appropriate
- Consider dedicating an .md file to a subject
- Use tests to inform you about API usage and examples
- Be concise and apply a logical order
- Look over entire files for repetition

## Key Patterns

### Transaction Pattern
Most operations follow this pattern:
1. Create contract instance with appropriate ABI
2. Estimate transaction fees
3. Configure gas options and transaction options
4. Execute transaction using TransactionRunnerNative
5. Parse event logs from receipt to extract results

### Network Support
The library supports multiple networks through the ChainIds and ChainNames constants. Contract addresses are network-specific and managed in the Contracts class.

### Error Handling
- Custom EASException for library-specific errors
- Transaction hash is preserved in exceptions for debugging
- Comprehensive validation of transaction receipts and event logs

## Testing Strategy

- Uses MSTest framework targeting .NET 7.0
- Integration tests require actual blockchain interaction
- Test helper class (EthereumTestContext) simplifies test setup
- Many tests depend on contract deployment state

## Dependencies

- **Evoq.Blockchain** [1.0.9,2.0.0): Core blockchain functionality
- **Evoq.Ethereum** [3.1.0,3.3.0): Ethereum-specific implementations
- **System.Text.Json** 8.0.5: JSON serialization

## Important Notes

- Contract addresses change when Hardhat node is reset or deployment order changes
- The library uses embedded ABI files for contract interaction
- All monetary values use EtherAmount/Wei types from the Evoq libraries
- The library supports both on-chain and off-chain operations (timestamping, revocation)