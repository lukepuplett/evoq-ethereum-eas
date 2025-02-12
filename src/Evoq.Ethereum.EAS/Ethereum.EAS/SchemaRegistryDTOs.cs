using System;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Evoq.Ethereum.EAS;

public class SchemaRecordDTO : ISchemaRecord
{
    [Parameter("bytes32", "uid", 1)]
    public byte[] UID { get; set; } = Array.Empty<byte>();

    [Parameter("address", "resolver", 2)]
    public string Resolver { get; set; } = string.Empty;

    [Parameter("bool", "revocable", 3)]
    public bool Revocable { get; set; }

    [Parameter("string", "schema", 4)]
    public string Schema { get; set; } = string.Empty;
}

[Event("Registered")]
public class RegisteredEventDTO : IEventDTO
{
    [Parameter("bytes32", "uid", 1, true)]
    public byte[] UID { get; set; } = Array.Empty<byte>();

    [Parameter("address", "registerer", 2, true)]
    public string Registerer { get; set; } = string.Empty;

    [Parameter("tuple", "schema", 3, false)]
    public SchemaRecordDTO Schema { get; set; } = new();
}