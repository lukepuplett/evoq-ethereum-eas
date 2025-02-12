using System;
using System.Threading.Tasks;
using Evoq.Blockchain;
using Evoq.Ethereum;
using Evoq.Ethereum.Accounts.Blockchain;
using Evoq.Ethereum.JsonRPC;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Evoq.Ethereum.EAS;

public class EASNethereum : IAttest
{
    private readonly Hex pk;
    private readonly INonceStore nonceStore;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<EASNethereum> logger;

    //

    public EASNethereum(Endpoint endpoint, Sender sender, ILoggerFactory loggerFactory)
    {
        this.Endpoint = endpoint;

        this.pk = sender.PrivateKey;
        this.nonceStore = sender.NonceStore;
        this.loggerFactory = loggerFactory;
        this.logger = loggerFactory.CreateLogger<EASNethereum>();
    }

    //

    public Endpoint Endpoint { get; }

    //

    /// <summary>
    /// Attest a new attestation.
    /// </summary>
    /// <param name="schemaUID">The UID of the schema to attest to.</param>
    /// <param name="request">The attestation data.</param>
    /// <returns>The UID of the attestation.</returns>
    /// <exception cref="AttestationFailedException">Thrown if the attestation fails.</exception>
    public async Task<Hex> AttestAsync(AttestationRequest request)
    {
        var schemaUID = request.Schema;

        this.logger.LogInformation(
            "Attesting to schema {SchemaUID} for {Recipient}",
            schemaUID,
            request.Data.Recipient);

        if (this.pk.IsZeroValue())
        {
            throw new InvalidOperationException("Private key is required to attest to a schema.");
        }

        var account = new Account(this.pk.ToString());
        var web3 = new Web3(account, this.Endpoint.URL);
        var getFees = new FeesNethereum(web3);

        var fees = await getFees.CalculateFeesAsync(request.Data.Value, 500 * 1000);

        var requestDataDto = new AttestationRequestDataDTO
        {
            Recipient = request.Data.Recipient.ToString(),
            ExpirationTime = request.Data.ExpirationUnixTimestamp,
            Revocable = request.Data.Revocable,
            RefUID = request.Data.RefUID.ToByteArray(),
            Data = request.Data.Data.ToByteArray(),
            Value = request.Data.Value,
        };

        var requestDto = new AttestationRequestDTO
        {
            Schema = schemaUID.ToByteArray(),
            Data = requestDataDto,
        };

        var networkId = Evoq.Ethereum.ChainNames.GetChainId(this.Endpoint.NetworkName);

        var contract = web3.Eth.GetContract(
            EAS.Contracts.GetEASJsonABI(networkId),
            EAS.Contracts.GetEASAddress(networkId));

        var runner = new TransactionRunnerNethereum(this.pk, this.nonceStore, this.loggerFactory);

        var receipt = await runner.RunTransactionAsync(
            contract, "attest", fees, new object[] { requestDto });

        try
        {
            var attestationEvent = runner.DecodeEvent<AttestedEventDTO>(receipt);

            return new Hex(attestationEvent.Uid);
        }
        catch (MissingEventLogException ex)
        {
            this.logger.LogError(ex, "Attestation event not found in transaction receipt. Dumping logs...");

            foreach (var log in receipt.Logs)
            {
                this.logger.LogError("Log: {Log}", log);
            }

            throw;
        }
    }
}