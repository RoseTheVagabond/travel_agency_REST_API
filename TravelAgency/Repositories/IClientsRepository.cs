using TravelAgency.DTOs;

namespace TravelAgency.Repositories;

public interface IClientsRepository
{
    Task<bool> DoesClientExist(int clientId, CancellationToken cancellationToken);
    Task<List<ClientTripDTO>> GetClientTrips(int clientId, CancellationToken cancellationToken);
    Task<int> CreateClient(ClientDTO client, CancellationToken cancellationToken);
    Task<bool> IsClientRegisteredForTrip(int clientId, int tripId, CancellationToken cancellationToken);
    Task<bool> RegisterClientForTrip(int clientId, int tripId, CancellationToken cancellationToken);
    Task<bool> RemoveClientFromTrip(int clientId, int tripId, CancellationToken cancellationToken);
}