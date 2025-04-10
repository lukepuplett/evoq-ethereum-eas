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
            context, "attest", context.Sender.SenderAccount.Address, request.RequestData.Value.ToWei(), arguments);

        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);
        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, request.RequestData.Value)
        {
            WaitForReceiptTimeout = context.WaitForReceiptTimeout
        };

        var receipt = await runner.RunTransactionAsync(
            context, eas, "attest", options, arguments);

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
            context, "revoke", context.Sender.SenderAccount.Address, request.Data.Value.ToWei(), args);

        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);
        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, request.Data.Value)
        {
            WaitForReceiptTimeout = context.WaitForReceiptTimeout
        };

        var receipt = await runner.RunTransactionAsync(
            context, eas, "revoke", options, args);

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
            context, "timestamp", context.Sender.SenderAccount.Address, null, args);

        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);
        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, EtherAmount.Zero)
        {
            WaitForReceiptTimeout = context.WaitForReceiptTimeout
        };

        var receipt = await runner.RunTransactionAsync(
            context, eas, "timestamp", options, args);

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
            context, "multiTimestamp", context.Sender.SenderAccount.Address, null, args);

        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);
        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, EtherAmount.Zero)
        {
            WaitForReceiptTimeout = context.WaitForReceiptTimeout
        };

        var receipt = await runner.RunTransactionAsync(
            context, eas, "multiTimestamp", options, args);

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
            context,
            "getTimestamp",
            context.Sender.SenderAccount.Address,
            AbiKeyValues.Create(("data", data)));

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
            context, "revokeOffchain", context.Sender.SenderAccount.Address, null, args);

        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);
        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, EtherAmount.Zero)
        {
            WaitForReceiptTimeout = context.WaitForReceiptTimeout
        };

        var receipt = await runner.RunTransactionAsync(
            context, eas, "revokeOffchain", options, args);

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
            context, "multiRevokeOffchain", context.Sender.SenderAccount.Address, null, args);

        var gas = context.FeeEstimateToGasOptions(estimate);
        var options = new ContractInvocationOptions(gas, EtherAmount.Zero)
        {
            WaitForReceiptTimeout = context.WaitForReceiptTimeout
        };

        var runner = new TransactionRunnerNative(context.Sender, context.Endpoint.LoggerFactory);

        var receipt = await runner.RunTransactionAsync(
            context, eas, "multiRevokeOffchain", options, args);

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
            context,
            "getRevokeOffchain",
            context.Sender.SenderAccount.Address,
            AbiKeyValues.Create(
                ("revoker", revoker),
                ("data", data)));

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
            context,
            "getAttestation",
            context.Sender.SenderAccount.Address,
            AbiKeyValues.Create(("uid", uid)));

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
            context,
            "isAttestationValid",
            context.Sender.SenderAccount.Address,
            args);

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
            context,
            "getSchemaRegistry",
            context.Sender.SenderAccount.Address,
            AbiKeyValues.Create());

        return result;
    }

    //

    /// <summary>
    /// Gets an instance of the EAS contract for the specified chain.
    /// </summary>
    /// <param name="chainId">The chain ID</param>
    /// <returns>An instance of the EAS contract</returns>
    /// <exception cref="ArgumentException">Thrown when the chain ID is not supported</exception>
    public static EAS GetContract(string chainId)
    {
        return new EAS(Contracts.GetEASAddress(chainId));
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