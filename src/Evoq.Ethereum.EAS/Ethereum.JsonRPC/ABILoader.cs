using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Evoq.Ethereum.JsonRPC;

/// <summary>
/// Static class for loading ABI (Application Binary Interface) files embedded in the assembly
/// </summary>
internal static class ABILoader
{
    /// <summary>
    /// Loads an ABI file from embedded resources
    /// </summary>
    /// <param name="abiResourceName">Name of the ABI resource file</param>
    /// <returns>The ABI content as a string</returns>
    /// <exception cref="FileNotFoundException">Thrown when the ABI resource cannot be found</exception>
    public static string LoadABI(string abiResourceName)
    {
        string resourcePath = $"Evoq.Ethereum.EAS.ABI.{abiResourceName}.json";

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            var availableResources = assembly.GetManifestResourceNames();
            var joinedNames = string.Join("\n", availableResources);

            throw new FileNotFoundException(
                $"ABI file '{resourcePath}' not found in embedded resources.\nAvailable resources ({availableResources.Length}):\n{joinedNames}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Loads and deserializes an ABI file from embedded resources
    /// </summary>
    /// <typeparam name="T">The type to deserialize the ABI into</typeparam>
    /// <param name="abiResourceName">Name of the ABI resource file</param>
    /// <returns>The deserialized ABI object</returns>
    public static T LoadAndParseABI<T>(string abiResourceName)
    {
        string abiContent = LoadABI(abiResourceName);
        return JsonSerializer.Deserialize<T>(abiContent)
            ?? throw new JsonException($"Failed to deserialize ABI file '{abiResourceName}'");
    }
}