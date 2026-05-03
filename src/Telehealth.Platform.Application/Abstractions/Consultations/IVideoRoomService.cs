using Telehealth.Platform.Domain.Consultations;

namespace Telehealth.Platform.Application.Abstractions.Consultations;

public interface IVideoRoomService
{
    Task<VideoRoom?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<VideoRoom?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VideoRoom> CreateAsync(Guid bookingId, string roomId, string createdBy, CancellationToken cancellationToken = default);
    Task<VideoRoom?> MarkJoinedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VideoRoom?> EndAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<VideoRoom>> GetActiveRoomsAsync(CancellationToken cancellationToken = default);
}