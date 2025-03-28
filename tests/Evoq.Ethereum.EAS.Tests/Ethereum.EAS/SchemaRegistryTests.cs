using Evoq.Ethereum.Chains;
using Evoq.Ethereum.JsonRPC;
using Microsoft.Extensions.Logging;

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
    public void Test_0_00_SchemaUID()
    {
        var schema = $"uint256 value, string name{suffix}";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        var uid = SchemaUID.FormatSchemaUID(schema, resolver, revocable);

        Assert.IsFalse(uid.IsEmpty(), "UID should not be empty");
        Assert.IsFalse(uid.IsZeroValue(), "UID should not be zero");
    }

    [TestMethod]
    public async Task Test_0_01_RegisterSchema_Initial_Success()
    {
        var registry = new SchemaRegistry(registryAddress);

        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        var schema = $"uint256 value, string name{suffix}";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        var r = await registry.RegisterAsync(context, schema, resolver, revocable);

        logger.LogInformation($"'{schema}' registered with UID: {r.Result}");

        Assert.IsTrue(r.Success);
    }

    [TestMethod]
    public async Task Test_0_02_RegisterSchema_Subsequent_AlreadyExists()
    {
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        var registry = new SchemaRegistry(registryAddress);
        var schema = $"uint256 value, string name{suffix}";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        try
        {
            var r = await registry.RegisterAsync(context, schema, resolver, revocable);

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
    public async Task Test_0_03_GetSchema_Existing_Success()
    {
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

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
    public async Task Test_0_04_GetSchema_Existing_Success()
    {
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

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
    public async Task Test_0_05_RegisterSchema_Subsequent_AlreadyExists()
    {
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        var registry = new SchemaRegistry(registryAddress);
        var schema = $"uint256 value, string name638785811949377690";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        try
        {
            var r = await registry.RegisterAsync(context, schema, resolver, revocable);

            Assert.Fail("Failed to catch expected exception");
        }
        catch (JsonRpcRequestFailedException requestFailed)
            when (requestFailed.InnerException is JsonRpcProviderErrorException error)
        {
            Assert.IsTrue(error.JsonRpcErrorCode == -32603);
            Assert.IsTrue(error.Message.Contains("AlreadyExists"));
        }
    }

    [TestMethod]
    public async Task Test_0_06_GetVersion_Success()
    {
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);
        var registry = new SchemaRegistry(registryAddress);

        var version = await registry.GetVersionAsync(context);

        Assert.IsNotNull(version);
        Assert.AreEqual(1, version.Major, "Major version should be 1");
        Assert.AreEqual(4, version.Minor, "Minor version should be 4");
        Assert.AreEqual(0, version.Patch, "Patch version should be 0");
        Assert.AreEqual("1.4.0", version.Version, "Version string should match");

        logger.LogInformation($"Schema Registry Version: {version.Version}");
    }

    [TestMethod]
    public async Task Test_0_07_RegisterSchema_IsAHuman()
    {
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        var registry = new SchemaRegistry(registryAddress);
        var schema = "bool isAHuman";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        try
        {
            var r = await registry.RegisterAsync(context, schema, resolver, revocable);
            logger.LogInformation($"'{schema}' registered with UID: {r.Result}");
            Assert.IsTrue(r.Success);
        }
        catch (JsonRpcRequestFailedException requestFailed)
            when (requestFailed.InnerException is JsonRpcProviderErrorException error)
        {
            // We're okay with either success or AlreadyExists
            Assert.IsTrue(error.JsonRpcErrorCode == -32603);
            Assert.IsTrue(error.Message.Contains("AlreadyExists"));
            logger.LogInformation($"'{schema}' schema already exists (as expected)");
        }
    }
}