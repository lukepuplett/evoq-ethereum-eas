using System;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Evoq.Ethereum.EAS;

[FunctionOutput]
class VoteDTO
{
    [Parameter("uint256", "eventId", 1)]
    public BigInteger EventId { get; set; }

    [Parameter("uint8", "voteIndex", 2)]
    public byte VoteIndex { get; set; }

    [Parameter("tuple", "details", 3)]
    public NestedDetailsDTO Details { get; set; } = new NestedDetailsDTO();
}

class NestedDetailsDTO
{
    [Parameter("uint8", "voteIndex", 1)]
    public byte VoteIndex { get; set; }

    [Parameter("bool", "isValid", 2)]
    public bool IsValid { get; set; }
}

class PocoWithArrayDTO
{
    [Parameter("uint32[]", "ages", 1)]
    public uint[] Ages { get; set; } = Array.Empty<uint>();
}

class ComplexTypeDTO
{
    [Parameter("uint256", "bigNumber", 1)]
    public BigInteger BigNumber { get; set; }

    [Parameter("uint64", "mediumNumber", 2)]
    public ulong MediumNumber { get; set; }

    [Parameter("uint32", "smallNumber", 3)]
    public uint SmallNumber { get; set; }

    [Parameter("uint8", "tinyNumber", 4)]
    public byte TinyNumber { get; set; }

    [Parameter("bool", "flag1", 5)]
    public bool Flag1 { get; set; }

    [Parameter("bool", "flag2", 6)]
    public bool Flag2 { get; set; }

    [Parameter("string", "name", 7)]
    public string Name { get; set; } = string.Empty;
}


