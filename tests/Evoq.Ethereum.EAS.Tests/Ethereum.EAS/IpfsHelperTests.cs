using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evoq.Ethereum.EAS
{
    [TestClass]
    public class IpfsHelperTests
    {
        // Known valid CID pairs from IPFS documentation
        private const string TestCIDv0 = "QmdfTbBqBPQ7VNxZEYEj14VmRuZBkqFbiwReogJgS1zR1n";
        private const string TestCIDv1 = "bafybeihdwdcefgh4dqkjv67uzcmw7ojee6xedzdetojuzjevtenxquvyku";
        private const string AltTestCIDv0 = "QmbWqxBEKC3P8tqsKc98xmWNzrzDtRLMiMPL8wBuTGsMnR";
        private const string AltTestCIDv1 = "bafybeigdyrzt5sfp7udm7hu76uh7y26nf3efuylqabf3oclgtqy55fbzdi";

        [TestMethod]
        public void IsCID_ValidCIDs_ReturnsTrue()
        {
            // These CIDs are known valid v0/v1 pairs from the IPFS documentation
            Assert.IsTrue(IpfsHelper.IsCID(TestCIDv0), "Should accept valid v0 CID");
            Assert.IsTrue(IpfsHelper.IsCID(TestCIDv1), "Should accept valid v1 CID");
            Assert.IsTrue(IpfsHelper.IsCID(AltTestCIDv0), "Should accept alternative valid v0 CID");
            Assert.IsTrue(IpfsHelper.IsCID(AltTestCIDv1), "Should accept alternative valid v1 CID");
        }

        [TestMethod]
        [DataRow("")]  // Empty
        [DataRow("Qm")] // Too short for v0
        [DataRow("QmInvalidCharacters!@#")] // Invalid chars
        [DataRow("bafy")] // Incomplete v1
        [DataRow("bafybeig")] // Incomplete v1 prefix
        [DataRow("f01701220c3c4733ec8affd06cf9e9ff50ffc6bcd2ec85a6170004bb709669c31de94391a")] // Raw hex CID
        public void IsCID_InvalidCIDs_ReturnsFalse(string cid)
        {
            Assert.IsFalse(IpfsHelper.IsCID(cid));
        }

        [TestMethod]
        public void EncodeQmHash_DecodeQmHash_RoundTrip()
        {
            // Using v0 CID for round trip as our DecodeQmHash specifically creates v0 CIDs
            var originalCid = TestCIDv0;

            // Act
            var encoded = IpfsHelper.EncodeQmHash(originalCid);
            var decoded = IpfsHelper.DecodeQmHash(encoded);

            // Assert
            Assert.AreEqual(originalCid, decoded);
        }

        [TestMethod]
        public void EncodeQmHash_WithV1CID_ThrowsArgumentException()
        {
            // Arrange
            var v1Cid = TestCIDv1;

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                IpfsHelper.EncodeQmHash(v1Cid),
                "Should reject V1 CIDs");
        }

        [TestMethod]
        public void EncodeQmHash_WithHexCID_ThrowsArgumentException()
        {
            // Arrange
            var hexCid = "f01701220c3c4733ec8affd06cf9e9ff50ffc6bcd2ec85a6170004bb709669c31de94391a";

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                IpfsHelper.EncodeQmHash(hexCid),
                "Should reject hex format CIDs");
        }

        [TestMethod]
        public void DecodeQmHash_InvalidHexString_ThrowsArgumentException()
        {
            // Arrange
            var invalidHex = "0xInvalidHex";

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                IpfsHelper.DecodeQmHash(invalidHex));
        }

        [TestMethod]
        public void DecodeQmHash_MissingPrefix_ThrowsArgumentException()
        {
            // Arrange
            var missingPrefix = "1234567890abcdef";

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                IpfsHelper.DecodeQmHash(missingPrefix));
        }

        [TestMethod]
        public void EncodeQmHash_InvalidCID_ThrowsArgumentException()
        {
            // Arrange
            var invalidCid = "NotACid";

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                IpfsHelper.EncodeQmHash(invalidCid));
        }

        // Add more tests...
    }
}