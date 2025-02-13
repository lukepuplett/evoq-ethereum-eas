using System;
using System.Text;
using System.Threading.Tasks;
using Evoq.Blockchain;
using Evoq.Ethereum.Accounts.Blockchain;
using Evoq.Ethereum.JsonRPC;
using Microsoft.Extensions.Logging;
using Nethereum.ABI;
using Nethereum.Web3;

namespace Evoq.Ethereum.EAS;

/// <summary>
/// A class for registering schemas on the EAS schema registry.
/// </summary>
public class SchemaRegistryNethereum
{
    private readonly string? pk;
    private readonly INonceStore nonceStore;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<SchemaRegistryNethereum> logger;

    //

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaRegistryNethereum"/> class.
    /// </summary>
    /// <param name="endpoint">The endpoint to use for the schema registry.</param>
    /// <param name="sender">The sender to use for the schema registry.</param>
    /// <param name="loggerFactory">The logger factory to use for the schema registry.</param>
    public SchemaRegistryNethereum(Endpoint endpoint, Sender sender, ILoggerFactory loggerFactory)
    {
        this.Endpoint = endpoint;

        this.pk = sender.PrivateKey.ToString();
        this.nonceStore = sender.NonceStore;
        this.loggerFactory = loggerFactory;
        this.logger = loggerFactory.CreateLogger<SchemaRegistryNethereum>();
    }

    //

    /// <summary>
    /// The endpoint to use for the schema registry.
    /// </summary>
    public Endpoint Endpoint { get; }

    //

    /// <summary>
    /// Registers a new schema on the registry
    /// </summary>
    /// <param name="schema">The schema string (e.g. "uint256 value, string name")</param>
    /// <param name="revocable">Whether attestations can be revoked</param>
    /// <param name="resolver">Optional resolver contract address</param>
    /// <returns>The schema UID</returns>
    public async Task<byte[]> Register(string schema, bool revocable, EthereumAddress? resolver = null)
    {
        return await this.Register(schema, resolver.GetValueOrDefault(EthereumAddress.Zero).ToString(), revocable);
    }

    /// <summary>
    /// Registers a new schema on the registry
    /// </summary>
    /// <param name="schema">The schema string (e.g. "uint256 value, string name")</param>
    /// <param name="resolverAddress">Optional resolver contract address</param>
    /// <param name="revocable">Whether attestations can be revoked</param>
    /// <returns>The schema UID</returns>
    public async Task<byte[]> Register(string schema, string resolverAddress, bool revocable)
    {
        this.logger.LogInformation(
            "Registering schema {Schema} with resolver {Resolver} and revocable {Revocable}",
            schema, resolverAddress.Substring(0, 8), revocable);

        if (string.IsNullOrEmpty(this.pk))
        {
            throw new InvalidOperationException("Private key is required to register a schema");
        }

        var getFees = new FeesNethereum(new Web3(this.Endpoint.URL));
        var fees = await getFees.CalculateFeesAsync(0, 250 * 1000);

        var runner = new TransactionRunnerNethereum(this.pk, this.nonceStore, this.loggerFactory);
        var contract = this.GetContract();

        var receipt = await runner.RunTransactionAsync(contract, "register", fees, new object[] { schema, resolverAddress, revocable });

        this.logger.LogDebug("Schema registered in transaction {TransactionHash}", receipt.TransactionHash);

        try
        {
            var registeredEvent = runner.DecodeEvent<RegisteredEventDTO>(receipt);

            return registeredEvent.UID;
        }
        catch (MissingEventLogException ex)
        {
            this.logger.LogError(ex, "Registered event not found in transaction receipt. Dumping logs...");

            foreach (var log in receipt.Logs)
            {
                this.logger.LogError("Log: {Log}", log);
            }

            throw;
        }
    }

    /// <summary>
    /// Gets an existing schema by its solidity signature, resolver address, and revocable flag.
    /// </summary>
    /// <param name="schemaSoliditySignature"></param>
    /// <param name="revocable"></param>
    /// <param name="resolver"></param>
    /// <returns></returns>
    public async Task<ISchemaRecord?> GetSchema(string schemaSoliditySignature, bool revocable, EthereumAddress? resolver)
    {
        var schemaUID = GetSchemaUID(schemaSoliditySignature, revocable, resolver.GetValueOrDefault(EthereumAddress.Zero));

        return await this.GetSchema(schemaUID);
    }

    /// <summary>
    /// Gets an existing schema by its UID
    /// </summary>
    /// <param name="uid">The schema UID</param>
    /// <returns>The schema record</returns>
    public async Task<ISchemaRecord?> GetSchema(byte[] uid)
    {
        if (uid.Length != 32)
        {
            throw new ArgumentException("Schema UID must be 32 bytes long", nameof(uid));
        }

        var providedUid = uid.ToHexStruct();

        this.logger.LogDebug("Getting schema with UID {UID}", providedUid.ToString().Substring(0, 66));

        var contract = this.GetContract();
        var getSchemaFunction = contract.GetFunction("getSchema");

        SchemaRecordDTO? schema = null;

        try
        {
            schema = await getSchemaFunction.CallAsync<SchemaRecordDTO>(uid);

            if (schema == null)
            {
                this.logger.LogDebug("Schema not found");

                return null;
            }
        }
        catch (Exception ex)
        {
            var message = $"Error getting schema with UID {providedUid.ToString().Substring(0, 66)}";

            this.logger.LogError(ex, message);

            throw new FailedToCallFunctionException(message, ex);
        }

        var recordUid = schema.UID.ToHexStruct();

        if (!recordUid.IsZeroValue() && !recordUid.ValueEquals(providedUid))
        {
            throw new Exception(
                $"The schema UID returned from the registry ({recordUid}) does not match the UID provided ({providedUid}).");
        }

        return schema;
    }

    /// <summary>
    /// Gets the version of the schema registry contract
    /// </summary>
    /// <returns>The semantic version string</returns>
    public Task<string> GetVersion()
    {
        throw new NotImplementedException();
    }

    //

    /// <summary>
    /// Computes the schema UID from the schema, resolver address, and revocable flag.
    /// </summary>
    /// <param name="schemaSoliditySignature">The schema 'signature' like 'uint256 number, string name'.</param>
    /// <param name="revocable">Whether the schema is revocable</param>
    /// <param name="resolver">The resolver address</param>
    /// <returns>The schema UID which has length 32</returns>
    public static byte[] GetSchemaUID(string schemaSoliditySignature, bool revocable, EthereumAddress resolver)
    {
        var abiEncode = new ABIEncode();
        var packed = abiEncode.GetSha3ABIEncodedPacked(
            Encoding.UTF8.GetBytes(schemaSoliditySignature),
            resolver.ToByteArray(),
            revocable
        );

        return packed;
    }

    //

    private Nethereum.Contracts.Contract GetContract()
    {
        var web3 = new Web3(this.Endpoint.URL);
        var networkId = ChainNames.GetChainId(this.Endpoint.NetworkName);
        var abi = Contracts.GetSchemaRegistryJsonABI(networkId);
        var address = Contracts.GetSchemaRegistryAddress(networkId);

        return web3.Eth.GetContract(abi, address.ToString());
    }
}
