using System.Security.Cryptography;
using System.Text;

namespace Telehealth.Platform.Domain.Services;

public class KAnonymityService
{
    /// <summary>
    /// Applies k-anonymity to a dataset by generalizing or suppressing quasi-identifiers
    /// </summary>
    public List<Dictionary<string, object>> ApplyKAnonymity(
        List<Dictionary<string, object>> data,
        List<string> quasiIdentifiers,
        int k)
    {
        var anonymizedData = new List<Dictionary<string, object>>();
        var equivalenceClasses = new Dictionary<string, List<Dictionary<string, object>>>();

        // Group records by quasi-identifier values
        foreach (var record in data)
        {
            var key = BuildEquivalenceClassKey(record, quasiIdentifiers);
            
            if (!equivalenceClasses.ContainsKey(key))
            {
                equivalenceClasses[key] = new List<Dictionary<string, object>>();
            }
            
            equivalenceClasses[key].Add(record);
        }

        // Apply generalization or suppression to meet k-anonymity
        foreach (var (key, records) in equivalenceClasses)
        {
            if (records.Count >= k)
            {
                // Class already satisfies k-anonymity
                anonymizedData.AddRange(records);
            }
            else
            {
                // Need to generalize or suppress
                var generalizedRecords = GeneralizeRecords(records, quasiIdentifiers, k);
                anonymizedData.AddRange(generalizedRecords);
            }
        }

        return anonymizedData;
    }

    /// <summary>
    /// Checks if a dataset satisfies k-anonymity
    /// </summary>
    public bool CheckKAnonymity(
        List<Dictionary<string, object>> data,
        List<string> quasiIdentifiers,
        int k)
    {
        var equivalenceClasses = new Dictionary<string, int>();

        foreach (var record in data)
        {
            var key = BuildEquivalenceClassKey(record, quasiIdentifiers);
            
            if (!equivalenceClasses.ContainsKey(key))
            {
                equivalenceClasses[key] = 0;
            }
            
            equivalenceClasses[key]++;
        }

        return equivalenceClasses.Values.All(count => count >= k);
    }

    /// <summary>
    /// Calculates the k-anonymity level of a dataset
    /// </summary>
    public int CalculateKAnonymityLevel(
        List<Dictionary<string, object>> data,
        List<string> quasiIdentifiers)
    {
        var equivalenceClasses = new Dictionary<string, int>();

        foreach (var record in data)
        {
            var key = BuildEquivalenceClassKey(record, quasiIdentifiers);
            
            if (!equivalenceClasses.ContainsKey(key))
            {
                equivalenceClasses[key] = 0;
            }
            
            equivalenceClasses[key]++;
        }

        return equivalenceClasses.Values.Min();
    }

    private string BuildEquivalenceClassKey(Dictionary<string, object> record, List<string> quasiIdentifiers)
    {
        var keyParts = new List<string>();
        
        foreach (var identifier in quasiIdentifiers)
        {
            if (record.TryGetValue(identifier, out var value))
            {
                keyParts.Add($"{identifier}:{value}");
            }
        }
        
        return string.Join("|", keyParts);
    }

    private List<Dictionary<string, object>> GeneralizeRecords(
        List<Dictionary<string, object>> records,
        List<string> quasiIdentifiers,
        int k)
    {
        // Simplified generalization - in production, use more sophisticated algorithms
        var generalizedRecords = new List<Dictionary<string, object>>();
        
        foreach (var record in records)
        {
            var generalizedRecord = new Dictionary<string, object>(record);
            
            foreach (var identifier in quasiIdentifiers)
            {
                if (generalizedRecord.TryGetValue(identifier, out var value))
                {
                    generalizedRecord[identifier] = GeneralizeValue(value);
                }
            }
            
            generalizedRecords.Add(generalizedRecord);
        }
        
        return generalizedRecords;
    }

    private object GeneralizeValue(object value)
    {
        // Simplified generalization logic
        switch (value)
        {
            case int intValue:
                // Generalize to range
                var range = (intValue / 10) * 10;
                return $"{range}-{range + 9}";
            case string stringValue when DateTime.TryParse(stringValue, out var dateValue):
                // Generalize date to month/year
                return $"{dateValue:yyyy-MM}";
            case string stringValue:
                // Generalize string to first character or hash
                return stringValue.Length > 0 ? stringValue[0].ToString() : "*";
            default:
                return "*";
        }
    }

    /// <summary>
    /// Hashes a value for additional privacy
    /// </summary>
    public string HashValue(string value)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
