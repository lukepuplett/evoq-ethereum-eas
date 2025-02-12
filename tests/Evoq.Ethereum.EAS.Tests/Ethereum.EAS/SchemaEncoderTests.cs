using System;
using System.Numerics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Evoq.Ethereum.EAS;

[TestClass]
public class SchemaEncoderTests
{
    [TestMethod]
    public void GetSchemaString_Vote_WithoutNames_ReturnsCorrectSchema()
    {
        // Arrange
        var sb = new StringBuilder();

        // Act
        var result = SchemaEncoder.GetSchemaString(typeof(VoteDTO), sb, includeNames: false);

        // Assert
        Assert.AreEqual("uint256,uint8,(uint8,bool)", result);
    }

    [TestMethod]
    public void GetSchemaString_Vote_WithNames_ReturnsCorrectSchema()
    {
        // Arrange
        var sb = new StringBuilder();

        // Act
        var result = SchemaEncoder.GetSchemaString(typeof(VoteDTO), sb, includeNames: true);

        // Assert
        Assert.AreEqual("uint256 eventId, uint8 voteIndex, tuple(uint8 voteIndex, bool isValid) details", result);
    }

    [TestMethod]
    public void GetSchemaString_NestedDetails_WithoutNames_ReturnsCorrectSchema()
    {
        // Arrange
        var sb = new StringBuilder();

        // Act
        var result = SchemaEncoder.GetSchemaString(typeof(NestedDetailsDTO), sb, includeNames: false);

        // Assert
        Assert.AreEqual("uint8,bool", result);
    }

    [TestMethod]
    public void GetSchemaString_NestedDetails_WithNames_ReturnsCorrectSchema()
    {
        // Arrange
        var sb = new StringBuilder();

        // Act
        var result = SchemaEncoder.GetSchemaString(typeof(NestedDetailsDTO), sb, includeNames: true);

        // Assert
        Assert.AreEqual("uint8 voteIndex, bool isValid", result);
    }

    [TestMethod]
    public void EncodeVote_WithCorrectSchema_ShouldNotThrow()
    {
        // Arrange
        var schema = "uint256 eventId, uint8 voteIndex, tuple(uint8 voteIndex, bool isValid) details";
        var encoder = new SchemaEncoder(schema);
        var vote = new VoteDTO
        {
            EventId = new BigInteger(123456789),
            VoteIndex = 1,
            Details = new NestedDetailsDTO
            {
                VoteIndex = 2,
                IsValid = true
            }
        };

        // Act & Assert
        encoder.AbiEncode(vote); // Should not throw
    }

    [TestMethod]
    public void EncodeNestedDetails_WithCorrectSchema_ShouldNotThrow()
    {
        // Arrange
        var schema = "uint8 voteIndex, bool isValid";
        var encoder = new SchemaEncoder(schema);
        var details = new NestedDetailsDTO
        {
            VoteIndex = 1,
            IsValid = true
        };

        // Act & Assert
        encoder.AbiEncode(details); // Should not throw
    }

    [TestMethod]
    [ExpectedException(typeof(SchemaException))]
    public void Encode_WithIncorrectSchema_ShouldThrow()
    {
        // Arrange
        var incorrectSchema = "uint256 eventId, uint256 voteIndex";
        var encoder = new SchemaEncoder(incorrectSchema);
        var vote = new VoteDTO
        {
            EventId = new BigInteger(123),
            VoteIndex = 1,
            Details = new NestedDetailsDTO
            {
                VoteIndex = 2,
                IsValid = true
            }
        };

        // Act
        encoder.AbiEncode(vote); // Should throw ArgumentException
    }

    [TestMethod]
    public void Encode_Vote_WithMatchingSchema_ShouldNotThrow()
    {
        // Arrange
        var schema = "uint256 eventId, uint8 voteIndex, tuple(uint8 voteIndex, bool isValid) details";
        var encoder = new SchemaEncoder(schema);
        var vote = new VoteDTO
        {
            EventId = new BigInteger(123456789),
            VoteIndex = 1,
            Details = new NestedDetailsDTO
            {
                VoteIndex = 2,
                IsValid = true
            }
        };

        // Act
        var result = encoder.AbiEncode(vote);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Encode_NestedDetails_WithMatchingSchema_ShouldNotThrow()
    {
        // Arrange
        var schema = "uint8 voteIndex, bool isValid";
        var encoder = new SchemaEncoder(schema);
        var details = new NestedDetailsDTO
        {
            VoteIndex = 1,
            IsValid = true
        };

        // Act
        var result = encoder.AbiEncode(details);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    [ExpectedException(typeof(SchemaException))]
    public void Encode_Vote_WithMismatchedSchema_ShouldThrowSchemaException()
    {
        // Arrange
        var incorrectSchema = "uint256 eventId, uint256 voteIndex, tuple(uint8 voteIndex, bool isValid) details"; // wrong type for voteIndex
        var encoder = new SchemaEncoder(incorrectSchema);
        var vote = new VoteDTO
        {
            EventId = new BigInteger(123456789),
            VoteIndex = 1,
            Details = new NestedDetailsDTO
            {
                VoteIndex = 2,
                IsValid = true
            }
        };

        // Act
        encoder.AbiEncode(vote); // Should throw SchemaException
    }

    [TestMethod]
    public void Encode_Vote_SchemaException_ContainsExpectedAndActualSchema()
    {
        // Arrange
        var incorrectSchema = "uint256 eventId, uint256 voteIndex, tuple(uint8 voteIndex, bool isValid) details";
        var encoder = new SchemaEncoder(incorrectSchema);
        var vote = new VoteDTO
        {
            EventId = new BigInteger(123),
            VoteIndex = 1,
            Details = new NestedDetailsDTO
            {
                VoteIndex = 2,
                IsValid = true
            }
        };

        try
        {
            // Act
            encoder.AbiEncode(vote);
            Assert.Fail("Expected SchemaException was not thrown");
        }
        catch (SchemaException ex)
        {
            // Assert
            Assert.AreEqual(incorrectSchema, ex.ExpectedSchema);
            Assert.AreEqual("uint256 eventId, uint8 voteIndex, tuple(uint8 voteIndex, bool isValid) details", ex.ActualSchema);
        }
    }

    [TestMethod]
    public void Encode_SingleBool_MatchesJsImplementation()
    {
        // Arrange
        var schema = "bool isValid";
        var encoder = new SchemaEncoder(schema);
        var data = new BoolTest { IsValid = true };

        // Act
        var result = encoder.AbiEncode(data);

        // Assert
        var expected = "0x0000000000000000000000000000000000000000000000000000000000000001";
        Assert.AreEqual(32, result.Length); // Byte Length: 32
        Assert.AreEqual(expected[2..], BitConverter.ToString(result).Replace("-", "").ToLower());
    }

    [TestMethod]
    public void Encode_Uint256AndUint8_MatchesJsImplementation()
    {
        // Arrange
        var schema = "uint256 eventId, uint8 voteIndex";
        var encoder = new SchemaEncoder(schema);
        var data = new EventVote
        {
            EventId = new BigInteger(123456789),
            VoteIndex = 1
        };

        // Act
        var result = encoder.AbiEncode(data);

        // Assert
        var expected = "0x00000000000000000000000000000000000000000000000000000000075bcd150000000000000000000000000000000000000000000000000000000000000001";
        Assert.AreEqual(64, result.Length); // Byte Length: 64
        Assert.AreEqual(expected[2..], BitConverter.ToString(result).Replace("-", "").ToLower());
    }

    [TestMethod]
    public void Encode_Uint256BoolUint8_MatchesJsImplementation()
    {
        // Arrange
        var schema = "uint256 eventId, bool isValid, uint8 voteIndex";
        var encoder = new SchemaEncoder(schema);
        var data = new EventVoteWithValid
        {
            EventId = new BigInteger(123456789),
            IsValid = true,
            VoteIndex = 1
        };

        // Act
        var result = encoder.AbiEncode(data);

        // Assert
        var expected = "0x00000000000000000000000000000000000000000000000000000000075bcd1500000000000000000000000000000000000000000000000000000000000000010000000000000000000000000000000000000000000000000000000000000001";
        Assert.AreEqual(96, result.Length); // Byte Length: 96
        Assert.AreEqual(expected[2..], BitConverter.ToString(result).Replace("-", "").ToLower());
    }

    [TestMethod]
    public void Encode_ComplexType_MatchesJsImplementation()
    {
        // Arrange
        var schema = "uint256 bigNumber, uint64 mediumNumber, uint32 smallNumber, uint8 tinyNumber, bool flag1, bool flag2, string name";
        var encoder = new SchemaEncoder(schema);
        var data = new ComplexTypeDTO
        {
            BigNumber = BigInteger.Parse("123456789123456789"),
            MediumNumber = 123456789,
            SmallNumber = 12345,
            TinyNumber = 255,
            Flag1 = true,
            Flag2 = false,
            Name = "Alice"
        };

        // Act
        var result = encoder.AbiEncode(data);

        // Assert
        var expected = "0x00000000000000000000000000000000000000000000000001b69b4bacd05f1500000000000000000000000000000000000000000000000000000000075bcd15000000000000000000000000000000000000000000000000000000000000303900000000000000000000000000000000000000000000000000000000000000ff0000000000000000000000000000000000000000000000000000000000000001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000e00000000000000000000000000000000000000000000000000000000000000005416c696365000000000000000000000000000000000000000000000000000000";
        Assert.AreEqual(288, result.Length); // Byte Length: 288
        Assert.AreEqual(expected[2..], BitConverter.ToString(result).Replace("-", "").ToLower());

        // Optional: Also verify Base64 if needed
        var base64Expected = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAbabS6zQXxUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB1vNFQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADA5AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAOAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABUFsaWNlAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        var base64Actual = Convert.ToBase64String(result);
        Assert.AreEqual(base64Expected, base64Actual);
    }

    [TestMethod]
    public void GetSchemaString_PocoWithArrayDTO_WithoutNames_ReturnsCorrectSchema()
    {
        // Arrange
        var sb = new StringBuilder();

        // Act
        var result = SchemaEncoder.GetSchemaString(typeof(PocoWithArrayDTO), sb, includeNames: false);

        // Assert
        Assert.AreEqual("uint32[]", result);
    }

    [TestMethod]
    public void GetSchemaString_PocoWithArrayDTO_WithNames_ReturnsCorrectSchema()
    {
        // Arrange
        var sb = new StringBuilder();

        // Act
        var result = SchemaEncoder.GetSchemaString(typeof(PocoWithArrayDTO), sb, includeNames: true);

        // Assert
        Assert.AreEqual("uint32[] ages", result);
    }

    [TestMethod]
    public void Encode_Uint32Array_MatchesJsImplementation()
    {
        // Arrange
        var schema = "uint32[] ages";
        var encoder = new SchemaEncoder(schema);
        var data = new PocoWithArrayDTO
        {
            Ages = new uint[] { 25, 30, 35 }
        };

        // Act
        var result = encoder.AbiEncode(data);

        // Assert
        var expected = "0x000000000000000000000000000000000000000000000000000000000000002000000000000000000000000000000000000000000000000000000000000000030000000000000000000000000000000000000000000000000000000000000019000000000000000000000000000000000000000000000000000000000000001e0000000000000000000000000000000000000000000000000000000000000023";
        Assert.AreEqual(160, result.Length); // Byte Length: 160
        Assert.AreEqual(expected[2..], BitConverter.ToString(result).Replace("-", "").ToLower());

        // Optional: Also verify Base64
        var base64Expected = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB4AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIw==";
        var base64Actual = Convert.ToBase64String(result);
        Assert.AreEqual(base64Expected, base64Actual);
    }
}

// Test classes
public class BoolTest
{
    [Parameter("bool", "isValid", 1)]
    public bool IsValid { get; set; }
}

public class EventVote
{
    [Parameter("uint256", "eventId", 1)]
    public BigInteger EventId { get; set; }

    [Parameter("uint8", "voteIndex", 2)]
    public byte VoteIndex { get; set; }
}

public class EventVoteWithValid
{
    [Parameter("uint256", "eventId", 1)]
    public BigInteger EventId { get; set; }

    [Parameter("bool", "isValid", 2)]
    public bool IsValid { get; set; }

    [Parameter("uint8", "voteIndex", 3)]
    public byte VoteIndex { get; set; }
}