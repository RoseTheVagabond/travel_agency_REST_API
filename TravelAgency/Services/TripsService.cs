using Microsoft.Data.SqlClient;
using TravelAgency.DTOs;
using TravelAgency.Repositories;

namespace TravelAgency.Services;

public class TripsService : ITripsService
{
    private readonly TripsRepository _tripsRepository;

    public TripsService(TripsRepository tripsRepository)
    {
        _tripsRepository = tripsRepository;
    }
    
    public async Task<List<TripDTO>> GetTrips()
    {
        return await _tripsRepository.GetTrips();
    }
}