using Evoq.Blockchain;
using Evoq.Ethereum.Chains;
using Evoq.Ethereum.JsonRPC;
using Microsoft.Extensions.Logging;

namespace Evoq.Ethereum.EAS;

// 0x9cab09bd63e24bc5fd42b523f390351de35951003b67ddcfeca814206c8eaffb

[TestClass]
public class EASTests
{
    // NOTE / The Hardhat addresses are consistent only when the default Hardhat account
    // is used as the deployer, and it is the accounts first transaction. This is because
    // contract addresses are a hash of the sender and the transaction nonce.

    static readonly EthereumAddress easAddress = Contracts.GetEASAddress(ChainIds.Hardhat);
    static readonly LogLevel logLevel = LogLevel.Information;

    // IAttest

    [TestMethod]
    public async Task Test_1_00_Attest__Success()
    {
        var eas = new EAS(easAddress);

        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        var schemaUID = SchemaUID.FormatSchemaUID("bool isAHuman", EthereumAddress.Zero, true);
        var data = new AttestationRequestData(
            Recipient: EthereumAddress.Zero,
            ExpirationTime: DateTimeOffset.UtcNow.AddDays(1),
            Revocable: true,
            RefUID: Hex.Empty,
            Data: Hex.Empty,
            Value: EtherAmount.Zero);

        var result = await eas.AttestAsync(context, new AttestationRequest(schemaUID, data));

        logger.LogInformation($"Attestation UID: {result.Result}");

        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public async Task Test_1_01_AttestThenGet__Success()
    {
        var eas = new EAS(easAddress);

        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        var schemaUID = SchemaUID.FormatSchemaUID("bool isAHuman", EthereumAddress.Zero, true);
        var data = new AttestationRequestData(
            Recipient: EthereumAddress.Zero,
            ExpirationTime: DateTimeOffset.UtcNow.AddDays(1),
            Revocable: true,
            RefUID: Hex.Empty,
            Data: Hex.Empty,
            Value: EtherAmount.Zero);

        var result = await eas.AttestAsync(context, new AttestationRequest(schemaUID, data));

        logger.LogInformation($"Attestation UID: {result.Result}");

        Assert.IsTrue(result.Success);

        var attestation = await eas.GetAttestationAsync(context, result.Result);

        logger.LogInformation($"Attestation: {attestation}");

        Assert.IsNotNull(attestation);
        Assert.IsTrue(attestation.UID == result.Result);
        Assert.IsTrue(attestation.Schema == schemaUID);
    }

    // IRevoke - missing

    // ITimestamp

    // IRevokeOffchain

    // Views and queries

    [TestMethod]
    public async Task Test_2_00_GetSchemaRegistry__Success()
    {
        var eas = new EAS(easAddress);

        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        var result = await eas.GetSchemaRegistryAsync(context);

        logger.LogInformation($"Schema registry: {result}");

        Assert.AreEqual(result, EthereumAddress.Empty); // deploy of EAS contract to Hardhat does not set the schema registry
    }

    [TestMethod]
    public async Task Test_3_01_GetTimestamp_NotTimestamped_ReturnsMinValue()
    {
        var eas = new EAS(easAddress);
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        // Create random data that hasn't been timestamped
        var random = new Random();
        var dataBytes = new byte[32];
        random.NextBytes(dataBytes);
        var randomData = new Hex(dataBytes);

        var timestamp = await eas.GetTimestampAsync(context, randomData);

        logger.LogInformation($"Timestamp for non-timestamped data: {timestamp}");

        Assert.AreEqual(DateTimeOffset.MinValue, timestamp, "Non-timestamped data should return MinValue");
    }

    [TestMethod]
    public async Task Test_3_02_Timestamp_Success()
    {
        var eas = new EAS(easAddress);
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        // Create random data to timestamp
        var random = new Random();
        var dataBytes = new byte[32];
        random.NextBytes(dataBytes);
        var data = new Hex(dataBytes);

        // First verify it's not timestamped
        var initialTimestamp = await eas.GetTimestampAsync(context, data);
        Assert.AreEqual(DateTimeOffset.MinValue, initialTimestamp, "Data should not be timestamped initially");

        // Timestamp the data
        var result = await eas.TimestampAsync(context, data);
        Assert.IsTrue(result.Success, "Timestamp operation should succeed");
        Assert.IsTrue(result.Result > DateTimeOffset.MinValue, "Timestamp should be set");

        // Verify the timestamp was recorded
        var finalTimestamp = await eas.GetTimestampAsync(context, data);
        Assert.AreEqual(result.Result, finalTimestamp, "Retrieved timestamp should match the recorded timestamp");
    }

    [TestMethod]
    public async Task Test_3_03_MultiTimestamp_Success()
    {
        var eas = new EAS(easAddress);
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        // Create multiple random pieces of data to timestamp
        var random = new Random();
        var data1Bytes = new byte[32];
        var data2Bytes = new byte[32];
        random.NextBytes(data1Bytes);
        random.NextBytes(data2Bytes);
        var data1 = new Hex(data1Bytes);
        var data2 = new Hex(data2Bytes);
        var data = new[] { data1, data2 };

        // Verify none are timestamped initially
        foreach (var d in data)
        {
            var initialTimestamp = await eas.GetTimestampAsync(context, d);
            Assert.AreEqual(DateTimeOffset.MinValue, initialTimestamp, "Data should not be timestamped initially");
        }

        // Timestamp all data
        var result = await eas.MultiTimestampAsync(context, data);
        Assert.IsTrue(result.Success, "Multi-timestamp operation should succeed");
        Assert.IsTrue(result.Result > DateTimeOffset.MinValue, "Timestamp should be set");

        // Verify all data was timestamped
        foreach (var d in data)
        {
            var finalTimestamp = await eas.GetTimestampAsync(context, d);
            Assert.AreEqual(result.Result, finalTimestamp, "Retrieved timestamp should match the recorded timestamp");
        }
    }

    [TestMethod]
    public async Task Test_3_04_RevokeOffchain_Success()
    {
        var eas = new EAS(easAddress);
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        // Create random data to revoke off-chain
        var random = new Random();
        var dataBytes = new byte[32];
        random.NextBytes(dataBytes);
        var data = new Hex(dataBytes);

        // First verify it's not revoked
        var initialTimestamp = await eas.GetRevokeOffchainAsync(context, context.Sender.SenderAccount.Address, data);
        Assert.AreEqual(DateTimeOffset.MinValue, initialTimestamp, "Data should not be revoked initially");

        // Revoke the data off-chain
        var result = await eas.RevokeOffchainAsync(context, data);
        Assert.IsTrue(result.Success, "Off-chain revocation should succeed");
        Assert.IsTrue(result.Result > DateTimeOffset.MinValue, "Timestamp should be set");

        // Verify the revocation was recorded
        var finalTimestamp = await eas.GetRevokeOffchainAsync(context, context.Sender.SenderAccount.Address, data);
        Assert.AreEqual(result.Result, finalTimestamp, "Retrieved timestamp should match the recorded timestamp");
    }

    [TestMethod]
    public async Task Test_3_05_MultiRevokeOffchain_Success()
    {
        var eas = new EAS(easAddress);
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        // Create multiple random pieces of data to revoke off-chain
        var random = new Random();
        var data1Bytes = new byte[32];
        var data2Bytes = new byte[32];
        random.NextBytes(data1Bytes);
        random.NextBytes(data2Bytes);
        var data1 = new Hex(data1Bytes);
        var data2 = new Hex(data2Bytes);
        var data = new[] { data1, data2 };

        // Verify none are revoked initially
        foreach (var d in data)
        {
            var initialTimestamp = await eas.GetRevokeOffchainAsync(context, context.Sender.SenderAccount.Address, d);
            Assert.AreEqual(DateTimeOffset.MinValue, initialTimestamp, "Data should not be revoked initially");
        }

        // Revoke all data off-chain
        var result = await eas.MultiRevokeOffchainAsync(context, data);
        Assert.IsTrue(result.Success, "Multi off-chain revocation should succeed");
        Assert.IsTrue(result.Result > DateTimeOffset.MinValue, "Timestamp should be set");

        // Verify all data was revoked
        foreach (var d in data)
        {
            var finalTimestamp = await eas.GetRevokeOffchainAsync(context, context.Sender.SenderAccount.Address, d);
            Assert.AreEqual(result.Result, finalTimestamp, "Retrieved timestamp should match the recorded timestamp");
        }
    }

    [TestMethod]
    public async Task Test_3_06_RevokeOffchain_DifferentRevokers_Success()
    {
        var eas = new EAS(easAddress);
        InteractionContext context = EthereumTestContext.CreateHardhatContext(out var logger);

        // Create random data to revoke off-chain
        var random = new Random();
        var dataBytes = new byte[32];
        random.NextBytes(dataBytes);
        var data = new Hex(dataBytes);

        // Verify data is not revoked initially
        var initialTimestamp = await eas.GetRevokeOffchainAsync(context, context.Sender.SenderAccount.Address, data);
        Assert.AreEqual(DateTimeOffset.MinValue, initialTimestamp, "Data should not be revoked initially");

        // Revoke the data
        var result = await eas.RevokeOffchainAsync(context, data);
        Assert.IsTrue(result.Success, "Off-chain revocation should succeed");
        Assert.IsTrue(result.Result > DateTimeOffset.MinValue, "Timestamp should be set");

        // Verify the revocation was recorded
        var timestamp = await eas.GetRevokeOffchainAsync(context, context.Sender.SenderAccount.Address, data);
        Assert.AreEqual(result.Result, timestamp, "Retrieved timestamp should match the recorded timestamp");

        // Attempt to revoke the same data again
        try
        {
            await eas.RevokeOffchainAsync(context, data);
            Assert.Fail("Second revocation should have failed with AlreadyRevokedOffchain error");
        }
        catch (JsonRpcRequestFailedException ex) when (ex.InnerException is JsonRpcProviderErrorException inner &&
            inner.Message.Contains("AlreadyRevokedOffchain()"))
        {
            // Expected error
        }
    }
}