using Evoq.Ethereum.Chains;
using Evoq.Ethereum.Contracts;
using Evoq.Ethereum.JsonRPC;
using Evoq.Ethereum.Transactions;
using Microsoft.Extensions.Logging;

namespace Evoq.Ethereum.EAS;

public static class EthereumTestContext
{
    private static readonly LogLevel DefaultLogLevel = LogLevel.Information;

    public static InteractionContext CreateHardhatContext(out ILogger logger, LogLevel? logLevel = null)
    {
        var loggerFactory = LoggerFactory.Create(
            builder => builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.IncludeScopes = true;
            }).SetMinimumLevel(logLevel ?? DefaultLogLevel));

        logger = loggerFactory.CreateLogger(typeof(EthereumTestContext));

        var pkStr = Environment.GetEnvironmentVariable("Blockchain__Ethereum__Addresses__Hardhat1PrivateKey");
        var addrStr = Environment.GetEnvironmentVariable("Blockchain__Ethereum__Addresses__Hardhat1Address");
        var address = EthereumAddress.Parse(addrStr!);

        var endpoint = new Endpoint(ChainNames.Hardhat, ChainNames.Hardhat, "http://localhost:8545", loggerFactory);

        var chain = endpoint.CreateChain();
        var getTransactionCount = () => chain.GetTransactionCountAsync(new JsonRpcContext(), address, "latest");

        var nonces = new InMemoryNonceStore(loggerFactory, getTransactionCount);

        var account = new SenderAccount(pkStr!, address);
        var sender = new Sender(account, nonces);

        var context = new InteractionContext(endpoint, sender, UseSuggestedGasOptions);

        return context;
    }

    private static GasOptions UseSuggestedGasOptions(ITransactionFeeEstimate estimate)
    {
        return estimate.ToSuggestedGasOptions();
    }
}