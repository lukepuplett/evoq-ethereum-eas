using Evoq.Blockchain;
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
    static readonly LogLevel logLevel = LogLevel.Information;
    static readonly string suffix = DateTimeOffset.UtcNow.Ticks.ToString();

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

    [TestMethod]
    public async Task Test_0_08_GetSchema_Unregistered_ReturnsNull()
    {
        var context = EthereumTestContext.CreateHardhatContext(out var logger);
        var registry = new SchemaRegistry(registryAddress);

        // Create a random schema that hasn't been registered
        var randomSuffix = Guid.NewGuid().ToString("N");
        var schema = $"uint256 randomValue, string randomName{randomSuffix}";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        var result = await registry.GetSchemaAsync(context, schema, resolver, revocable);

        Assert.IsTrue(result.UID.IsEmpty() || result.UID.IsZeroValue(), "UID should be empty or zero");

        logger.LogInformation($"Successfully verified that unregistered schema '{schema}' returns null");
    }

    [TestMethod]
    public async Task Test_0_09_TryGetSchema_Existing_Success()
    {
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);
        var registry = new SchemaRegistry(registryAddress);

        // Use a schema we know exists from previous tests
        var schema = $"uint256 value, string name{suffix}";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        var (record, wasFound) = await registry.TryGetSchemaAsync(context, schema, resolver, revocable);

        Assert.IsTrue(wasFound, "Schema should be found");
        Assert.IsNotNull(record, "Record should not be null");
        Assert.AreEqual(schema, record.Schema, "Schema should match");
        Assert.AreEqual(resolver, record.Resolver, "Resolver should match");
        Assert.AreEqual(revocable, record.Revocable, "Revocable should match");

        logger.LogInformation($"Successfully found schema '{schema}' with UID: {record.UID}");
    }

    [TestMethod]
    public async Task Test_0_10_TryGetSchema_Unregistered_ReturnsFalse()
    {
        var context = EthereumTestContext.CreateHardhatContext(out var logger);
        var registry = new SchemaRegistry(registryAddress);

        // Create a random schema that hasn't been registered
        var randomSuffix = Guid.NewGuid().ToString("N");
        var schema = $"uint256 randomValue, string randomName{randomSuffix}";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        var (record, wasFound) = await registry.TryGetSchemaAsync(context, schema, resolver, revocable);

        Assert.IsFalse(wasFound, "Schema should not be found");
        Assert.IsNotNull(record, "Record should not be null even when not found");
        Assert.IsTrue(record.UID.IsEmpty() || record.UID.IsZeroValue(), "UID should be empty or zero for non-existent schema");

        logger.LogInformation($"Successfully verified that unregistered schema '{schema}' returns wasFound=false");
    }

    [TestMethod]
    public async Task Test_0_11_TryGetSchemaByUID_Existing_Success()
    {
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);
        var registry = new SchemaRegistry(registryAddress);

        // Use a schema we know exists and get its UID
        var schema = $"uint256 value, string name{suffix}";
        var revocable = true;
        var resolver = EthereumAddress.Zero;
        var uid = SchemaUID.FormatSchemaUID(schema, resolver, revocable);

        var (record, wasFound) = await registry.TryGetSchemaAsync(context, uid);

        Assert.IsTrue(wasFound, "Schema should be found");
        Assert.IsNotNull(record, "Record should not be null");
        Assert.AreEqual(schema, record.Schema, "Schema should match");
        Assert.AreEqual(resolver, record.Resolver, "Resolver should match");
        Assert.AreEqual(revocable, record.Revocable, "Revocable should match");

        logger.LogInformation($"Successfully found schema by UID: {uid}");
    }

    [TestMethod]
    public async Task Test_0_12_TryGetSchemaByUID_NonExistent_ReturnsFalse()
    {
        var context = EthereumTestContext.CreateHardhatContext(out var logger);
        var registry = new SchemaRegistry(registryAddress);

        // Create a random UID that won't exist
        var randomUID = new Hex(Guid.NewGuid().ToByteArray());

        var (record, wasFound) = await registry.TryGetSchemaAsync(context, randomUID);

        Assert.IsFalse(wasFound, "Schema should not be found");
        Assert.IsNotNull(record, "Record should not be null even when not found");
        Assert.IsTrue(record.UID.IsEmpty() || record.UID.IsZeroValue(), "UID should be empty or zero for non-existent schema");

        logger.LogInformation($"Successfully verified that non-existent UID returns wasFound=false");
    }
}