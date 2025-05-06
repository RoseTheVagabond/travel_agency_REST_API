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
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _tripsService.GetTrips();
        return Ok(trips);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrip(int id)
    {
        // if( await DoesTripExist(id)){
        //  return NotFound();
        // }
        // var trip = ... GetTrip(id);
        return Ok();
    }
}