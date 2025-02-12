namespace Evoq.Ethereum.EAS;

/// <summary>
/// A schema record returned by the schema registry contract.
/// </summary>
public interface ISchemaRecord
{
    byte[] UID { get; set; }
    string Resolver { get; set; }
    bool Revocable { get; set; }
    string Schema { get; set; }
}
