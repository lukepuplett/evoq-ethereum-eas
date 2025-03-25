using System;
using Evoq.Ethereum.Chains;
using Evoq.Ethereum.JsonRPC;

namespace Evoq.Ethereum.EAS;

/// <summary>
/// Static class for getting the addresses of the EAS and Schema Registry contracts
/// </summary>
public static class Contracts
{
    // EAS Addresses - Mainnets
    private const string EAS_ADDRESS_MAINNET = "0xA1207F3BBa224E2c9c3c6D5aF63D0eb1582Ce587";
    private const string EAS_ADDRESS_OPTIMISM_MAINNET = "0x4200000000000000000000000000000000000021";
    private const string EAS_ADDRESS_POLYGON_MAINNET = "0x5E634ef5355f45A855d02D66eCD687b1502AF790";
    private const string EAS_ADDRESS_ARBITRUM_MAINNET = "0xbD75f629A22Dc1ceD33dDA0b68c546A1c035c458";
    private const string EAS_ADDRESS_BASE_MAINNET = "0x4200000000000000000000000000000000000021";
    private const string EAS_ADDRESS_SCROLL_MAINNET = "0xC47300428b6AD2c7D03BB76D05A176058b47E6B0";
    private const string EAS_ADDRESS_ZKSYNC_MAINNET = "0x21d8d4eE83b80bc0Cc0f2B7df3117Cf212d02901";
    private const string EAS_ADDRESS_CELO_MAINNET = "0x72E1d8ccf5299fb36fEfD8CC4394B8ef7e98Af92";
    private const string EAS_ADDRESS_BLAST_MAINNET = "0x4200000000000000000000000000000000000021";
    private const string EAS_ADDRESS_LINEA_MAINNET = "0x4200000000000000000000000000000000000021";

    // EAS Addresses - Testnets
    private const string EAS_ADDRESS_SEPOLIA = "0xC2679fBD37d54388Ce493F1DB75320D236e1815e";
    private const string EAS_ADDRESS_OPTIMISM_SEPOLIA = "0x4200000000000000000000000000000000000021";
    private const string EAS_ADDRESS_POLYGON_SEPOLIA = "0x5E634ef5355f45A855d02D66eCD687b1502AF790";
    private const string EAS_ADDRESS_ARBITRUM_SEPOLIA = "0xbD75f629A22Dc1ceD33dDA0b68c546A1c035c458";
    private const string EAS_ADDRESS_BASE_SEPOLIA = "0x4200000000000000000000000000000000000021";
    private const string EAS_ADDRESS_SCROLL_SEPOLIA = "0xC47300428b6AD2c7D03BB76D05A176058b47E6B0";
    private const string EAS_ADDRESS_LINEA_SEPOLIA = "0x4200000000000000000000000000000000000021";

    // EAS Addresses - Development
    private const string EAS_ADDRESS_HARDHAT = "0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512";

    // Schema Registry Addresses - Mainnets
    private const string SCHEMA_ADDRESS_MAINNET = "0xA7b39296258348C78294F95B872b282326A97BDF";
    private const string SCHEMA_ADDRESS_OPTIMISM_MAINNET = "0x4200000000000000000000000000000000000020";
    private const string SCHEMA_ADDRESS_POLYGON_MAINNET = "0x7876EEF51A891E737AF8ba5A5E0f0Fd29073D5a7";
    private const string SCHEMA_ADDRESS_ARBITRUM_MAINNET = "0xA310da9c5B885E7fb3fbA9D66E9Ba6Df512b78eB";
    private const string SCHEMA_ADDRESS_ARBITRUM_NOVA = "0x49563d0DA8DF38ef2eBF9C1167270334D72cE0AE";
    private const string SCHEMA_ADDRESS_BASE_MAINNET = "0x4200000000000000000000000000000000000020";
    private const string SCHEMA_ADDRESS_SCROLL_MAINNET = "0xD2CDF46556543316e7D34e8eDc4624e2bB95e3B6";
    private const string SCHEMA_ADDRESS_ZKSYNC_MAINNET = "0xB8566376dFe68B76FA985D5448cc2FbD578412a2";
    private const string SCHEMA_ADDRESS_CELO_MAINNET = "0x5ece93bE4BDCF293Ed61FA78698B594F2135AF34";
    private const string SCHEMA_ADDRESS_BLAST_MAINNET = "0x4200000000000000000000000000000000000020";
    private const string SCHEMA_ADDRESS_LINEA_MAINNET = "0x55D26f9ae0203EF95494AE4C170eD35f4Cf77797";

    // Schema Registry Addresses - Testnets
    private const string SCHEMA_ADDRESS_SEPOLIA = "0x0a7E2Ff54e76B8E6659aedc9103FB21c038050D0";
    private const string SCHEMA_ADDRESS_OPTIMISM_SEPOLIA = "0x4200000000000000000000000000000000000020";
    private const string SCHEMA_ADDRESS_POLYGON_AMOY = "0x23c5701A1BDa89C61d181BD79E5203c730708AE7";
    private const string SCHEMA_ADDRESS_ARBITRUM_GOERLI = "0x55D26f9ae0203EF95494AE4C170eD35f4Cf77797";
    private const string SCHEMA_ADDRESS_SCROLL_SEPOLIA = "0x55D26f9ae0203EF95494AE4C170eD35f4Cf77797";
    private const string SCHEMA_ADDRESS_LINEA_GOERLI = "0x55D26f9ae0203EF95494AE4C170eD35f4Cf77797";

    // Schema Registry Addresses - Development
    private const string SCHEMA_ADDRESS_HARDHAT = "0x5FbDB2315678afecb367f032d93F642f64180aa3";

    /// <summary>
    /// Get the address of the EAS contract for a given network
    /// </summary>
    /// <param name="networkId">The network ID</param>
    /// <returns>The address of the EAS contract</returns>
    /// <exception cref="ArgumentException">Thrown when the network ID is not supported</exception>
    public static EthereumAddress GetEASAddress(string networkId)
    {
        var addressString = networkId switch
        {
            // Mainnets
            ChainIds.EthereumMainnet => EAS_ADDRESS_MAINNET,
            ChainIds.OptimismMainnet => EAS_ADDRESS_OPTIMISM_MAINNET,
            ChainIds.PolygonMainnet => EAS_ADDRESS_POLYGON_MAINNET,
            ChainIds.ArbitrumMainnet => EAS_ADDRESS_ARBITRUM_MAINNET,
            ChainIds.ArbitrumNova => EAS_ADDRESS_BASE_MAINNET,
            ChainIds.BaseMainnet => EAS_ADDRESS_BASE_MAINNET,
            ChainIds.ScrollMainnet => EAS_ADDRESS_SCROLL_MAINNET,
            ChainIds.ZkSyncMainnet => EAS_ADDRESS_ZKSYNC_MAINNET,
            ChainIds.CeloMainnet => EAS_ADDRESS_CELO_MAINNET,
            ChainIds.BlastMainnet => EAS_ADDRESS_BLAST_MAINNET,
            ChainIds.LineaMainnet => EAS_ADDRESS_LINEA_MAINNET,
            // Testnets
            ChainIds.EthereumSepolia => EAS_ADDRESS_SEPOLIA,
            ChainIds.OptimismSepolia => EAS_ADDRESS_OPTIMISM_SEPOLIA,
            ChainIds.OptimismGoerli => EAS_ADDRESS_OPTIMISM_SEPOLIA,
            ChainIds.BaseSepolia => EAS_ADDRESS_BASE_SEPOLIA,
            ChainIds.BaseGoerli => EAS_ADDRESS_BASE_SEPOLIA,
            ChainIds.ArbitrumGoerli => EAS_ADDRESS_ARBITRUM_SEPOLIA,
            ChainIds.PolygonAmoy => EAS_ADDRESS_POLYGON_SEPOLIA,
            ChainIds.ScrollSepolia => EAS_ADDRESS_SCROLL_SEPOLIA,
            ChainIds.LineaGoerli => EAS_ADDRESS_LINEA_SEPOLIA,
            // Development
            ChainIds.Hardhat => EAS_ADDRESS_HARDHAT,
            _ => throw new ArgumentException($"Cannot get EAS address for unsupported network: {networkId}")
        };

        return EthereumAddress.Parse(addressString);
    }

    /// <summary>
    /// Get the address of the Schema Registry contract for a given network
    /// </summary>
    /// <param name="networkId">The network ID</param>
    /// <returns>The address of the Schema Registry contract</returns>
    /// <exception cref="ArgumentException">Thrown when the network ID is not supported</exception>
    public static EthereumAddress GetSchemaRegistryAddress(string networkId)
    {
        var addressString = networkId switch
        {
            // Mainnets
            ChainIds.EthereumMainnet => SCHEMA_ADDRESS_MAINNET,
            ChainIds.OptimismMainnet => SCHEMA_ADDRESS_OPTIMISM_MAINNET,
            ChainIds.PolygonMainnet => SCHEMA_ADDRESS_POLYGON_MAINNET,
            ChainIds.ArbitrumMainnet => SCHEMA_ADDRESS_ARBITRUM_MAINNET,
            ChainIds.ArbitrumNova => SCHEMA_ADDRESS_ARBITRUM_NOVA,
            ChainIds.BaseMainnet => SCHEMA_ADDRESS_BASE_MAINNET,
            ChainIds.ScrollMainnet => SCHEMA_ADDRESS_SCROLL_MAINNET,
            ChainIds.ZkSyncMainnet => SCHEMA_ADDRESS_ZKSYNC_MAINNET,
            ChainIds.CeloMainnet => SCHEMA_ADDRESS_CELO_MAINNET,
            ChainIds.BlastMainnet => SCHEMA_ADDRESS_BLAST_MAINNET,
            ChainIds.LineaMainnet => SCHEMA_ADDRESS_LINEA_MAINNET,
            // Testnets
            ChainIds.EthereumSepolia => SCHEMA_ADDRESS_SEPOLIA,
            ChainIds.OptimismSepolia => SCHEMA_ADDRESS_OPTIMISM_SEPOLIA,
            ChainIds.OptimismGoerli => SCHEMA_ADDRESS_OPTIMISM_SEPOLIA,
            ChainIds.BaseSepolia => SCHEMA_ADDRESS_OPTIMISM_SEPOLIA,
            ChainIds.BaseGoerli => SCHEMA_ADDRESS_OPTIMISM_SEPOLIA,
            ChainIds.ArbitrumGoerli => SCHEMA_ADDRESS_ARBITRUM_GOERLI,
            ChainIds.PolygonAmoy => SCHEMA_ADDRESS_POLYGON_AMOY,
            ChainIds.ScrollSepolia => SCHEMA_ADDRESS_SCROLL_SEPOLIA,
            ChainIds.LineaGoerli => SCHEMA_ADDRESS_LINEA_GOERLI,
            // Development
            ChainIds.Hardhat => SCHEMA_ADDRESS_HARDHAT,
            _ => throw new ArgumentException($"Cannot get Schema Registry address for unsupported network: {networkId}")
        };

        return EthereumAddress.Parse(addressString);
    }

    /// <summary>
    /// Get the JSON ABI for the EAS contract for a given network
    /// </summary>
    /// <param name="networkId">The network ID</param>
    /// <returns>The JSON ABI for the EAS contract</returns>
    /// <exception cref="ArgumentException">Thrown when the network ID is not supported</exception>
    public static string GetEASJsonABI(string networkId)
    {
        return ABILoader.LoadAbi("EAS.EAS");
    }

    /// <summary>
    /// Get the JSON ABI for the Schema Registry contract for a given network
    /// </summary>
    /// <param name="networkId">The network ID</param>
    /// <returns>The JSON ABI for the Schema Registry contract</returns>
    /// <exception cref="ArgumentException">Thrown when the network ID is not supported</exception>
    public static string GetSchemaRegistryJsonABI(string networkId)
    {
        return ABILoader.LoadAbi("EAS.SchemaRegistry");
    }
}
