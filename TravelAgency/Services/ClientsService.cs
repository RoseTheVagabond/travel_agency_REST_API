using TravelAgency.DTOs;
using TravelAgency.Repositories;

namespace TravelAgency.Services;

public enum ClientTripRegistrationResult
{
    Success,
    ClientNotFound,
    TripNotFound,
    TripFull,
    AlreadyRegistered,
    Error
}

public enum ClientTripRemovalResult
{
    Success,
    RegistrationNotFound,
    Error
}

public class ClientsService : IClientsService
{
    private readonly IClientsRepository _clientsRepository;
    private readonly ITripsRepository _tripsRepository;

    public ClientsService(IClientsRepository clientsRepository, ITripsRepository tripsRepository)
    {
        _clientsRepository = clientsRepository;
        _tripsRepository = tripsRepository;
    }

    public async Task<bool> DoesClientExist(int clientId, CancellationToken cancellationToken)
    {
        return await _clientsRepository.DoesClientExist(clientId, cancellationToken);
    }

    public async Task<List<ClientTripDTO>> GetClientTrips(int clientId, CancellationToken cancellationToken)
    {
        return await _clientsRepository.GetClientTrips(clientId, cancellationToken);
    }

    public async Task<int> CreateClient(ClientDTO client, CancellationToken cancellationToken)
    {
        return await _clientsRepository.CreateClient(client, cancellationToken);
    }

    public async Task<ClientTripRegistrationResult> RegisterClientForTrip(int clientId, int tripId, CancellationToken cancellationToken)
    {
        // Check if client exists
        var clientExists = await _clientsRepository.DoesClientExist(clientId, cancellationToken);
        if (!clientExists)
            return ClientTripRegistrationResult.ClientNotFound;

        // Check if trip exists
        var tripExists = await _tripsRepository.DoesTripExist(tripId, cancellationToken);
        if (!tripExists)
            return ClientTripRegistrationResult.TripNotFound;

        // Check if client is already registered for this trip
        var isRegistered = await _clientsRepository.IsClientRegisteredForTrip(clientId, tripId, cancellationToken);
        if (isRegistered)
            return ClientTripRegistrationResult.AlreadyRegistered;

        // Check if the trip is full
        var isTripFull = await _tripsRepository.IsTripFull(tripId, cancellationToken);
        if (isTripFull)
            return ClientTripRegistrationResult.TripFull;

        // Register client for the trip
        var success = await _clientsRepository.RegisterClientForTrip(clientId, tripId, cancellationToken);
        return success ? ClientTripRegistrationResult.Success : ClientTripRegistrationResult.Error;
    }

    public async Task<ClientTripRemovalResult> RemoveClientFromTrip(int clientId, int tripId, CancellationToken cancellationToken)
    {
        // Check if client is registered for this trip
        var isRegistered = await _clientsRepository.IsClientRegisteredForTrip(clientId, tripId, cancellationToken);
        if (!isRegistered)
            return ClientTripRemovalResult.RegistrationNotFound;

        // Remove client from the trip
        var success = await _clientsRepository.RemoveClientFromTrip(clientId, tripId, cancellationToken);
        return success ? ClientTripRemovalResult.Success : ClientTripRemovalResult.Error;
    }
}