using System;

namespace Evoq.Ethereum.EAS;

[Serializable]
public class EASException : Exception
{
    public EASException(string message) : base(message) { }

    public EASException(string message, Exception inner) : base(message, inner) { }

    //

    public string? TransactionHash { get; }
}