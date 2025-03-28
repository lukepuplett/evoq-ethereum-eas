using System;
using System.IO;
using System.Linq;
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
public class EAS : IAttest, IRevoke, ITimestamp, IRevokeOffchain
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
            ("recipient", request.RequestData.Recipient),
            ("expirationTime", request.RequestData.ExpirationTime.ToUniversalTime().ToUnixTimestamp()),
            ("revocable", request.RequestData.Revocable),
            ("refUID", request.RequestData.RefUID),
            ("data", request.RequestData.Data),
            ("value", request.RequestData.Value.WeiValue)
        );

        var attestationRequest = AbiKeyValues.Create(
            ("schema", request.SchemaUID),
            ("data", attestationRequestData)
        );

        var arguments = AbiKeyValues.Create(("request", attestationRequest));

        var estimate = await eas.EstimateTransactionFeeAsync(
            "attest", context.Sender.SenderAccount.Address, request.RequestData.Value.ToWei(), arguments, context.CancellationToken);

        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, request.RequestData.Value);
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
            ("value", request.Data.Value.ToWei())
        );

        var req = AbiKeyValues.Create(
            ("schema", request.Schema),
            ("data", data)
        );

        var args = AbiKeyValues.Create(("request", req));

        var estimate = await eas.EstimateTransactionFeeAsync(
            "revoke", context.Sender.SenderAccount.Address, request.Data.Value.ToWei(), args, context.CancellationToken);

        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, request.Data.Value);
        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);

        var receipt = await runner.RunTransactionAsync(
            eas, "revoke", options, args, context.CancellationToken);

        // Wait for the receipt and check for the Revoked event
        if (!eas.TryReadEventLogsFromReceipt(receipt, "Revoked", out var _, out var revoked))
        {
            throw new EASException(
                $"The revocation was successfully submitted as transaction {receipt.TransactionHash}, " +
                "but the event log was not found.")
            {
                TransactionHash = receipt.TransactionHash,
            };
        }

        return new TransactionResult<Hex>(receipt, receipt.TransactionHash);
    }

    /// <inheritdoc />
    public async Task<TransactionResult<DateTimeOffset>> TimestampAsync(
        InteractionContext context, Hex data)
    {
        var eas = GetEASContract(context);
        var args = AbiKeyValues.Create(("data", data));

        var estimate = await eas.EstimateTransactionFeeAsync(
            "timestamp", context.Sender.SenderAccount.Address, null, args, context.CancellationToken);

        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, EtherAmount.Zero);
        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);

        var receipt = await runner.RunTransactionAsync(
            eas, "timestamp", options, args, context.CancellationToken);

        // Get timestamp from event logs
        if (!eas.TryReadEventLogsFromReceipt(receipt, "Timestamped", out var timestamped, out var _))
        {
            throw new EASException(
                $"The timestamp was successfully submitted as transaction {receipt.TransactionHash}, " +
                "but the event log was not found.")
            {
                TransactionHash = receipt.TransactionHash,
            };
        }

        if (!timestamped!.TryGetValue("timestamp", out var timestampValue))
        {
            throw new EASException(
                $"The timestamp was successfully submitted as transaction {receipt.TransactionHash}, " +
                "but the timestamp was not found in the event log.")
            {
                TransactionHash = receipt.TransactionHash,
            };
        }

        if (timestampValue is ulong timestampUlong)
        {
            return new TransactionResult<DateTimeOffset>(receipt, DateTimeOffset.FromUnixTimeSeconds((long)timestampUlong));
        }

        throw new EASException(
            $"The timestamp was successfully submitted as transaction {receipt.TransactionHash}, " +
            $"but the timestamp '{timestampValue}' in the event could not be converted to a ulong value.")
        {
            TransactionHash = receipt.TransactionHash,
        };
    }

    /// <inheritdoc />
    public async Task<TransactionResult<DateTimeOffset>> MultiTimestampAsync(
        InteractionContext context, Hex[] data)
    {
        var eas = GetEASContract(context);
        var args = AbiKeyValues.Create(("data", data));

        var estimate = await eas.EstimateTransactionFeeAsync(
            "multiTimestamp", context.Sender.SenderAccount.Address, null, args, context.CancellationToken);

        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, EtherAmount.Zero);
        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);

        var receipt = await runner.RunTransactionAsync(
            eas, "multiTimestamp", options, args, context.CancellationToken);

        // Get timestamp from event logs
        if (!eas.TryReadEventLogsFromReceipt(receipt, "Timestamped", out var timestamped, out var _))
        {
            throw new EASException(
                $"The multi-timestamp was successfully submitted as transaction {receipt.TransactionHash}, " +
                "but the event log was not found.")
            {
                TransactionHash = receipt.TransactionHash,
            };
        }

        if (!timestamped!.TryGetValue("timestamp", out var timestampValue))
        {
            throw new EASException(
                $"The multi-timestamp was successfully submitted as transaction {receipt.TransactionHash}, " +
                "but the timestamp was not found in the event log.")
            {
                TransactionHash = receipt.TransactionHash,
            };
        }

        if (timestampValue is ulong timestampUlong)
        {
            return new TransactionResult<DateTimeOffset>(receipt, DateTimeOffset.FromUnixTimeSeconds((long)timestampUlong));
        }

        throw new EASException(
            $"The multi-timestamp was successfully submitted as transaction {receipt.TransactionHash}, " +
            $"but the timestamp '{timestampValue}' in the event could not be converted to a ulong value.")
        {
            TransactionHash = receipt.TransactionHash,
        };
    }

    /// <inheritdoc />
    public async Task<DateTimeOffset> GetTimestampAsync(
        InteractionContext context, Hex data)
    {
        var eas = GetEASContract(context);

        var dic = await eas.CallAsync(
            "getTimestamp",
            context.Sender.SenderAccount.Address,
            AbiKeyValues.Create(("data", data)),
            context.CancellationToken);

        if (dic.Count != 1)
        {
            throw new EASException("The call to getTimestamp returned an unexpected number of results.");
        }

        var value = dic.First().Value;

        if (value == null)
        {
            throw new EASException("The call to getTimestamp returned an unexpected null result.");
        }

        var timestamp = (ulong)value;

        return timestamp == 0
            ? DateTimeOffset.MinValue
            : DateTimeOffset.FromUnixTimeSeconds((long)timestamp);
    }

    /// <inheritdoc />
    public async Task<TransactionResult<DateTimeOffset>> RevokeOffchainAsync(
        InteractionContext context, Hex data)
    {
        var eas = GetEASContract(context);
        var args = AbiKeyValues.Create(("data", data));

        var estimate = await eas.EstimateTransactionFeeAsync(
            "revokeOffchain", context.Sender.SenderAccount.Address, null, args, context.CancellationToken);

        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, EtherAmount.Zero);
        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);

        var receipt = await runner.RunTransactionAsync(
            eas, "revokeOffchain", options, args, context.CancellationToken);

        var timestamp = await GetRevokeOffchainAsync(context, context.Sender.SenderAccount.Address, data);
        return new TransactionResult<DateTimeOffset>(receipt, timestamp);
    }

    /// <inheritdoc />
    public async Task<TransactionResult<DateTimeOffset>> MultiRevokeOffchainAsync(
        InteractionContext context, Hex[] data)
    {
        var eas = GetEASContract(context);
        var args = AbiKeyValues.Create(("data", data));

        var estimate = await eas.EstimateTransactionFeeAsync(
            "multiRevokeOffchain", context.Sender.SenderAccount.Address, null, args, context.CancellationToken);

        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, EtherAmount.Zero);
        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);

        var receipt = await runner.RunTransactionAsync(
            eas, "multiRevokeOffchain", options, args, context.CancellationToken);

        // Get the timestamp from the first piece of data
        var timestamp = await GetRevokeOffchainAsync(context, context.Sender.SenderAccount.Address, data[0]);
        return new TransactionResult<DateTimeOffset>(receipt, timestamp);
    }

    /// <inheritdoc />
    public async Task<DateTimeOffset> GetRevokeOffchainAsync(
        InteractionContext context, EthereumAddress revoker, Hex data)
    {
        var eas = GetEASContract(context);

        var dic = await eas.CallAsync(
            "getRevokeOffchain",
            context.Sender.SenderAccount.Address,
            AbiKeyValues.Create(
                ("revoker", revoker),
                ("data", data)),
            context.CancellationToken);

        if (dic.Count != 1)
        {
            throw new EASException("The call to getRevokeOffchain returned an unexpected number of results.");
        }

        var value = dic.First().Value;

        if (value == null)
        {
            throw new EASException("The call to getRevokeOffchain returned an unexpected null result.");
        }

        var timestamp = (ulong)value;

        return timestamp == 0
            ? DateTimeOffset.MinValue
            : DateTimeOffset.FromUnixTimeSeconds((long)timestamp);
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

        var dic = await eas.CallAsync(
            "isAttestationValid", context.Sender.SenderAccount.Address, args, context.CancellationToken);

        if (dic.Count != 1)
        {
            throw new EASException("The call to getRevokeOffchain returned an unexpected number of results.");
        }

        var value = dic.First().Value;

        if (value == null)
        {
            throw new EASException("The call to getRevokeOffchain returned an unexpected null result.");
        }

        return (bool)value;
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