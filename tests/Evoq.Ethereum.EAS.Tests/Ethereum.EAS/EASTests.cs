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

    // Views and queries

    [TestMethod]
    public async Task Test_3_00_GetSchemaRegistry__Success()
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

        // Create a random bytes32 that hasn't been timestamped
        var randomData = new Hex(new byte[32] {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        });

        var timestamp = await eas.GetTimestampAsync(context, randomData);

        logger.LogInformation($"Timestamp for non-timestamped data: {timestamp}");

        Assert.AreEqual(DateTimeOffset.MinValue, timestamp, "Non-timestamped data should return MinValue");
    }

    //
}