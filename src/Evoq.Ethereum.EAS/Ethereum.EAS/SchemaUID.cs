using System;
using Evoq.Blockchain;
using Evoq.Ethereum.ABI;
using Evoq.Ethereum.Crypto;

namespace Evoq.Ethereum.EAS;

/// <summary>
/// A class for managing schema UID.
/// </summary>
public static class SchemaUID
{    /// <summary>
     /// Gets the schema UID for a given schema.
     /// </summary>
     /// <param name="schema">The schema string (e.g. "uint256 value, string name")</param>
     /// <param name="revocable">Whether the schema is revocable</param>
     /// <returns>The schema UID</returns>
    public static Hex FormatSchemaUID(string schema, bool revocable) =>
        FormatSchemaUID(schema, revocable, EthereumAddress.Zero);

    /// <summary>
    /// Gets the schema UID for a given schema.
    /// </summary>
    /// <param name="schema">The schema string (e.g. "uint256 value, string name")</param>
    /// <param name="revocable">Whether the schema is revocable</param>
    /// <param name="resolver">The resolver address</param>
    /// <returns>The schema UID</returns>
    public static Hex FormatSchemaUID(string schema, bool revocable, EthereumAddress resolver)
    {
        if (string.IsNullOrEmpty(schema))
        {
            throw new ArgumentException("Schema cannot be null or empty", nameof(schema));
        }

        schema = schema.Trim();
        if (!schema.StartsWith("(") || !schema.EndsWith(")"))
        {
            schema = $"({schema})";
        }

        var parameters = AbiParameters.Parse("(string schema, bool revocable, address resolver)");
        var values = AbiKeyValues.Create(("schema", schema), ("revocable", revocable), ("resolver", resolver));

        var packer = new AbiEncoderPacked();
        var packedBytes = packer.EncodeParameters(parameters, values).ToByteArray();
        var uid = KeccakHash.ComputeHash(packedBytes);

        return new Hex(uid);
    }
}