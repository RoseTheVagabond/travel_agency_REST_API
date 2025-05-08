using TravelAgency.DTOs;
using TravelAgency.Repositories;

namespace TravelAgency.Services;

public class TripsService : ITripsService
{
    private readonly ITripsRepository _tripsRepository;

    public TripsService(ITripsRepository tripsRepository)
    {
        _tripsRepository = tripsRepository;
    }
    
    public async Task<List<TripDTO>> GetTrips(CancellationToken cancellationToken)
    {
        return await _tripsRepository.GetTrips(cancellationToken);
    }
}