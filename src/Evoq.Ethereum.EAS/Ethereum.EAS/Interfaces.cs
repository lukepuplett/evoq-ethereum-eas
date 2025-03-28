using System;
using System.Threading.Tasks;
using Evoq.Blockchain;
using Evoq.Ethereum.JsonRPC;
using Evoq.Ethereum.Transactions;

namespace Evoq.Ethereum.EAS;

// main SchemaRegistry interfaces

/// <summary>
/// A request to register a schema.
/// </summary>
/// <param name="Schema">The schema to register.</param>
/// <param name="Resolver">The resolver address of the schema.</param>
/// <param name="Revocable">Whether the schema is revocable.</param>
public record struct SchemaRegistrationRequest(
    string Schema,
    EthereumAddress Resolver = default,
    bool Revocable = true
) : ISchemaDescription;

/// <summary>
/// An interface for registering a schema.
/// </summary>
public interface IRegisterSchema
{
    /// <summary>
    /// Register a new schema.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="request">The details of the schema to register.</param>
    /// <returns>The transaction result containing the UID of the schema.</returns>
    Task<TransactionResult<Hex>> RegisterAsync(InteractionContext context, ISchemaDescription request);
}

/// <summary>
/// A description of a schema.
/// </summary>
public interface ISchemaDescription
{
    /// <summary>
    /// The resolver address of the schema.
    /// </summary>
    EthereumAddress Resolver { get; }

    /// <summary>
    /// Whether the schema is revocable.
    /// </summary>
    bool Revocable { get; }

    /// <summary>
    /// The schema string.
    /// </summary>
    string Schema { get; }
}

/// <summary>
/// A schema record returned by the schema registry contract.
/// </summary>
public interface ISchemaRecord : ISchemaDescription
{
    /// <summary>
    /// The UID of the schema.
    /// </summary>
    Hex UID { get; }
}

/// <summary>
/// An interface for getting a schema from the schema registry.
/// </summary>
public interface IGetSchema
{
    /// <summary>
    /// Get a schema from the schema registry.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="schemaUID">The UID of the schema to get.</param>
    /// <returns>The schema record.</returns>
    Task<ISchemaRecord> GetSchemaAsync(InteractionContext context, Hex schemaUID);
}

// main attestation interfaces

/// <summary>
/// A request to attest to an EAS schema.
/// </summary>
/// <param name="SchemaUID">The UID of the schema to attest to.</param>
/// <param name="RequestData">The data to attest to.</param>
public record struct AttestationRequest(
    Hex SchemaUID,
    AttestationRequestData RequestData
);

/// <summary>
/// The data to attest to.
/// </summary>
/// <param name="Recipient">The recipient of the attestation.</param>
/// <param name="ExpirationTime">The expiration time of the attestation.</param>
/// <param name="Revocable">Whether the attestation is revocable.</param>
/// <param name="RefUID">The UID of a related attestation.</param>
/// <param name="Data">Custom attestation data.</param>
/// <param name="Value">An explicit Eth amount to send to the resolver.</param>
public record struct AttestationRequestData(
    EthereumAddress Recipient,          // The recipient of the attestation.
    DateTimeOffset ExpirationTime,      // The expiration time of the attestation.
    bool Revocable,                     // Whether the attestation is revocable.
    Hex RefUID,                         // The UID of a related attestation.
    Hex Data,                           // Custom attestation data.
    EtherAmount Value                   // An explicit Eth amount to send to the resolver.
);

/// <summary>
/// An interface for attesting to EAS schemas.
/// </summary>
public interface IAttest
{
    /// <summary>
    /// Attest a new attestation.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="request">The attestation data.</param>
    /// <returns>The transaction result containing the UID of the attestation.</returns>
    Task<TransactionResult<Hex>> AttestAsync(InteractionContext context, AttestationRequest request);
}

// main revocation interfaces

/// <summary>
/// A request to revoke an attestation.
/// </summary>
/// <param name="Schema">The UID of the schema to revoke from.</param>
/// <param name="Data">The revocation data.</param>
public record struct RevocationRequest(
    Hex Schema,
    RevocationRequestData Data
);

/// <summary>
/// The data for revoking an attestation.
/// </summary>
/// <param name="Uid">The UID of the attestation to revoke.</param>
/// <param name="Value">An explicit Eth amount to send to the resolver.</param>
public record struct RevocationRequestData(
    Hex Uid,
    EtherAmount Value
);

/// <summary>
/// An interface for revoking attestations.
/// </summary>
public interface IRevoke
{
    /// <summary>
    /// Revoke an attestation.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="request">The revocation data.</param>
    /// <returns>The transaction hash.</returns>
    Task<TransactionResult<Hex>> RevokeAsync(InteractionContext context, RevocationRequest request);
}

// timestamping interfaces

/// <summary>
/// An interface for timestamping data on-chain.
/// </summary>
public interface ITimestamp
{
    /// <summary>
    /// Timestamp a single piece of data on-chain.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="data">The data to timestamp.</param>
    /// <returns>The transaction result containing the timestamp.</returns>
    Task<TransactionResult<DateTimeOffset>> TimestampAsync(InteractionContext context, Hex data);

    /// <summary>
    /// Timestamp multiple pieces of data on-chain in a single transaction.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="data">The array of data to timestamp.</param>
    /// <returns>The transaction result containing the timestamp.</returns>
    Task<TransactionResult<DateTimeOffset>> MultiTimestampAsync(InteractionContext context, Hex[] data);

    /// <summary>
    /// Get the timestamp for a piece of data.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="data">The data to query.</param>
    /// <returns>The timestamp the data was timestamped with, or DateTimeOffset.MinValue if not timestamped.</returns>
    Task<DateTimeOffset> GetTimestampAsync(InteractionContext context, Hex data);
}

/// <summary>
/// An interface for timestamping off-chain revocation data.
/// </summary>
public interface IRevokeOffchain
{
    /// <summary>
    /// Timestamp a single piece of revocation data on-chain.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="data">The revocation data to timestamp.</param>
    /// <returns>The transaction result containing the timestamp.</returns>
    Task<TransactionResult<DateTimeOffset>> RevokeOffchainAsync(InteractionContext context, Hex data);

    /// <summary>
    /// Timestamp multiple pieces of revocation data on-chain in a single transaction.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="data">The array of revocation data to timestamp.</param>
    /// <returns>The transaction result containing the timestamp.</returns>
    Task<TransactionResult<DateTimeOffset>> MultiRevokeOffchainAsync(InteractionContext context, Hex[] data);

    /// <summary>
    /// Get the timestamp for a piece of revocation data.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="revoker">The address that revoked the data.</param>
    /// <param name="data">The revocation data to query.</param>
    /// <returns>The timestamp the data was revoked with, or DateTimeOffset.MinValue if not revoked.</returns>
    Task<DateTimeOffset> GetRevokeOffchainAsync(InteractionContext context, EthereumAddress revoker, Hex data);
}

// get attestation interfaces

/// <summary>
/// Interface for reading attestation data.
/// </summary>
public interface IAttestation
{
    /// <summary>
    /// The UID of the attestation.
    /// </summary>
    Hex UID { get; }

    /// <summary>
    /// The schema of the attestation.
    /// </summary>
    Hex Schema { get; }

    /// <summary>
    /// The time of the attestation.
    /// </summary>
    DateTimeOffset Time { get; }

    /// <summary>
    /// The expiration time of the attestation.
    /// </summary>
    DateTimeOffset ExpirationTime { get; }

    /// <summary>
    /// The revocation time of the attestation.
    /// </summary>
    DateTimeOffset RevocationTime { get; }

    /// <summary>
    /// The refUID of the attestation.
    /// </summary>
    Hex RefUID { get; }

    /// <summary>
    /// The recipient of the attestation.
    /// </summary>
    EthereumAddress Recipient { get; }

    /// <summary>
    /// The address that created the attestation.
    /// </summary>
    EthereumAddress Attester { get; }

    /// <summary>
    /// Whether the attestation can be revoked.
    /// </summary>
    bool Revocable { get; }

    /// <summary>
    /// The data of the attestation.
    /// </summary>
    byte[] Data { get; }
}

// main version interfaces

/// <summary>
/// Represents semantic version information.
/// </summary>
/// <param name="Major">The major version number.</param>
/// <param name="Minor">The minor version number.</param>
/// <param name="Patch">The patch version number.</param>
public record struct SemanticVersion(
    int Major,
    int Minor,
    int Patch
)
{
    /// <summary>
    /// Gets the full version string in the format "MAJOR.MINOR.PATCH".
    /// </summary>
    public string Version => $"{Major}.{Minor}.{Patch}";
}

/// <summary>
/// An interface for getting the semantic version of a contract.
/// </summary>
public interface IGetVersion
{
    /// <summary>
    /// Get the semantic version of the contract.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <returns>The semantic version information.</returns>
    Task<SemanticVersion> GetVersionAsync(InteractionContext context);
}
