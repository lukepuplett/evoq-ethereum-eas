using System;
using Evoq.Blockchain;
using Evoq.Ethereum.ABI;
using Evoq.Ethereum.ABI.Conversion;

namespace Evoq.Ethereum.EAS;

// main schema registry DTOs

/// <summary>
/// A DTO for a schema record.
/// </summary>
public class SchemaRecordDTO : ISchemaRecord
{
    /// <summary>
    /// The UID of the schema.
    /// </summary>
    [AbiParameter("uid", AbiType = AbiTypeNames.FixedByteArrays.Bytes32)]
    public Hex UID { get; set; } = Hex.Empty;

    /// <summary>
    /// The resolver address of the schema.
    /// </summary>
    [AbiParameter("resolver", AbiType = AbiTypeNames.Address)]
    public EthereumAddress Resolver { get; set; } = EthereumAddress.Zero;

    /// <summary>
    /// Whether the schema is revocable.
    /// </summary>
    [AbiParameter("revocable", AbiType = AbiTypeNames.Bool)]
    public bool Revocable { get; set; }

    /// <summary>
    /// The schema string.
    /// </summary>
    [AbiParameter("schema", AbiType = AbiTypeNames.String)]
    public string Schema { get; set; } = string.Empty;
}

/// <summary>
/// A DTO for the Registered event.
/// </summary>
public class RegisteredEventDTO // TODO / this needs testing in Evoq.Ethereum package
{
    /// <summary>
    /// The UID of the schema.
    /// </summary>
    [AbiParameter("uid", AbiType = AbiTypeNames.FixedByteArrays.Bytes32)]
    public Hex UID { get; set; } = Hex.Empty;

    /// <summary>
    /// The address of the registerer.
    /// </summary>
    [AbiParameter("registerer", AbiType = AbiTypeNames.Address)]
    public EthereumAddress Registerer { get; set; } = EthereumAddress.Zero;

    /// <summary>
    /// The schema record.
    /// </summary>
    [AbiParameter("schema")]
    public SchemaRecordDTO Schema { get; set; } = new();
}

// main attestation DTOs

/// <summary>
/// A DTO for an attestation.
/// </summary>
public class AttestationDTO : IAttestation
{
    /// <summary>
    /// The UID of the attestation.
    /// </summary>
    [AbiParameter("uid", AbiType = AbiTypeNames.FixedByteArrays.Bytes32)]
    public Hex UID { get; set; } = Hex.Empty;

    /// <summary>
    /// The schema of the attestation.
    /// </summary>
    [AbiParameter("schema", AbiType = AbiTypeNames.FixedByteArrays.Bytes32)]
    public Hex Schema { get; set; } = Hex.Empty;

    /// <summary>
    /// The time of the attestation.
    /// </summary>
    [AbiParameter("time", AbiType = AbiTypeNames.IntegerTypes.Uint64)]
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// The expiration time of the attestation.
    /// </summary>
    [AbiParameter("expirationTime", AbiType = AbiTypeNames.IntegerTypes.Uint64)]
    public DateTimeOffset ExpirationTime { get; set; }

    /// <summary>
    /// The revocation time of the attestation.
    /// </summary>
    [AbiParameter("revocationTime", AbiType = AbiTypeNames.IntegerTypes.Uint64)]
    public DateTimeOffset RevocationTime { get; set; }

    /// <summary>
    /// The refUID of the attestation.
    /// </summary>
    [AbiParameter("refUID", AbiType = AbiTypeNames.FixedByteArrays.Bytes32)]
    public Hex RefUID { get; set; } = Hex.Empty;

    /// <summary>
    /// The recipient of the attestation.
    /// </summary>
    [AbiParameter("recipient", AbiType = AbiTypeNames.Address)]
    public EthereumAddress Recipient { get; set; } = EthereumAddress.Empty;

    /// <summary>
    /// The address that created the attestation.
    /// </summary>
    [AbiParameter("attester", AbiType = AbiTypeNames.Address)]
    public EthereumAddress Attester { get; set; } = EthereumAddress.Empty;

    /// <summary>
    /// Whether the attestation can be revoked.
    /// </summary>
    [AbiParameter("revocable", AbiType = AbiTypeNames.Bool)]
    public bool Revocable { get; set; }

    /// <summary>
    /// The data of the attestation.
    /// </summary>
    [AbiParameter("data", AbiType = AbiTypeNames.Bytes)]
    public byte[] Data { get; set; } = Array.Empty<byte>();
}