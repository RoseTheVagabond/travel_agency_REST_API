using TravelAgency.DTOs;

namespace TravelAgency.Services;

public interface IClientsService
{
    Task<bool> DoesClientExist(int clientId, CancellationToken cancellationToken);
    Task<List<ClientTripDTO>> GetClientTrips(int clientId, CancellationToken cancellationToken);
    Task<int> CreateClient(ClientDTO client, CancellationToken cancellationToken);
    Task<ClientTripRegistrationResult> RegisterClientForTrip(int clientId, int tripId, CancellationToken cancellationToken);
    Task<ClientTripRemovalResult> RemoveClientFromTrip(int clientId, int tripId, CancellationToken cancellationToken);
}