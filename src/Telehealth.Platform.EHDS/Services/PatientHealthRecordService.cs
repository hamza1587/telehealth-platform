using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Telehealth.Platform.Domain.Entities;
using FhirCondition = Hl7.Fhir.Model.Condition;
using FhirAllergyIntolerance = Hl7.Fhir.Model.AllergyIntolerance;
using FhirImmunization = Hl7.Fhir.Model.Immunization;
using FhirProcedure = Hl7.Fhir.Model.Procedure;
using FhirObservation = Hl7.Fhir.Model.Observation;
using FhirMedicationRequest = Hl7.Fhir.Model.MedicationRequest;

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

    public async Task<PatientHealthRecord> AddConditionAsync(Guid recordId, Telehealth.Platform.Domain.Entities.Condition condition)
    {
        var record = await _context.PatientHealthRecords.FindAsync(recordId);
        if (record == null)
            throw new ArgumentException("Record not found", nameof(recordId));

        record.AddCondition(condition);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<PatientHealthRecord> AddMedicationAsync(Guid recordId, Telehealth.Platform.Domain.Entities.Medication medication)
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

        var bundle = new Bundle
        {
            Type = Bundle.BundleType.Collection,
            Entry = new List<Bundle.EntryComponent>()
        };

        var patient = new Patient
        {
            Id = record.PatientId.ToString(),
            Active = true
        };
        bundle.Entry.Add(new Bundle.EntryComponent
        {
            Resource = patient
        });

        foreach (var condition in record.Conditions)
        {
            var fhirCondition = new FhirCondition
            {
                Id = condition.Id.ToString(),
                ClinicalStatus = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://terminology.hl7.org/CodeSystem/condition-clinical",
                            Code = condition.ClinicalStatus
                        }
                    }
                },
                VerificationStatus = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://terminology.hl7.org/CodeSystem/condition-ver-status",
                            Code = condition.VerificationStatus
                        }
                    }
                },
                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = condition.System,
                            Code = condition.Code,
                            Display = condition.Display
                        }
                    }
                },
                Onset = new FhirDateTime(condition.OnsetDateTime)
            };
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                Resource = fhirCondition
            });
        }

        foreach (var medication in record.Medications)
        {
            var medRequest = new FhirMedicationRequest
            {
                Id = medication.Id.ToString(),
                Medication = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = medication.System,
                            Code = medication.Code,
                            Display = medication.Display
                        }
                    }
                },
                DosageInstruction = new List<Dosage>
                {
                    new Dosage
                    {
                        Text = $"{medication.Dosage} {medication.Frequency}",
                        Timing = new Timing
                        {
                            Repeat = new Timing.RepeatComponent
                            {
                                Frequency = int.Parse(medication.Frequency.Split(' ')[0])
                            }
                        }
                    }
                },
                AuthoredOn = medication.StartDate.ToString("o")
            };
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                Resource = medRequest
            });
        }

        foreach (var allergy in record.Allergies)
        {
            var allergyIntolerance = new FhirAllergyIntolerance
            {
                Id = allergy.Id.ToString(),
                ClinicalStatus = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://terminology.hl7.org/CodeSystem/allergyintolerance-clinical",
                            Code = allergy.ClinicalStatus
                        }
                    }
                },
                Criticality = (FhirAllergyIntolerance.AllergyIntoleranceCriticality?)Enum.Parse(
                    typeof(FhirAllergyIntolerance.AllergyIntoleranceCriticality), 
                    allergy.Criticality, 
                    true),
                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = allergy.System,
                            Code = allergy.Code,
                            Display = allergy.Display
                        }
                    }
                }
            };
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                Resource = allergyIntolerance
            });
        }

        foreach (var immunization in record.Immunizations)
        {
            var fhirImmunization = new FhirImmunization
            {
                Id = immunization.Id.ToString(),
                Status = FhirImmunization.ImmunizationStatusCodes.Completed,
                VaccineCode = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = immunization.System,
                            Code = immunization.VaccineCode,
                            Display = immunization.Display
                        }
                    }
                },
                Occurrence = new FhirDateTime(immunization.AdministrationDate),
                LotNumber = immunization.LotNumber
            };
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                Resource = fhirImmunization
            });
        }

        foreach (var procedure in record.Procedures)
        {
            var fhirProcedure = new FhirProcedure
            {
                Id = procedure.Id.ToString(),
                Status = (EventStatus?)Enum.Parse(typeof(EventStatus), procedure.Status ?? "completed", true),
                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = procedure.System,
                            Code = procedure.Code,
                            Display = procedure.Display
                        }
                    }
                },
                Performed = new FhirDateTime(procedure.PerformedDate)
            };
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                Resource = fhirProcedure
            });
        }

        foreach (var observation in record.Observations)
        {
            var fhirObservation = new FhirObservation
            {
                Id = observation.Id.ToString(),
                Status = ObservationStatus.Final,
                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = observation.System,
                            Code = observation.Code,
                            Display = observation.Display
                        }
                    }
                },
                Value = new Quantity
                {
                    Value = decimal.Parse(observation.Value),
                    Unit = observation.Unit
                },
                Effective = new FhirDateTime(observation.EffectiveDateTime)
            };
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                Resource = fhirObservation
            });
        }

        var serializer = new FhirJsonSerializer();
        return serializer.SerializeToString(bundle);
    }

    public async Task<string> ImportFromFhirAsync(string fhirJson)
    {
        var parser = new FhirJsonParser();
        var bundle = parser.Parse<Bundle>(fhirJson);

        if (bundle == null || bundle.Entry == null)
            throw new ArgumentException("Invalid FHIR bundle");

        var patientResource = bundle.Entry
            .FirstOrDefault(e => e.Resource is Patient)?.Resource as Patient;

        var patientId = patientResource?.Id != null ? 
            Guid.Parse(patientResource.Id) : Guid.NewGuid();

        var record = new PatientHealthRecord(patientId, "FHIR Import", false);

        foreach (var entry in bundle.Entry.Where(e => e.Resource is FhirCondition))
        {
            var condition = entry.Resource as FhirCondition;
            if (condition?.Code?.Coding?.FirstOrDefault() != null)
            {
                var coding = condition.Code.Coding.First();
                record.AddCondition(new Telehealth.Platform.Domain.Entities.Condition
                {
                    Id = condition.Id != null ? Guid.Parse(condition.Id) : Guid.NewGuid(),
                    Code = coding.Code,
                    System = coding.System,
                    Display = coding.Display,
                    ClinicalStatus = condition.ClinicalStatus?.Coding?.FirstOrDefault()?.Code ?? "active",
                    VerificationStatus = condition.VerificationStatus?.Coding?.FirstOrDefault()?.Code ?? "confirmed",
                    OnsetDateTime = condition.Onset is FhirDateTime dt ? dt.ToDateTimeOffset(TimeSpan.Zero) : DateTimeOffset.UtcNow
                });
            }
        }

        foreach (var entry in bundle.Entry.Where(e => e.Resource is FhirMedicationRequest))
        {
            var medRequest = entry.Resource as FhirMedicationRequest;
            if (medRequest?.Medication is CodeableConcept medConcept &&
                medConcept.Coding?.FirstOrDefault() != null)
            {
                var coding = medConcept.Coding.First();
                var dosage = medRequest.DosageInstruction?.FirstOrDefault();
                record.AddMedication(new Telehealth.Platform.Domain.Entities.Medication
                {
                    Id = medRequest.Id != null ? Guid.Parse(medRequest.Id) : Guid.NewGuid(),
                    Code = coding.Code,
                    System = coding.System,
                    Display = coding.Display,
                    Dosage = dosage?.Text ?? "As prescribed",
                    Frequency = dosage?.Timing?.Repeat?.Frequency?.ToString() ?? "1",
                    StartDate = DateTimeOffset.Parse(medRequest.AuthoredOn ?? DateTime.UtcNow.ToString("o"))
                });
            }
        }

        foreach (var entry in bundle.Entry.Where(e => e.Resource is FhirAllergyIntolerance))
        {
            var allergy = entry.Resource as FhirAllergyIntolerance;
            if (allergy?.Code?.Coding?.FirstOrDefault() != null)
            {
                var coding = allergy.Code.Coding.First();
                record.Allergies.Add(new Telehealth.Platform.Domain.Entities.AllergyIntolerance
                {
                    Id = allergy.Id != null ? Guid.Parse(allergy.Id) : Guid.NewGuid(),
                    Code = coding.Code,
                    System = coding.System,
                    Display = coding.Display,
                    ClinicalStatus = allergy.ClinicalStatus?.Coding?.FirstOrDefault()?.Code ?? "active",
                    Criticality = allergy.Criticality?.ToString() ?? "low",
                    Reaction = allergy.Reaction?.FirstOrDefault()?.Manifestation?.FirstOrDefault()?.Text
                });
            }
        }

        foreach (var entry in bundle.Entry.Where(e => e.Resource is FhirImmunization))
        {
            var immunization = entry.Resource as FhirImmunization;
            if (immunization?.VaccineCode?.Coding?.FirstOrDefault() != null)
            {
                var coding = immunization.VaccineCode.Coding.First();
                record.Immunizations.Add(new Telehealth.Platform.Domain.Entities.Immunization
                {
                    Id = immunization.Id != null ? Guid.Parse(immunization.Id) : Guid.NewGuid(),
                    VaccineCode = coding.Code,
                    System = coding.System,
                    Display = coding.Display,
                    AdministrationDate = immunization.Occurrence is FhirDateTime dt ?
                        dt.ToDateTimeOffset(TimeSpan.Zero) : DateTimeOffset.UtcNow,
                    LotNumber = immunization.LotNumber,
                    Site = immunization.Site?.Coding?.FirstOrDefault()?.Display
                });
            }
        }

        foreach (var entry in bundle.Entry.Where(e => e.Resource is FhirProcedure))
        {
            var procedure = entry.Resource as FhirProcedure;
            if (procedure?.Code?.Coding?.FirstOrDefault() != null)
            {
                var coding = procedure.Code.Coding.First();
                record.Procedures.Add(new Telehealth.Platform.Domain.Entities.Procedure
                {
                    Id = procedure.Id != null ? Guid.Parse(procedure.Id) : Guid.NewGuid(),
                    Code = coding.Code,
                    System = coding.System,
                    Display = coding.Display,
                    PerformedDate = procedure.Performed is FhirDateTime dt ?
                        dt.ToDateTimeOffset(TimeSpan.Zero) : DateTimeOffset.UtcNow,
                    Status = procedure.Status?.ToString() ?? "completed",
                    Notes = procedure.Note?.FirstOrDefault()?.Text?.ToString() ?? string.Empty
                });
            }
        }

        foreach (var entry in bundle.Entry.Where(e => e.Resource is FhirObservation))
        {
            var observation = entry.Resource as FhirObservation;
            if (observation?.Code?.Coding?.FirstOrDefault() != null)
            {
                var coding = observation.Code.Coding.First();
                var value = observation.Value as Quantity;
                record.Observations.Add(new Telehealth.Platform.Domain.Entities.Observation
                {
                    Id = observation.Id != null ? Guid.Parse(observation.Id) : Guid.NewGuid(),
                    Code = coding.Code,
                    System = coding.System,
                    Display = coding.Display,
                    Value = value?.Value?.ToString() ?? "0",
                    Unit = value?.Unit ?? "",
                    EffectiveDateTime = observation.Effective is FhirDateTime dt ?
                        dt.ToDateTimeOffset(TimeSpan.Zero) : DateTimeOffset.UtcNow
                });
            }
        }
        
        _context.PatientHealthRecords.Add(record);
        await _context.SaveChangesAsync();
        
        return record.Id.ToString();
    }
}