using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Evoq.Ethereum.EAS;

/// <summary>
/// Encodes data using a schema string like "uint256 value, string name"
/// </summary>
public class SchemaEncoder
{
    /// <summary>
    /// Constructor for SchemaEncoder
    /// </summary>
    /// <param name="schema">The schema string with names e.g. "uint256 value, string name"</param>
    public SchemaEncoder(string schema)
    {
        this.Schema = schema;
    }

    //

    /// <summary>
    /// The schema string with names e.g. "uint256 value, string name"
    /// </summary>
    public string Schema { get; }

    //

    /// <summary>
    /// Encodes the given data using the schema, I don't think this is packed.
    /// </summary>
    /// <param name="data">The data object to encode</param>
    /// <returns>The encoded bytes</returns>
    public byte[] AbiEncode<TDto>(TDto data)
    {
        AssertSchema<TDto>();

        var encoder = new ParametersEncoder();
        var encodedData = encoder.EncodeParametersFromTypeAttributes(typeof(TDto), data);

        return encodedData;
    }

    /// <summary>
    /// Encodes the given data using the schema, I don't think this is packed.
    /// </summary>
    /// <param name="values">The values to encode, e.g. [38, "Dave"]</param>
    /// <returns>The encoded bytes</returns>
    public byte[] AbiEncode(object[] values)
    {
        var typeNamePairs = Schema.Split(',').Select(t => t.Trim()).ToList();
        var abiTypes = typeNamePairs
            .Select(extractType)
            .Select(type => ABIType.CreateABIType(type))
            .ToArray();

        if (abiTypes.Length != values.Length)
        {
            throw new SchemaException(
                $"The number of values {values.Length} does not match the number of types ({abiTypes.Length}) in the schema ({this.Schema})")
            {
                ExpectedSchema = this.Schema,
                ActualSchema = string.Join(", ", abiTypes.Select(t => t.Name))
            };
        }

        var encoder = new ParametersEncoder();
        var encodedData = encoder.EncodeAbiTypes(abiTypes, values);

        return encodedData;

        //

        string extractType(string typeNamePair)
        {
            if (typeNamePair.Contains(' '))
            {
                return typeNamePair.Split(' ')[0];
            }

            return typeNamePair;
        }
    }

    //

    private void AssertSchema<TDto>()
    {
        // convert the data to a schema string and compare it to the expected schema

        var schemaString = new StringBuilder();
        GetSchemaString(typeof(TDto), schemaString, includeNames: true);

        if (this.Schema != schemaString.ToString())
        {
            throw new SchemaException("Schema mismatch")
            {
                ExpectedSchema = this.Schema,
                ActualSchema = schemaString.ToString()
            };
        }
    }

    //

    /// <summary>
    /// Gets the schema string for a given type, including names if requested
    /// </summary>
    /// <remarks>
    /// The inclusion of names is useful for detecting strict schema mismatches. The syntax is
    /// taken from the ABI specification for function signatures. The "nameless" syntax is actually
    /// known as ABI "signature" syntax though it is not officially part of the ABI specification.
    ///
    /// The ABI specification does define a JSON format which includes the keyword "tuple" to denote
    /// nested complex types, but the format "uint256 myVal, tuple(uint256 x, uint256 y) coords" is
    /// not specified in the ABI specification.
    /// </remarks>
    /// <param name="type">The type to get the schema string for</param>
    /// <param name="schemaString">The schema string to append to</param>
    /// <param name="includeNames">Whether to include the property names in the schema string</param>
    /// <returns>The schema string, e.g. "uint256 value, string name"</returns>
    public static string GetSchemaString(Type type, StringBuilder schemaString, bool includeNames = false)
    {
        List<(PropertyInfo, ParameterAttribute)> pps = new();

        foreach (var property in type.GetProperties())
        {
            var paramAtts = property.GetCustomAttributes(typeof(ParameterAttribute), false);
            if (paramAtts.Length > 0)
            {
                pps.Add((property, (ParameterAttribute)paramAtts[0]));
            }
        }

        int count = pps.Count;
        foreach (var (property, paramAtt) in pps.OrderBy(p => p.Item2.Order))
        {
            // if it's a nested type, get the schema string for it
            // else just get the property name and type

            string annotatedTypeName = paramAtt.Type.ToLower().Replace("[]", "").Trim();
            string annotatedPropertyName = paramAtt.Name.Trim();

            if (annotatedTypeName.Equals("tuple", StringComparison.OrdinalIgnoreCase))
            {
                if (includeNames)
                {
                    schemaString.Append("tuple");
                }

                schemaString.Append("(");
                GetSchemaString(property.PropertyType, schemaString, includeNames);
                schemaString.Append(")");

                if (includeNames)
                {
                    schemaString.Append(" ");
                    schemaString.Append(annotatedPropertyName);
                }
            }
            else
            {
                schemaString.Append(annotatedTypeName);

                if (property.PropertyType.IsArray)
                {
                    schemaString.Append("[]");
                }

                if (includeNames)
                {
                    schemaString.Append(" ");
                    schemaString.Append(annotatedPropertyName);
                }
            }

            if (count-- > 1)
            {
                if (includeNames)
                {
                    schemaString.Append(", ");
                }
                else
                {
                    schemaString.Append(",");
                }
            }
        }

        return schemaString.ToString().Trim().TrimEnd(',');
    }
}
