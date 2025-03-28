using System;
using System.Threading.Tasks;
using Evoq.Blockchain;
using Evoq.Ethereum.ABI;
using Evoq.Ethereum.JsonRPC;
using Evoq.Ethereum.Transactions;

namespace Evoq.Ethereum.EAS;

/// <summary>
/// Extension methods for the EAS.
/// </summary>
public static class EASExtensions
{
    /// <summary>
    /// Register a new schema.
    /// </summary>
    /// <param name="eas">The EAS instance.</param>
    /// <param name="context">The interaction context.</param>
    /// <param name="schema">The schema to register.</param>
    /// <param name="resolver">The resolver address of the schema.</param>
    /// <param name="revocable">Whether the schema is revocable.</param>
    /// <returns>The transaction result containing the UID of the schema.</returns>
    public static async Task<TransactionResult<Hex>> RegisterAsync(
        this IRegisterSchema eas,
        InteractionContext context,
        string schema,
        EthereumAddress resolver = default,
        bool revocable = true)
    {
        var request = new SchemaRegistrationRequest(schema, resolver, revocable);

        return await eas.RegisterAsync(context, request);
    }

    /// <summary>
    /// Attest a new attestation.
    /// </summary>
    /// <param name="eas">The EAS instance.</param>
    /// <param name="context">The interaction context.</param>
    /// <param name="schema">The schema to attest.</param>
    /// <param name="recipient">The recipient of the attestation.</param>
    /// <param name="data">The data to attest.</param>
    /// <param name="revocable">Whether the attestation is revocable.</param>
    /// <param name="expirationTime">The expiration time of the attestation.</param>
    /// <param name="refUID">The UID of a related attestation.</param>
    /// <param name="value">The value of the attestation.</param>
    /// <returns>The transaction result containing the UID of the attestation.</returns>
    public static async Task<TransactionResult<Hex>> AttestAsync(
        this IAttest eas,
        InteractionContext context,
        SchemaRegistrationRequest schema,
        EthereumAddress recipient,
        AbiKeyValues data,
        bool revocable = true,
        DateTimeOffset? expirationTime = null,
        Hex refUID = default,
        EtherAmount value = default)
    {
        var schemaUID = SchemaUID.FormatSchemaUID(schema);

        var parameters = AbiParameters.Parse(schema.Schema);
        var encoder = new AbiEncoder(context.Endpoint.LoggerFactory);
        var encoded = encoder.EncodeParameters(parameters, data);

        refUID = refUID == default ? Hex.Zero : refUID;
        value = value == default ? EtherAmount.Zero : value;

        var requestData = new AttestationRequestData(
            recipient,
            expirationTime ?? DateTimeOffset.MaxValue,
            revocable,
            refUID,
            encoded.ToHexStruct(),
            value);

        var request = new AttestationRequest(schemaUID, requestData);

        return await eas.AttestAsync(context, request);
    }
}
