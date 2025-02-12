using System;
using System.Numerics;
using System.Threading.Tasks;
using Evoq.Blockchain;
using Evoq.Ethereum;

namespace Evoq.Ethereum.EAS;

public record struct AttestationRequest(
    Hex Schema,
    AttestationRequestData Data
);

public record struct AttestationRequestData(
    EthereumAddress Recipient,          // The recipient of the attestation.
    UInt64 ExpirationUnixTimestamp,     // The expiration time of the attestation.
    bool Revocable,                     // Whether the attestation is revocable.
    Hex RefUID,                         // The UID of a related attestation.
    Hex Data,                           // Custom attestation data.
    BigInteger Value                    // An explicit Eth amount to send to the resolver.
);

public interface IAttest
{
    Task<Hex> AttestAsync(AttestationRequest request);
}
