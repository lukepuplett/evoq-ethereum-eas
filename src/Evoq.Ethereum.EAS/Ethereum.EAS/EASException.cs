using System;
using Evoq.Blockchain;

namespace Evoq.Ethereum.EAS;

/// <summary>
/// An exception that occurs when interacting with the EAS.
/// </summary>
[Serializable]
public class EASException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EASException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public EASException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EASException"/> class.
    /// </summary>
    public EASException(string message, Exception inner) : base(message, inner) { }

    //

    /// <summary>
    /// Gets the transaction hash.
    /// </summary>
    public Hex TransactionHash { get; init; } = Hex.Empty;
}