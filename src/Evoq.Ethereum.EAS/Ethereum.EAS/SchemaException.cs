using System;
using System.Runtime.Serialization;

namespace Evoq.Ethereum.EAS;

[Serializable]
public class SchemaException : Exception
{
    public SchemaException()
    {
    }

    public SchemaException(string? message) : base(message)
    {
    }

    public SchemaException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SchemaException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    //

    public string? ExpectedSchema { get; init; }
    public string? ActualSchema { get; init; }
}