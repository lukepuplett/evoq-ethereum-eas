using System;
using Evoq.Blockchain;
using Evoq.Ethereum.ABI;
using Evoq.Ethereum.Crypto;

namespace Evoq.Ethereum.EAS;

/// <summary>
/// A class for managing schema UID.
/// </summary>
public static class SchemaUID
{
    /// <summary>
    /// Gets the schema UID for a given schema.
    /// </summary>
    /// <param name="schema">The schema registration.</param>
    /// <returns>The schema UID</returns>
    public static Hex FormatSchemaUID(ISchemaDescription schema)
    {
        return FormatSchemaUID(schema.Schema, schema.Resolver, schema.Revocable);
    }

    /// <summary>
    /// Gets the schema UID for a given schema.
    /// </summary>
    /// <param name="schema">The schema string (e.g. "uint256 value, string name")</param>
    /// <param name="resolver">The resolver address</param>
    /// <param name="revocable">Whether the schema is revocable</param>
    /// <returns>The schema UID</returns>
    public static Hex FormatSchemaUID(string schema, EthereumAddress resolver, bool revocable)
    {
        if (string.IsNullOrEmpty(schema))
        {
            throw new ArgumentException("Schema cannot be null or empty", nameof(schema));
        }

        schema = NormalizeSchema(schema, false);

        var parameters = AbiParameters.Parse("(string schema, address resolver, bool revocable)");
        var values = AbiKeyValues.Create(
            ("schema", schema),
            ("resolver", resolver),
            ("revocable", revocable)
        );

        var packer = new AbiEncoderPacked();
        var packedBytes = packer.EncodeParameters(parameters, values).ToByteArray();
        var uid = KeccakHash.ComputeHash(packedBytes);

        return new Hex(uid);
    }

    internal static string NormalizeSchema(string schema, bool withBrackets = false)
    {
        schema = schema.Trim();

        if (schema.StartsWith("(") && schema.EndsWith(")"))
        {
            schema = schema[1..^1];
        }

        if (withBrackets)
        {
            schema = $"({schema})";
        }

        return schema;
    }
}