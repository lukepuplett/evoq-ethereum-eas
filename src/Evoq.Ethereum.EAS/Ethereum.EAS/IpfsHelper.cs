using System;
using System.Text.RegularExpressions;
using SimpleBase;

namespace Evoq.Ethereum.EAS
{
    public static class IpfsHelper
    {
        private static readonly Regex V0Pattern = new(@"^Qm[1-9A-HJ-NP-Za-km-z]{44}$", RegexOptions.Compiled);
        private static readonly Regex V1Pattern = new(@"^baf[a-z2-7]{4}[a-z2-7]{52}$", RegexOptions.Compiled);

        public static bool IsCID(string cid)
        {
            if (string.IsNullOrEmpty(cid))
                return false;

            return V0Pattern.IsMatch(cid) || V1Pattern.IsMatch(cid);
        }

        public static string EncodeQmHash(string hash)
        {
            if (!IsCID(hash))
                throw new ArgumentException("Invalid CID format", nameof(hash));

            // For V0 CIDs, we need to extract the multihash portion (everything after "Qm")
            var multihash = hash.StartsWith("Qm") ? hash[2..] : throw new ArgumentException("Only V0 CIDs are supported", nameof(hash));

            // Convert from base58 to bytes, then to hex
            var bytes = Base58.Bitcoin.Decode(multihash);
            return "0x" + BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        public static string DecodeQmHash(string bytes32)
        {
            if (!bytes32.StartsWith("0x"))
                throw new ArgumentException("Input must start with 0x", nameof(bytes32));

            try
            {
                var bytes = HexToBytes(bytes32[2..]);
                return "Qm" + Base58.Bitcoin.Encode(bytes);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid hex string format", nameof(bytes32), ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unexpected error processing bytes32 hash", ex);
            }
        }

        //

        private static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new FormatException("Hex string must have an even number of characters");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}