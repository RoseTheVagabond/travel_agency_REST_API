using TravelAgency.DTOs;

namespace TravelAgency.Repositories;

public interface ITripsRepository
{
    Task<List<TripDTO>> GetTrips(CancellationToken cancellationToken);
    Task<bool> DoesTripExist(int tripId, CancellationToken cancellationToken);
    Task<bool> IsTripFull(int tripId, CancellationToken cancellationToken);
}