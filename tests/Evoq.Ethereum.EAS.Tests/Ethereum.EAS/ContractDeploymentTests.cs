using Evoq.Ethereum.Chains;
using Microsoft.Extensions.Logging;

namespace Evoq.Ethereum.EAS;

[TestClass]
public class ContractDeploymentTests
{
    static readonly EthereumAddress easAddress = Contracts.GetEASAddress(ChainIds.Hardhat);
    static readonly EthereumAddress registryAddress = Contracts.GetSchemaRegistryAddress(ChainIds.Hardhat);

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        var testContext = EthereumTestContext.CreateHardhatContext(out var logger);
        var eas = new EAS(easAddress);
        var registry = new SchemaRegistry(registryAddress);

        // Check if Schema Registry is deployed
        var isRegistryDeployed = await registry.IsDeployedAsync(testContext);
        logger.LogInformation($"Schema Registry deployed: {isRegistryDeployed}");
        if (!isRegistryDeployed)
        {
            context.Properties["ContractDeploymentFailed"] = "Schema Registry contract is not deployed. Please deploy the contracts before running tests.";
            return;
        }

        // Check if EAS is deployed
        var isEASDeployed = await eas.IsDeployedAsync(testContext);
        logger.LogInformation($"EAS deployed: {isEASDeployed}");
        if (!isEASDeployed)
        {
            context.Properties["ContractDeploymentFailed"] = "EAS contract is not deployed. Please deploy the contracts before running tests.";
            return;
        }
    }

    [TestMethod]
    public async Task Test_Deployment_VerifyContracts()
    {
        var context = EthereumTestContext.CreateHardhatContext(out var logger);
        var eas = new EAS(easAddress);
        var registry = new SchemaRegistry(registryAddress);

        // Check if Schema Registry is deployed
        var isRegistryDeployed = await registry.IsDeployedAsync(context);
        logger.LogInformation($"Schema Registry deployed: {isRegistryDeployed}");
        Assert.IsTrue(isRegistryDeployed, "Schema Registry contract should be deployed");

        // Check if EAS is deployed
        var isEASDeployed = await eas.IsDeployedAsync(context);
        logger.LogInformation($"EAS deployed: {isEASDeployed}");
        Assert.IsTrue(isEASDeployed, "EAS contract should be deployed");
    }
}