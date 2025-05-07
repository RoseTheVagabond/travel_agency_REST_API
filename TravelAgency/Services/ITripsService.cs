using TravelAgency.DTOs;

namespace TravelAgency.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips(CancellationToken cancellationToken);
    Task<TripDTO> GetTrip(int tripId, CancellationToken cancellationToken);
    Task<bool> DoesTripExist(int tripId, CancellationToken cancellationToken);
}