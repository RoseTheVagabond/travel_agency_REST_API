using TravelAgency.DTOs;

namespace TravelAgency.Repositories;

public interface ITripsRepository
{
    Task<List<TripDTO>> GetTrips();
}