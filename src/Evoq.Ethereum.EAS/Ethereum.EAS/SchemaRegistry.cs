using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Evoq.Blockchain;
using Evoq.Ethereum.ABI;
using Evoq.Ethereum.Chains;
using Evoq.Ethereum.Contracts;
using Evoq.Ethereum.JsonRPC;
using Evoq.Ethereum.Transactions;

namespace Evoq.Ethereum.EAS;

/// <summary>
/// A client for the EAS schema registry.
/// </summary>
public class SchemaRegistry : IGetSchema, IGetVersion, IRegisterSchema
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaRegistry"/> class.
    /// </summary>
    public SchemaRegistry(EthereumAddress contractAddress)
    {
        this.ContractAddress = contractAddress;
    }

    //

    /// <summary>
    /// The address of the schema registry contract.
    /// </summary>
    public EthereumAddress ContractAddress { get; }

    //

    /// <summary>
    /// Gets a schema from the registry
    /// </summary>
    /// <param name="context">The context to use for the interaction.</param>
    /// <param name="schema">The schema string (e.g. "uint256 value, string name")</param>
    /// <param name="resolver">The resolver address</param>
    /// <param name="revocable">Whether the schema is revocable</param>
    /// <returns>The schema record</returns>
    public async Task<ISchemaRecord> GetSchemaAsync(
        InteractionContext context, string schema, EthereumAddress? resolver = null, bool revocable = true)
    {
        if (string.IsNullOrEmpty(schema))
        {
            throw new ArgumentException("Schema cannot be null or empty", nameof(schema));
        }

        // Clean up schema string - remove brackets and trim whitespace
        schema = schema.Trim();
        if (schema.StartsWith("(") && schema.EndsWith(")"))
        {
            schema = schema[1..^1];
        }

        var uid = SchemaUID.FormatSchemaUID(schema, resolver.GetValueOrDefault(EthereumAddress.Zero), revocable);

        return await this.GetSchemaAsync(context, uid);
    }

    /// <summary>
    /// Checks if a schema is registered on the registry
    /// </summary>
    /// <param name="context">The context to use for the interaction.</param>
    /// <param name="schemaUID">The schema UID</param>
    /// <returns>The schema record</returns>
    public async Task<ISchemaRecord> GetSchemaAsync(
        InteractionContext context, Hex schemaUID)
    {
        var schemaReg = GetSchemaRegistryContract(context);
        var args = AbiKeyValues.Create(("uid", schemaUID));

        var result = await schemaReg.CallAsync<SchemaRecordDTO>(
            context, "getSchema", context.Sender.SenderAccount.Address, args);

        // result.UID = schemaUID;

        return result;
    }


    /// <summary>
    /// Attempts to get a schema from the registry
    /// </summary>
    /// <param name="context">The context to use for the interaction.</param>
    /// <param name="schema">The schema string (e.g. "uint256 value, string name")</param>
    /// <param name="resolver">The resolver address</param>
    /// <param name="revocable">Whether the schema is revocable</param>
    /// <returns>A tuple containing the schema record (if found) and a boolean indicating if it was found</returns>
    public async Task<(ISchemaRecord Record, bool WasFound)> TryGetSchemaAsync(
        InteractionContext context, string schema, EthereumAddress? resolver = null, bool revocable = true)
    {
        var record = await this.GetSchemaAsync(context, schema, resolver, revocable);

        var wasFound = !record.UID.IsEmpty() && !record.UID.IsZeroValue();

        return (record, wasFound);
    }

    /// <summary>
    /// Attempts to get a schema from the registry using its UID
    /// </summary>
    /// <param name="context">The context to use for the interaction.</param>
    /// <param name="schemaUID">The schema UID</param>
    /// <returns>A tuple containing the schema record (if found) and a boolean indicating if it was found</returns>
    public async Task<(ISchemaRecord Record, bool WasFound)> TryGetSchemaAsync(
        InteractionContext context, Hex schemaUID)
    {
        var record = await GetSchemaAsync(context, schemaUID);

        var wasFound = !record.UID.IsEmpty() && !record.UID.IsZeroValue();

        return (record, wasFound);
    }

    /// <summary>
    /// Registers a new schema on the registry
    /// </summary>
    /// <param name="context">The context to use for the interaction.</param>
    /// <param name="request">The schema registration request.</param>
    /// <returns>The schema UID</returns>
    public async Task<TransactionResult<Hex>> RegisterAsync(
        InteractionContext context, ISchemaDescription request)
    {
        var schema = request.Schema;
        var resolver = request.Resolver.IsEmpty ? EthereumAddress.Zero : request.Resolver;
        var revocable = request.Revocable;

        if (string.IsNullOrEmpty(schema))
        {
            throw new ArgumentException("Schema cannot be null or empty", nameof(schema));
        }

        // Clean up schema string - remove brackets and trim whitespace
        schema = schema.Trim();
        if (schema.StartsWith("(") && schema.EndsWith(")"))
        {
            schema = schema[1..^1];
        }

        var schemaReg = GetSchemaRegistryContract(context);
        var args = AbiKeyValues.Create(
            ("schema", schema),
            ("resolver", resolver),
            ("revocable", revocable));

        var estimate = await schemaReg.EstimateTransactionFeeAsync(
            context, "register", context.Sender.SenderAccount.Address, null, args);

        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);
        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, EtherAmount.Zero)
        {
            WaitForReceiptTimeout = context.WaitForReceiptTimeout
        };

        var receipt = await runner.RunTransactionAsync(
            context, schemaReg, "register", options, args);

        var computedUID = SchemaUID.FormatSchemaUID(schema, resolver, revocable);

        return new TransactionResult<Hex>(receipt, computedUID);
    }

    /// <summary>
    /// Get the semantic version of the contract.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <returns>The semantic version information.</returns>
    public async Task<SemanticVersion> GetVersionAsync(InteractionContext context)
    {
        var schemaReg = GetSchemaRegistryContract(context);
        var result = await schemaReg.CallAsync(
            context,
            "version",
            context.Sender.SenderAccount.Address,
            AbiKeyValues.Create());

        if (!result.TryFirst(out var first))
        {
            throw new EASException("Version not found");
        }

        if (first.Value is not string version)
        {
            throw new EASException("Version not found");
        }

        var parts = version.Split('.');

        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out var major) ||
            !int.TryParse(parts[1], out var minor) ||
            !int.TryParse(parts[2], out var patch))
        {
            throw new EASException($"Invalid version format: {version}");
        }

        return new SemanticVersion(major, minor, patch);
    }

    //

    private Contract GetSchemaRegistryContract(InteractionContext context)
    {
        var networkId = ChainNames.GetChainId(context.Endpoint.NetworkName);
        var chainId = ulong.Parse(networkId);
        var chain = Chain.CreateDefault(chainId, new Uri(context.Endpoint.URL), context.Endpoint.LoggerFactory);
        var abi = Contracts.GetSchemaRegistryJsonABI(networkId);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(abi));
        var schemaReg = chain.GetContract(this.ContractAddress, context.Endpoint, context.Sender, stream);

        return schemaReg;
    }
}
