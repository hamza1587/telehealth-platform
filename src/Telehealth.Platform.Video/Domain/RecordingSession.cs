namespace Telehealth.Platform.Video.Domain;

public enum RecordingStatus
{
    Pending,
    Recording,
    Processing,
    Completed,
    Failed
}

public class RecordingSession
{
    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public string RecordingUrl { get; private set; } = string.Empty;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public RecordingStatus Status { get; private set; }
    public string? TwilioRecordingSid { get; private set; }
    public long DurationSeconds { get; private set; }
    public string? TranscriptionUrl { get; private set; }
    public bool RequiresConsent { get; private set; }
    public bool ConsentObtained { get; private set; }

    public RecordingSession()
    {
    }

    public RecordingSession(Guid roomId, bool requiresConsent)
    {
        Id = Guid.NewGuid();
        RoomId = roomId;
        RequiresConsent = requiresConsent;
        ConsentObtained = !requiresConsent;
        Status = RecordingStatus.Pending;
        StartedAt = DateTimeOffset.UtcNow;
        RecordingUrl = string.Empty;
        DurationSeconds = 0;
    }

    public void StartRecording(string twilioRecordingSid)
    {
        Status = RecordingStatus.Recording;
        TwilioRecordingSid = twilioRecordingSid;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void EndRecording()
    {
        Status = RecordingStatus.Processing;
        EndedAt = DateTimeOffset.UtcNow;
        DurationSeconds = (long)(EndedAt.Value - StartedAt).TotalSeconds;
    }

    public void CompleteRecording(string recordingUrl)
    {
        Status = RecordingStatus.Completed;
        RecordingUrl = recordingUrl;
    }

    public void FailRecording()
    {
        Status = RecordingStatus.Failed;
        EndedAt = DateTimeOffset.UtcNow;
    }

    public void SetTranscription(string transcriptionUrl)
    {
        TranscriptionUrl = transcriptionUrl;
    }

    public void GrantConsent()
    {
        ConsentObtained = true;
    }
}
