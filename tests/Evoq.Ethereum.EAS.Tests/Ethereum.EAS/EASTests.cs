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

    //

    [TestMethod]
    public async Task Test_1_00_Attest__Success()
    {
        var eas = new EAS(easAddress);

        InteractionContext context = EthereumTestContext.CreateContext(out var logger);

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

        InteractionContext context = EthereumTestContext.CreateContext(out var logger);

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

    //
}