using System;
using Evoq.Ethereum.JsonRPC;

namespace Evoq.Ethereum.EAS;

/// <summary>
/// Static class for getting the addresses of the EAS and Schema Registry contracts
/// </summary>
public static class Contracts
{
    /// <summary>
    /// Get the address of the EAS contract for a given network
    /// </summary>
    /// <param name="networkId">The network ID</param>
    /// <returns>The address of the EAS contract</returns>
    /// <exception cref="ArgumentException">Thrown when the network ID is not supported</exception>
    public static string GetEASAddress(string networkId)
    {
        switch (networkId)
        {
            // Mainnets
            case ChainIds.EthereumMainnet:
                return "0xA1207F3BBa224E2c9c3c6D5aF63D0eb1582Ce587";
            case ChainIds.OptimismMainnet:
                return "0x4200000000000000000000000000000000000021";
            case ChainIds.PolygonMainnet:
                return "0x5E634ef5355f45A855d02D66eCD687b1502AF790";
            case ChainIds.ArbitrumMainnet:
                return "0xbD75f629A22Dc1ceD33dDA0b68c546A1c035c458";
            case ChainIds.ArbitrumNova:
                return "0x6d3dC0Fe5351087E3Af3bDe8eB3F7350ed894fc3";
            case ChainIds.BaseMainnet:
                return "0x4200000000000000000000000000000000000021";
            case ChainIds.ScrollMainnet:
                return "0xC47300428b6AD2c7D03BB76D05A176058b47E6B0";
            case ChainIds.ZkSyncMainnet:
                return "0x21d8d4eE83b80bc0Cc0f2B7df3117Cf212d02901";
            case ChainIds.CeloMainnet:
                return "0x72E1d8ccf5299fb36fEfD8CC4394B8ef7e98Af92";
            case ChainIds.BlastMainnet:
                return "0x4200000000000000000000000000000000000021";
            case ChainIds.LineaMainnet:
                return "0xaEF4103A04090071165F78D45D83A0C0782c2B2a";
            // Testnets
            case ChainIds.EthereumSepolia:
                return "0xC2679fBD37d54388Ce493F1DB75320D236e1815e";
            case ChainIds.OptimismSepolia:
                return "0x4200000000000000000000000000000000000021";
            case ChainIds.OptimismGoerli:
                return "0x4200000000000000000000000000000000000021";
            case ChainIds.BaseSepolia:
                return "0x4200000000000000000000000000000000000021";
            case ChainIds.BaseGoerli:
                return "0x4200000000000000000000000000000000000021";
            case ChainIds.ArbitrumGoerli:
                return "0xaEF4103A04090071165F78D45D83A0C0782c2B2a";
            case ChainIds.PolygonAmoy:
                return "0xb101275a60d8bfb14529C421899aD7CA1Ae5B5Fc";
            case ChainIds.ScrollSepolia:
                return "0xaEF4103A04090071165F78D45D83A0C0782c2B2a";
            case ChainIds.LineaGoerli:
                return "0xaEF4103A04090071165F78D45D83A0C0782c2B2a";
            // Development
            case ChainIds.Hardhat:
                return "0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512";
            default:
                throw new ArgumentException(
                    $"Cannot get EAS address for unsupported network: {networkId}");
        }
    }

    /// <summary>
    /// Get the address of the Schema Registry contract for a given network
    /// </summary>
    /// <param name="networkId">The network ID</param>
    /// <returns>The address of the Schema Registry contract</returns>
    /// <exception cref="ArgumentException">Thrown when the network ID is not supported</exception>
    public static string GetSchemaRegistryAddress(string networkId)
    {
        switch (networkId)
        {
            // Mainnets
            case ChainIds.EthereumMainnet:
                return "0xA7b39296258348C78294F95B872b282326A97BDF";
            case ChainIds.OptimismMainnet:
                return "0x4200000000000000000000000000000000000020";
            case ChainIds.PolygonMainnet:
                return "0x7876EEF51A891E737AF8ba5A5E0f0Fd29073D5a7";
            case ChainIds.ArbitrumMainnet:
                return "0xA310da9c5B885E7fb3fbA9D66E9Ba6Df512b78eB";
            case ChainIds.ArbitrumNova:
                return "0x49563d0DA8DF38ef2eBF9C1167270334D72cE0AE";
            case ChainIds.BaseMainnet:
                return "0x4200000000000000000000000000000000000020";
            case ChainIds.ScrollMainnet:
                return "0xD2CDF46556543316e7D34e8eDc4624e2bB95e3B6";
            case ChainIds.ZkSyncMainnet:
                return "0xB8566376dFe68B76FA985D5448cc2FbD578412a2";
            case ChainIds.CeloMainnet:
                return "0x5ece93bE4BDCF293Ed61FA78698B594F2135AF34";
            case ChainIds.BlastMainnet:
                return "0x4200000000000000000000000000000000000020";
            case ChainIds.LineaMainnet:
                return "0x55D26f9ae0203EF95494AE4C170eD35f4Cf77797";
            // Testnets
            case ChainIds.EthereumSepolia:
                return "0x0a7E2Ff54e76B8E6659aedc9103FB21c038050D0";
            case ChainIds.OptimismSepolia:
                return "0x4200000000000000000000000000000000000020";
            case ChainIds.OptimismGoerli:
                return "0x4200000000000000000000000000000000000020";
            case ChainIds.BaseSepolia:
                return "0x4200000000000000000000000000000000000020";
            case ChainIds.BaseGoerli:
                return "0x4200000000000000000000000000000000000020";
            case ChainIds.ArbitrumGoerli:
                return "0x55D26f9ae0203EF95494AE4C170eD35f4Cf77797";
            case ChainIds.PolygonAmoy:
                return "0x23c5701A1BDa89C61d181BD79E5203c730708AE7";
            case ChainIds.ScrollSepolia:
                return "0x55D26f9ae0203EF95494AE4C170eD35f4Cf77797";
            case ChainIds.LineaGoerli:
                return "0x55D26f9ae0203EF95494AE4C170eD35f4Cf77797";
            // Development
            case ChainIds.Hardhat:
                return "0x5FbDB2315678afecb367f032d93F642f64180aa3";
            default:
                throw new ArgumentException(
                    $"Cannot get Schema Registry address for unsupported network: {networkId}");
        }
    }

    /// <summary>
    /// Get the JSON ABI for the EAS contract for a given network
    /// </summary>
    /// <param name="networkId">The network ID</param>
    /// <returns>The JSON ABI for the EAS contract</returns>
    /// <exception cref="ArgumentException">Thrown when the network ID is not supported</exception>
    public static string GetEASJsonABI(string networkId)
    {
        return ABILoader.LoadABI("EAS.EAS");
    }

    /// <summary>
    /// Get the JSON ABI for the Schema Registry contract for a given network
    /// </summary>
    /// <param name="networkId">The network ID</param>
    /// <returns>The JSON ABI for the Schema Registry contract</returns>
    /// <exception cref="ArgumentException">Thrown when the network ID is not supported</exception>
    public static string GetSchemaRegistryJsonABI(string networkId)
    {
        return ABILoader.LoadABI("EAS.SchemaRegistry");
    }
}
