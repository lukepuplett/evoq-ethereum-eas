using Evoq.Ethereum.Chains;
using Evoq.Ethereum.Contracts;
using Evoq.Ethereum.JsonRPC;
using Evoq.Ethereum.Transactions;
using Microsoft.Extensions.Logging;

namespace Evoq.Ethereum.EAS;

[TestClass]
public class SchemaRegistryTests
{
    // NOTE / The Hardhat addresses are consistent only when the default Hardhat account
    // is used as the deployer, and it is the accounts first transaction. This is because
    // contract addresses are a hash of the sender and the transaction nonce.

    static readonly EthereumAddress registryAddress = Contracts.GetSchemaRegistryAddress(ChainIds.Hardhat);

    //

    [TestMethod]
    public async Task RegisterSchema()
    {
        InteractionContext context = CreateContext();

        var registry = new SchemaRegistry(registryAddress);
        var schema = "uint256 value, string name";
        var revocable = true;
        var resolver = EthereumAddress.Zero;

        var r = await registry.Register(context, schema, resolver, revocable);

        Assert.IsTrue(r.Success);

        Console.WriteLine($"'{schema}' registered with UID: {r.Result}");
    }

    //

    private static InteractionContext CreateContext()
    {
        var loggerFactory = new LoggerFactory();
        var nonces = new InMemoryNonceStore(loggerFactory);

        var pkStr = Environment.GetEnvironmentVariable("Blockchain__Ethereum__Addresses__Hardhat1PrivateKey");
        var addrStr = Environment.GetEnvironmentVariable("Blockchain__Ethereum__Addresses__Hardhat1Address");
        var address = EthereumAddress.Parse(addrStr!);

        var account = new SenderAccount(pkStr!, address);
        var sender = new Sender(account, nonces);
        var endpoint = new Endpoint(ChainNames.Hardhat, ChainNames.Hardhat, "http://localhost:8545", loggerFactory);

        var context = new InteractionContext(endpoint, sender, UseSuggestedGasOptions);
        return context;
    }

    private static GasOptions UseSuggestedGasOptions(ITransactionFeeEstimate estimate)
    {
        return estimate.ToSuggestedGasOptions();
    }
}