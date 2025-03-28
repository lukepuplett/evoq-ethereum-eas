using System.Diagnostics;
using Evoq.Blockchain;
using Evoq.Ethereum.ABI;
using Evoq.Ethereum.Crypto;

namespace Evoq.Ethereum.EAS;

[TestClass]
public class SchemaUIDTests
{
    [TestMethod]
    public void EncodePacked_SchemaUID_MatchesContract()
    {
        var encoder = new AbiEncoderPacked();
        var schema = "uint256 value, string name638786021703006440";
        var resolver = EthereumAddress.Zero;
        var revocable = true;

        var parameters = AbiParameters.Parse("(string schema, address resolver, bool revocable)");
        var values = AbiKeyValues.Create(
            ("schema", schema),
            ("resolver", resolver),
            ("revocable", revocable)
        );

        var packedBytes = encoder.EncodeParameters(parameters, values).ToByteArray();
        var uid = KeccakHash.ComputeHash(packedBytes);
        var uidHex = new Hex(uid);

        var expectedUID = "0xeaed8689d3f36f335df81863a21b1c8841cc5c49f97009b347e2a2afbd9f52a1";
        Assert.AreEqual(expectedUID, uidHex.ToString());
    }

    [TestMethod]
    public void FormatSchemaUID_WithBracketsAndWhitespace_MatchesContract()
    {
        var schema = "  (uint256 value, string name638786021703006440)  ";
        var resolver = EthereumAddress.Zero;
        var revocable = true;

        var uid = SchemaUID.FormatSchemaUID(schema, resolver, revocable);

        var expectedUID = "0xeaed8689d3f36f335df81863a21b1c8841cc5c49f97009b347e2a2afbd9f52a1";
        Assert.AreEqual(expectedUID, uid.ToString());
    }
}