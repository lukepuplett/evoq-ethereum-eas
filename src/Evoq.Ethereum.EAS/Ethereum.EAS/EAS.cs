using System;
using System.Numerics;
using System.Threading.Tasks;
using Evoq.Blockchain;

namespace Evoq.Ethereum.EAS;

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
/// <param name="ExpirationUnixTimestamp">The expiration time of the attestation.</param>
/// <param name="Revocable">Whether the attestation is revocable.</param>
/// <param name="RefUID">The UID of a related attestation.</param>
/// <param name="Data">Custom attestation data.</param>
/// <param name="Value">An explicit Eth amount to send to the resolver.</param>
public record struct AttestationRequestData(
    EthereumAddress Recipient,          // The recipient of the attestation.
    UInt64 ExpirationUnixTimestamp,     // The expiration time of the attestation.
    bool Revocable,                     // Whether the attestation is revocable.
    Hex RefUID,                         // The UID of a related attestation.
    Hex Data,                           // Custom attestation data.
    BigInteger Value                    // An explicit Eth amount to send to the resolver.
);

/// <summary>
/// An interface for attesting to EAS schemas.
/// </summary>
public interface IAttest
{
    /// <summary>
    /// Attest a new attestation.
    /// </summary>
    /// <param name="request">The attestation data.</param>
    /// <returns>The UID of the attestation.</returns>
    Task<Hex> AttestAsync(AttestationRequest request);
}
