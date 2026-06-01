using Telehealth.Platform.Domain.Entities;

namespace Telehealth.Platform.EHDS.Services;

public interface IDeIdentificationService
{
    List<Dictionary<string, object>> DeIdentifyRecords(
        List<PatientHealthRecord> records,
        int kAnonymityLevel = 5,
        double epsilon = 1.0);

    Dictionary<string, object> DeIdentifyRecord(
        PatientHealthRecord record,
        int kAnonymityLevel = 5,
        double epsilon = 1.0);

    bool ValidatePrivacy(
        List<Dictionary<string, object>> data,
        int kAnonymityLevel,
        double epsilon);

    int CalculateCurrentKAnonymity(List<Dictionary<string, object>> data);

    Dictionary<string, object> RemoveDirectIdentifiers(Dictionary<string, object> record);
}
