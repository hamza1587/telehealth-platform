using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Telehealth.Platform.Domain.Entities;

namespace Telehealth.Platform.EHDS.Services;

public class PatientHealthRecordService : IPatientHealthRecordService
{
    private readonly EhdsDbContext _context;

    public PatientHealthRecordService(EhdsDbContext context)
    {
        _context = context;
    }

    public async Task<PatientHealthRecord> CreateRecordAsync(Guid patientId, string dataOrigin, bool isCrossBorderAccessible = false)
    {
        var record = new PatientHealthRecord(patientId, dataOrigin, isCrossBorderAccessible);
        _context.PatientHealthRecords.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<PatientHealthRecord?> GetRecordAsync(Guid id)
    {
        return await _context.PatientHealthRecords.FindAsync(id);
    }

    public async Task<PatientHealthRecord?> GetRecordByPatientIdAsync(Guid patientId)
    {
        return await _context.PatientHealthRecords
            .FirstOrDefaultAsync(r => r.PatientId == patientId);
    }

    public async Task<List<PatientHealthRecord>> GetAllRecordsAsync()
    {
        return await _context.PatientHealthRecords.ToListAsync();
    }

    public async Task<PatientHealthRecord> AddConditionAsync(Guid recordId, Condition condition)
    {
        var record = await _context.PatientHealthRecords.FindAsync(recordId);
        if (record == null)
            throw new ArgumentException("Record not found", nameof(recordId));

        record.AddCondition(condition);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<PatientHealthRecord> AddMedicationAsync(Guid recordId, Medication medication)
    {
        var record = await _context.PatientHealthRecords.FindAsync(recordId);
        if (record == null)
            throw new ArgumentException("Record not found", nameof(recordId));

        record.AddMedication(medication);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<PatientHealthRecord> SetCrossBorderAccessAsync(Guid recordId, bool accessible)
    {
        var record = await _context.PatientHealthRecords.FindAsync(recordId);
        if (record == null)
            throw new ArgumentException("Record not found", nameof(recordId));

        record.SetCrossBorderAccess(accessible);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<string> ExportToFhirAsync(Guid recordId)
    {
        var record = await _context.PatientHealthRecords.FindAsync(recordId);
        if (record == null)
            throw new ArgumentException("Record not found", nameof(recordId));

        // Convert to FHIR R4 format
        var fhirResource = new
        {
            resourceType = "Bundle",
            type = "collection",
            entry = new List<object>
            {
                new
                {
                    resource = new
                    {
                        resourceType = "Patient",
                        id = record.PatientId.ToString()
                    }
                }
            }
        };

        // Add conditions
        foreach (var condition in record.Conditions)
        {
            fhirResource.entry.Add(new
            {
                resource = new
                {
                    resourceType = "Condition",
                    id = condition.Id.ToString(),
                    code = new
                    {
                        coding = new[]
                        {
                            new
                            {
                                system = condition.System,
                                code = condition.Code,
                                display = condition.Display
                            }
                        }
                    },
                    clinicalStatus = new
                    {
                        coding = new[]
                        {
                            new
                            {
                                system = "http://terminology.hl7.org/CodeSystem/condition-clinical",
                                code = condition.ClinicalStatus
                            }
                        }
                    },
                    verificationStatus = new
                    {
                        coding = new[]
                        {
                            new
                            {
                                system = "http://terminology.hl7.org/CodeSystem/condition-ver-status",
                                code = condition.VerificationStatus
                            }
                        }
                    },
                    onsetDateTime = condition.OnsetDateTime.ToString("o")
                }
            });
        }

        // Add medications
        foreach (var medication in record.Medications)
        {
            fhirResource.entry.Add(new
            {
                resource = new
                {
                    resourceType = "MedicationRequest",
                    id = medication.Id.ToString(),
                    medicationCodeableConcept = new
                    {
                        coding = new[]
                        {
                            new
                            {
                                system = medication.System,
                                code = medication.Code,
                                display = medication.Display
                            }
                        }
                    },
                    dosageInstruction = new[]
                    {
                        new
                        {
                            text = $"{medication.Dosage} {medication.Frequency}",
                            timing = new
                            {
                                repeat = new
                                {
                                    frequency = medication.Frequency
                                }
                            }
                        }
                    },
                    authoredOn = medication.StartDate.ToString("o")
                }
            });
        }

        return JsonSerializer.Serialize(fhirResource, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public async Task<string> ImportFromFhirAsync(string fhirJson)
    {
        // Parse FHIR JSON and convert to PatientHealthRecord
        // This is a simplified implementation
        var fhirData = JsonSerializer.Deserialize<JsonElement>(fhirJson);
        
        if (fhirData.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Invalid FHIR JSON");

        // Extract patient ID
        var patientId = Guid.NewGuid(); // In production, extract from FHIR resource
        
        var record = new PatientHealthRecord(patientId, "FHIR Import", false);
        
        // Parse conditions, medications, etc. from FHIR
        // This would be more comprehensive in production
        
        _context.PatientHealthRecords.Add(record);
        await _context.SaveChangesAsync();
        
        return record.Id.ToString();
    }
}
