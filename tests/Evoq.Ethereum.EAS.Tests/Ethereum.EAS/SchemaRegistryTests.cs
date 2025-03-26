using System.Diagnostics;
using System.Numerics;
using Evoq.Blockchain;
using Evoq.Ethereum.Chains;
using Evoq.Ethereum.Contracts;
using Evoq.Ethereum.Crypto;
using Evoq.Ethereum.JsonRPC;
using Evoq.Ethereum.Transactions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace Evoq.Ethereum.EAS;

[TestClass]
public class SchemaRegistryTests
{
    // NOTE / The Hardhat addresses are consistent only when the default Hardhat account
    // is used as the deployer, and it is the accounts first transaction. This is because
    // contract addresses are a hash of the sender and the transaction nonce.

    static readonly EthereumAddress registryAddress = Contracts.GetSchemaRegistryAddress(ChainIds.Hardhat);
    static readonly string suffix = DateTimeOffset.UtcNow.Ticks.ToString();
    static readonly LogLevel logLevel = LogLevel.Information;

    //

    [TestMethod]
    public void Test_00_SchemaUID()
    {
        var schema = $"uint256 value, string name{suffix}";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        var uid = SchemaUID.FormatSchemaUID(schema, resolver, revocable);
        var uidv2 = SchemaUID.FormatSchemaUIDConcat(schema, resolver, revocable);

        Assert.AreEqual(uid, uidv2, "UIDs should be equal");
    }

    [TestMethod]
    public async Task Test_01_RegisterSchema_Initial_Success()
    {
        var registry = new SchemaRegistry(registryAddress);

        InteractionContext context = CreateContext(out var logger);

        var schema = $"uint256 value, string name{suffix}";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        var r = await registry.Register(context, schema, resolver, revocable);

        logger.LogInformation($"'{schema}' registered with UID: {r.Result}");

        Assert.IsTrue(r.Success);
    }

    [TestMethod]
    public async Task Test_02_RegisterSchema_Subsequent_AlreadyExists()
    {
        InteractionContext context = CreateContext(out var logger);

        var registry = new SchemaRegistry(registryAddress);
        var schema = $"uint256 value, string name{suffix}";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        try
        {
            var r = await registry.Register(context, schema, resolver, revocable);

            Assert.Fail("Expected exception");
        }
        catch (JsonRpcRequestFailedException requestFailed)
            when (requestFailed.InnerException is JsonRpcProviderErrorException error)
        {
            Assert.IsTrue(error.JsonRpcErrorCode == -32603);
            Assert.IsTrue(error.Message.Contains("AlreadyExists"));
        }
    }

    [TestMethod]
    public async Task Test_03_GetSchema_Existing_Success()
    {
        InteractionContext context = CreateContext(out var logger);

        var registry = new SchemaRegistry(registryAddress);
        var schema = $"uint256 value, string name{suffix}";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        var r = await registry.GetSchemaAsync(context, schema, resolver, revocable);

        Assert.IsNotNull(r);
        Assert.IsFalse(r.UID.IsEmpty(), "UID should not be empty");
        Assert.IsFalse(r.UID.IsZeroValue(), "UID should not be zero");
        Assert.AreEqual(schema, r.Schema, "Schema should be equal");
        Assert.AreEqual(resolver, r.Resolver, "Resolver should be equal");
        Assert.AreEqual(revocable, r.Revocable, "Revocable should be equal");
    }

    [TestMethod]
    public async Task Test_04_GetSchema_Existing_Success()
    {
        InteractionContext context = CreateContext(out var logger);

        var registry = new SchemaRegistry(registryAddress);
        var schema = $"uint256 value, string name638785811949377690";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        var r = await registry.GetSchemaAsync(context, schema, resolver, revocable);

        Assert.IsNotNull(r);
        Assert.IsFalse(r.UID.IsEmpty(), "UID should not be empty");
        Assert.IsFalse(r.UID.IsZeroValue(), "UID should not be zero");
        Assert.AreEqual(schema, r.Schema, "Schema should be equal");
        Assert.AreEqual(resolver, r.Resolver, "Resolver should be equal");
        Assert.AreEqual(revocable, r.Revocable, "Revocable should be equal");
    }

    [TestMethod]
    public async Task Test_05_RegisterSchema_Subsequent_AlreadyExists()
    {
        InteractionContext context = CreateContext(out var logger);

        var registry = new SchemaRegistry(registryAddress);
        var schema = $"uint256 value, string name638785811949377690";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        try
        {
            var r = await registry.Register(context, schema, resolver, revocable);

            Assert.Fail("Failed to catch expected exception");
        }
        catch (JsonRpcRequestFailedException requestFailed)
            when (requestFailed.InnerException is JsonRpcProviderErrorException error)
        {
            Assert.IsTrue(error.JsonRpcErrorCode == -32603);
            Assert.IsTrue(error.Message.Contains("AlreadyExists"));
        }
    }

    //

    private static InteractionContext CreateContext(out ILogger logger)
    {
        var loggerFactory = LoggerFactory.Create(
            builder => builder.AddSimpleConsole(
                options => options.SingleLine = true).SetMinimumLevel(logLevel));

        logger = loggerFactory.CreateLogger<SchemaRegistryTests>();

        var pkStr = Environment.GetEnvironmentVariable("Blockchain__Ethereum__Addresses__Hardhat1PrivateKey");
        var addrStr = Environment.GetEnvironmentVariable("Blockchain__Ethereum__Addresses__Hardhat1Address");
        var address = EthereumAddress.Parse(addrStr!);

        var endpoint = new Endpoint(ChainNames.Hardhat, ChainNames.Hardhat, "http://localhost:8545", loggerFactory);

        var chain = endpoint.CreateChain();
        var getTransactionCount = () => chain.GetTransactionCountAsync(address, "latest");

        var nonces = new InMemoryNonceStore(loggerFactory, getTransactionCount);

        var account = new SenderAccount(pkStr!, address);
        var sender = new Sender(account, nonces);

        var context = new InteractionContext(endpoint, sender, UseSuggestedGasOptions);

        return context;
    }

    private static GasOptions UseSuggestedGasOptions(ITransactionFeeEstimate estimate)
    {
        return estimate.ToSuggestedGasOptions();
    }
}