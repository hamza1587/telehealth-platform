namespace Telehealth.Platform.Analytics.Services;

public interface IDataWarehouseService
{
    Task<string> ExecuteQueryAsync(string queryDefinition);
    Task<List<Dictionary<string, object>>> QueryDataAsync(string query);
    Task<bool> StoreMetricsAsync(Dictionary<string, object> metrics);
}
