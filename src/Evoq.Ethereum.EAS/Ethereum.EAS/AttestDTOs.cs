using System;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Evoq.Ethereum.EAS;

[Struct("AttestationRequestData")]
public class AttestationRequestDataDTO
{
    [Parameter("address", "recipient", 1)]
    public string Recipient { get; set; } = string.Empty;

    [Parameter("uint64", "expirationTime", 2)]
    public ulong ExpirationTime { get; set; } = 0;

    [Parameter("bool", "revocable", 3)]
    public bool Revocable { get; set; } = false;

    [Parameter("bytes32", "refUID", 4)]
    public byte[] RefUID { get; set; } = Array.Empty<byte>();

    [Parameter("bytes", "data", 5)]
    public byte[] Data { get; set; } = Array.Empty<byte>();

    [Parameter("uint256", "value", 6)]
    public BigInteger Value { get; set; } = 0;
}

[Struct("AttestationRequest")]
public class AttestationRequestDTO
{
    [Parameter("bytes32", "schema", 1)]
    public byte[] Schema { get; set; } = Array.Empty<byte>();

    [Parameter("tuple", "data", 2)]
    public AttestationRequestDataDTO Data { get; set; } = new();
}

[Event("Attested")]
public class AttestedEventDTO : IEventDTO
{
    // The 'indexed' keyword in Solidity means this parameter can be used for filtering logs
    [Parameter("address", "recipient", 1, true)]
    public string Recipient { get; set; } = string.Empty;

    [Parameter("address", "attester", 2, true)]
    public string Attester { get; set; } = string.Empty;

    [Parameter("bytes32", "uid", 3, false)]
    public byte[] Uid { get; set; } = Array.Empty<byte>();

    [Parameter("bytes32", "schemaUID", 4, true)]
    public byte[] SchemaUID { get; set; } = Array.Empty<byte>();
}