using System;
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
/// A client for the Ethereum Attestation Service (EAS) contract.
/// </summary>
public class EAS : IAttest, IRevoke
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EAS"/> class.
    /// </summary>
    public EAS(EthereumAddress contractAddress)
    {
        this.ContractAddress = contractAddress;
    }

    //

    /// <summary>
    /// The address of the EAS contract.
    /// </summary>
    public EthereumAddress ContractAddress { get; }

    // transactions

    /// <summary>
    /// Creates a new attestation
    /// </summary>
    /// <returns>The transaction result containing the UID of the attestation.</returns>
    public async Task<TransactionResult<Hex>> AttestAsync(
        InteractionContext context, AttestationRequest request)
    {
        var eas = GetEASContract(context);

        var attestationRequestData = AbiKeyValues.Create(
            ("recipient", request.Data.Recipient),
            ("expirationTime", request.Data.ExpirationTime.ToUniversalTime().ToUnixTimestamp()),
            ("revocable", request.Data.Revocable),
            ("refUID", request.Data.RefUID),
            ("data", request.Data.Data),
            ("value", request.Data.Value.WeiValue)
        );

        var attestationRequest = AbiKeyValues.Create(
            ("schema", request.Schema),
            ("data", attestationRequestData)
        );

        var arguments = AbiKeyValues.Create(("request", attestationRequest));

        var estimate = await eas.EstimateTransactionFeeAsync(
            "attest", context.Sender.SenderAccount.Address, request.Data.Value.ToWei(), arguments, context.CancellationToken);

        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, request.Data.Value);
        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);

        var receipt = await runner.RunTransactionAsync(
            eas, "attest", options, arguments, context.CancellationToken);

        // Get UID from event logs
        if (!eas.TryReadEventLogsFromReceipt(receipt, "Attested", out var _, out var attested))
        {
            throw new EASException(
                $"The attestation was successfully submitted as transaction {receipt.TransactionHash}, " +
                "but the event log was not found.")
            {
                TransactionHash = receipt.TransactionHash,
            };
        }

        if (!attested!.TryGetValue("uid", out var uid))
        {
            throw new EASException(
                $"The attestation was successfully submitted as transaction {receipt.TransactionHash}, " +
                "but the UID of the attestation was not found in the event log.")
            {
                TransactionHash = receipt.TransactionHash,
            };
        }

        if (uid is byte[] uidBytes)
        {
            return new TransactionResult<Hex>(receipt, new Hex(uidBytes));
        }

        if (uid is Hex uidHex)
        {
            return new TransactionResult<Hex>(receipt, uidHex);
        }

        throw new EASException(
            $"The attestation was successfully submitted as transaction {receipt.TransactionHash}, " +
            $"but the UID '{uid}' in the attestation could not be converted to a Hex value.")
        {
            TransactionHash = receipt.TransactionHash,
        };
    }

    /// <summary>
    /// Revokes an attestation
    /// </summary>
    /// <returns>The transaction hash.</returns>
    public async Task<TransactionResult<Hex>> RevokeAsync(
        InteractionContext context, RevocationRequest request)
    {
        var eas = GetEASContract(context);

        var data = AbiKeyValues.Create(
            ("uid", request.Data.Uid),
            ("value", request.Data.Value)
        );

        var args = AbiKeyValues.Create(
            ("schema", request.Schema),
            ("data", data)
        );

        var estimate = await eas.EstimateTransactionFeeAsync(
            "revoke", context.Sender.SenderAccount.Address, request.Data.Value.ToWei(), args, context.CancellationToken);

        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, request.Data.Value);
        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);

        var receipt = await runner.RunTransactionAsync(
            eas, "revoke", options, args, context.CancellationToken);

        return new TransactionResult<Hex>(receipt, receipt.TransactionHash);
    }

    // views and queries

    /// <summary>
    /// Gets an attestation by its UID
    /// </summary>
    public async Task<IAttestation> GetAttestationAsync(
        InteractionContext context, Hex uid)
    {
        var eas = GetEASContract(context);

        var result = await eas.CallAsync<AttestationDTO>(
            "getAttestation",
            context.Sender.SenderAccount.Address,
            AbiKeyValues.Create(("uid", uid)),
            context.CancellationToken);

        return result;
    }

    /// <summary>
    /// Checks if an attestation is valid
    /// </summary>
    public async Task<bool> IsAttestationValidAsync(
        InteractionContext context, Hex uid)
    {
        var eas = GetEASContract(context);
        var args = AbiKeyValues.Create(("uid", uid));

        var result = await eas.CallAsync<bool>(
            "isAttestationValid", context.Sender.SenderAccount.Address, args, context.CancellationToken);

        return result;
    }

    /// <summary>
    /// Gets the address of the global schema registry
    /// </summary>
    /// <returns>The address of the global schema registry.</returns>
    public async Task<EthereumAddress> GetSchemaRegistryAsync(InteractionContext context)
    {
        var eas = GetEASContract(context);

        var result = await eas.CallAsync<EthereumAddress>(
            "getSchemaRegistry",
            context.Sender.SenderAccount.Address,
            AbiKeyValues.Create(),
            context.CancellationToken);

        return result;
    }

    /// <summary>
    /// Gets the timestamp that the specified data was timestamped with
    /// </summary>
    /// <param name="context">The interaction context</param>
    /// <param name="data">The data to query</param>
    /// <returns>The timestamp the data was timestamped with, or DateTimeOffset.MinValue if not timestamped</returns>
    public async Task<DateTimeOffset> GetTimestampAsync(
        InteractionContext context,
        Hex data)
    {
        var eas = GetEASContract(context);

        var timestamp = await eas.CallAsync<ulong>(
            "getTimestamp",
            context.Sender.SenderAccount.Address,
            AbiKeyValues.Create(("data", data)),
            context.CancellationToken);

        return timestamp == 0
            ? DateTimeOffset.MinValue
            : DateTimeOffset.FromUnixTimeSeconds((long)timestamp);
    }

    //

    private Contract GetEASContract(InteractionContext context)
    {
        var networkId = ChainNames.GetChainId(context.Endpoint.NetworkName);
        var chainId = ulong.Parse(networkId);
        var chain = Chain.CreateDefault(chainId, new Uri(context.Endpoint.URL), context.Endpoint.LoggerFactory);
        var abi = Contracts.GetEASJsonABI(networkId);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(abi));
        var eas = chain.GetContract(this.ContractAddress, context.Endpoint, context.Sender, stream);

        return eas;
    }
}