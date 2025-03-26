using System;
using System.Collections;
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
public class SchemaRegistry : IGetSchema
{

    //

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

        // var result = await schemaReg.CallAsync(
        //     "getSchema", context.Sender.SenderAccount.Address, args, context.CancellationToken);

        // throw new NotImplementedException();

        var result = await schemaReg.CallAsync<SchemaRecordDTO>(
            "getSchema", context.Sender.SenderAccount.Address, args, context.CancellationToken);

        result.UID = schemaUID;

        return result;
    }

    /// <summary>
    /// Registers a new schema on the registry
    /// </summary>
    /// <param name="context">The context to use for the interaction.</param>
    /// <param name="schema">The schema string (e.g. "uint256 value, string name")</param>
    /// <param name="resolver">Optional resolver contract address</param>
    /// <param name="revocable">Whether attestations can be revoked</param>
    /// <returns>The schema UID</returns>
    public async Task<TransactionResult<Hex>> Register(
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

        var schemaReg = GetSchemaRegistryContract(context);
        var args = AbiKeyValues.Create(
            ("schema", schema),
            ("resolver", resolver.GetValueOrDefault(EthereumAddress.Zero)),
            ("revocable", revocable));

        var estimate = await schemaReg.EstimateTransactionFeeAsync(
            "register", context.Sender.SenderAccount.Address, null, args, context.CancellationToken);

        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, EtherAmount.Zero);
        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);

        var receipt = await runner.RunTransactionAsync(
            schemaReg, "register", options, args, context.CancellationToken);

        var computedUID = SchemaUID.FormatSchemaUID(schema, resolver.GetValueOrDefault(EthereumAddress.Zero), revocable);

        return new TransactionResult<Hex>(receipt, computedUID);
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
