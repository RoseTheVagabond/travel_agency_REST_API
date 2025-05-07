using Microsoft.AspNetCore.Mvc;
using TravelAgency.Services;

namespace TravelAgency.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly ITripsService _tripsService;

    public TripsController(ITripsService tripsService)
    {
        _tripsService = tripsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips(CancellationToken cancellationToken)
    {
        var trips = await _tripsService.GetTrips(cancellationToken);
        return Ok(trips);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrip(int id, CancellationToken cancellationToken)
    {
        if (!await _tripsService.DoesTripExist(id, cancellationToken))
        {
            return NotFound($"Trip with ID {id} not found");
        }
        
        var trip = await _tripsService.GetTrip(id, cancellationToken);
        return Ok(trip);
    }
}