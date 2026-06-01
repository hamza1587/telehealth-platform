using Telehealth.Platform.Video.Domain;

namespace Telehealth.Platform.Video.Services;

public interface IRecordingService
{
    Task<RecordingSession> StartRecordingAsync(Guid roomId, bool requiresConsent);
    Task<RecordingSession?> GetRecordingSessionAsync(Guid recordingId);
    Task<RecordingSession> StopRecordingAsync(Guid recordingId);
    Task<RecordingSession> GrantConsentAsync(Guid recordingId);
}
