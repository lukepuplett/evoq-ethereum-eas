using System;
using System.Threading.Tasks;
using Evoq.Blockchain;
using Evoq.Ethereum.JsonRPC;
using Evoq.Ethereum.Transactions;

namespace Evoq.Ethereum.EAS;

// main SchemaRegistry interfaces

/// <summary>
/// A schema record returned by the schema registry contract.
/// </summary>
public interface ISchemaRecord
{
    /// <summary>
    /// The UID of the schema.
    /// </summary>
    Hex UID { get; }

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
/// <param name="Schema">The UID of the schema to attest to.</param>
/// <param name="Data">The data to attest to.</param>
public record struct AttestationRequest(
    Hex Schema,
    AttestationRequestData Data
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
    EtherAmount Value                    // An explicit Eth amount to send to the resolver.
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
