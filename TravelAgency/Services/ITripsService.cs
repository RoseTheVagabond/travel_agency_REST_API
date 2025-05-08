using TravelAgency.DTOs;

namespace TravelAgency.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips(CancellationToken cancellationToken);
    
}