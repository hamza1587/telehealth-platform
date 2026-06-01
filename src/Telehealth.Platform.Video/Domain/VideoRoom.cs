namespace Telehealth.Platform.Video.Domain;

public enum RoomStatus
{
    Created,
    Active,
    Ended,
    Archived
}

public class VideoRoom
{
    public Guid Id { get; private set; }
    public string RoomName { get; private set; } = string.Empty;
    public int MaxParticipants { get; private set; }
    public bool IsRecording { get; private set; }
    public bool HasScreenSharing { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public RoomStatus Status { get; private set; }
    public string? TwilioRoomSid { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public int CurrentParticipants { get; private set; }
    public string CountryCode { get; private set; } = "US";

    public VideoRoom()
    {
    }

    public VideoRoom(string roomName, int maxParticipants, string countryCode = "US")
    {
        Id = Guid.NewGuid();
        RoomName = roomName;
        MaxParticipants = maxParticipants;
        CountryCode = countryCode;
        Status = RoomStatus.Created;
        CreatedAt = DateTimeOffset.UtcNow;
        CurrentParticipants = 0;
        IsRecording = false;
        HasScreenSharing = false;
    }

    public void Activate(string twilioRoomSid)
    {
        Status = RoomStatus.Active;
        TwilioRoomSid = twilioRoomSid;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void End()
    {
        Status = RoomStatus.Ended;
        EndedAt = DateTimeOffset.UtcNow;
        CurrentParticipants = 0;
    }

    public void EnableRecording()
    {
        IsRecording = true;
    }

    public void DisableRecording()
    {
        IsRecording = false;
    }

    public void EnableScreenSharing()
    {
        HasScreenSharing = true;
    }

    public void DisableScreenSharing()
    {
        HasScreenSharing = false;
    }

    public void AddParticipant()
    {
        if (CurrentParticipants < MaxParticipants)
        {
            CurrentParticipants++;
        }
    }

    public void RemoveParticipant()
    {
        if (CurrentParticipants > 0)
        {
            CurrentParticipants--;
        }
    }
}
