using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Telehealth.Platform.Domain.Entities;
using Telehealth.Platform.EHDS.Services;
using FhirCondition = Hl7.Fhir.Model.Condition;
using FhirAllergyIntolerance = Hl7.Fhir.Model.AllergyIntolerance;
using FhirImmunization = Hl7.Fhir.Model.Immunization;
using FhirProcedure = Hl7.Fhir.Model.Procedure;
using FhirObservation = Hl7.Fhir.Model.Observation;
using FhirMedicationRequest = Hl7.Fhir.Model.MedicationRequest;

namespace Telehealth.Platform.EHDS.Controllers;

[ApiController]
[Route("fhir")]
public class FhirController : ControllerBase
{
    private readonly IPatientHealthRecordService _patientRecordService;
    private readonly IResearchExportService _researchService;

    public FhirController(IPatientHealthRecordService patientRecordService, IResearchExportService researchService)
    {
        _patientRecordService = patientRecordService;
        _researchService = researchService;
    }

    [HttpGet]
    public IActionResult GetCapabilityStatement()
    {
        var capabilityStatement = new CapabilityStatement
        {
            Id = "ehds-telehealth-platform",
            Name = "EHDS Telehealth Platform",
            Title = "EHDS Telehealth Platform FHIR Server",
            Status = PublicationStatus.Active,
            Date = DateTime.UtcNow.ToString("o"),
            Publisher = "Telehealth Platform",
            Kind = CapabilityStatementKind.Instance,
            Software = new CapabilityStatement.SoftwareComponent
            {
                Name = "Telehealth Platform EHDS Service",
                Version = "1.0.0"
            },
            FhirVersion = FHIRVersion.N4_0_1,
            Format = new List<string> { "application/fhir+json", "application/fhir+xml" },
            Rest = new List<CapabilityStatement.RestComponent>
            {
                new CapabilityStatement.RestComponent
                {
                    Mode = CapabilityStatement.RestfulCapabilityMode.Server,
                    Resource = new List<CapabilityStatement.ResourceComponent>
                    {
                        new CapabilityStatement.ResourceComponent
                        {
                            Type = ResourceType.Patient,
                            Interaction = new List<CapabilityStatement.ResourceInteractionComponent>
                            {
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.Read
                                },
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.SearchType
                                }
                            }
                        },
                        new CapabilityStatement.ResourceComponent
                        {
                            Type = ResourceType.Condition,
                            Interaction = new List<CapabilityStatement.ResourceInteractionComponent>
                            {
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.Read
                                },
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.SearchType
                                }
                            }
                        },
                        new CapabilityStatement.ResourceComponent
                        {
                            Type = ResourceType.MedicationRequest,
                            Interaction = new List<CapabilityStatement.ResourceInteractionComponent>
                            {
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.Read
                                },
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.SearchType
                                }
                            }
                        },
                        new CapabilityStatement.ResourceComponent
                        {
                            Type = ResourceType.AllergyIntolerance,
                            Interaction = new List<CapabilityStatement.ResourceInteractionComponent>
                            {
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.Read
                                },
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.SearchType
                                }
                            }
                        },
                        new CapabilityStatement.ResourceComponent
                        {
                            Type = ResourceType.Immunization,
                            Interaction = new List<CapabilityStatement.ResourceInteractionComponent>
                            {
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.Read
                                },
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.SearchType
                                }
                            }
                        },
                        new CapabilityStatement.ResourceComponent
                        {
                            Type = ResourceType.Procedure,
                            Interaction = new List<CapabilityStatement.ResourceInteractionComponent>
                            {
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.Read
                                },
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.SearchType
                                }
                            }
                        },
                        new CapabilityStatement.ResourceComponent
                        {
                            Type = ResourceType.Observation,
                            Interaction = new List<CapabilityStatement.ResourceInteractionComponent>
                            {
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.Read
                                },
                                new CapabilityStatement.ResourceInteractionComponent
                                {
                                    Code = CapabilityStatement.TypeRestfulInteraction.SearchType
                                }
                            }
                        }
                    }
                }
            }
        };

        return Ok(SerializeResource(capabilityStatement));
    }

    [HttpGet("Patient/{id}")]
    public async Task<IActionResult> GetPatient(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var patientId))
                return BadRequest("Invalid patient ID");

            var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
            if (record == null)
                return NotFound();

            var patient = new Patient
            {
                Id = record.PatientId.ToString(),
                Active = true
            };

            return Ok(SerializeResource(patient));
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to retrieve patient: {ex.Message}");
        }
    }

    [HttpGet("Condition")]
    public async Task<IActionResult> SearchConditions([FromQuery] string patient)
    {
        try
        {
            if (string.IsNullOrEmpty(patient) || !Guid.TryParse(patient, out var patientId))
                return BadRequest("Invalid patient ID");

            var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
            if (record == null)
                return NotFound();

            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Entry = new List<Bundle.EntryComponent>()
            };

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

            return Ok(SerializeResource(bundle));
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to search conditions: {ex.Message}");
        }
    }

    [HttpGet("MedicationRequest")]
    public async Task<IActionResult> SearchMedicationRequests([FromQuery] string patient)
    {
        try
        {
            if (string.IsNullOrEmpty(patient) || !Guid.TryParse(patient, out var patientId))
                return BadRequest("Invalid patient ID");

            var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
            if (record == null)
                return NotFound();

            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Entry = new List<Bundle.EntryComponent>()
            };

            foreach (var medication in record.Medications)
            {
                var medRequest = new MedicationRequest
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

            return Ok(SerializeResource(bundle));
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to search medication requests: {ex.Message}");
        }
    }

    [HttpGet("AllergyIntolerance")]
    public async Task<IActionResult> SearchAllergyIntolerances([FromQuery] string patient)
    {
        try
        {
            if (string.IsNullOrEmpty(patient) || !Guid.TryParse(patient, out var patientId))
                return BadRequest("Invalid patient ID");

            var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
            if (record == null)
                return NotFound();

            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Entry = new List<Bundle.EntryComponent>()
            };

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
                                allergy.ClinicalStatus,
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

            return Ok(SerializeResource(bundle));
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to search allergy intolerances: {ex.Message}");
        }
    }

    [HttpGet("Immunization")]
    public async Task<IActionResult> SearchImmunizations([FromQuery] string patient)
    {
        try
        {
            if (string.IsNullOrEmpty(patient) || !Guid.TryParse(patient, out var patientId))
                return BadRequest("Invalid patient ID");

            var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
            if (record == null)
                return NotFound();

            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Entry = new List<Bundle.EntryComponent>()
            };

            foreach (var immunization in record.Immunizations)
            {
                var fhirImmunization = new FhirImmunization
                {
                    Id = immunization.Id.ToString(),
                    Status = ImmunizationStatus.Completed,
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

            return Ok(SerializeResource(bundle));
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to search immunizations: {ex.Message}");
        }
    }

    [HttpGet("Procedure")]
    public async Task<IActionResult> SearchProcedures([FromQuery] string patient)
    {
        try
        {
            if (string.IsNullOrEmpty(patient) || !Guid.TryParse(patient, out var patientId))
                return BadRequest("Invalid patient ID");

            var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
            if (record == null)
                return NotFound();

            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Entry = new List<Bundle.EntryComponent>()
            };

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

            return Ok(SerializeResource(bundle));
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to search procedures: {ex.Message}");
        }
    }

    [HttpGet("Observation")]
    public async Task<IActionResult> SearchObservations([FromQuery] string patient)
    {
        try
        {
            if (string.IsNullOrEmpty(patient) || !Guid.TryParse(patient, out var patientId))
                return BadRequest("Invalid patient ID");

            var record = await _patientRecordService.GetRecordByPatientIdAsync(patientId);
            if (record == null)
                return NotFound();

            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Entry = new List<Bundle.EntryComponent>()
            };

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

            return Ok(SerializeResource(bundle));
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to search observations: {ex.Message}");
        }
    }

    private string SerializeResource(Resource resource)
    {
        var serializer = new FhirJsonSerializer();
        return serializer.SerializeToString(resource);
    }
}