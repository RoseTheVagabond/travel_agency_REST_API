using Microsoft.AspNetCore.Mvc;
using TravelAgency.DTOs;
using TravelAgency.Services;

namespace TravelAgency.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _clientsService;

    public ClientsController(IClientsService clientsService)
    {
        _clientsService = clientsService;
    }

    // Displays all trips for which the client with a specified id is signed up
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest("Invalid client ID");

        var clientExists = await _clientsService.DoesClientExist(id, cancellationToken);
        if (!clientExists)
            return NotFound($"Client with ID {id} not found");

        var trips = await _clientsService.GetClientTrips(id, cancellationToken);
        return Ok(trips);
    }

    // adds a new client to the database with validation of the data passed by the user
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] ClientDTO client, CancellationToken cancellationToken)
    {
        if (client == null)
            return BadRequest("Client data is required");

        if (string.IsNullOrEmpty(client.FirstName))
            return BadRequest("First name is required");

        if (string.IsNullOrEmpty(client.LastName))
            return BadRequest("Last name is required");

        if (string.IsNullOrEmpty(client.Email))
            return BadRequest("Email is required");

        if (string.IsNullOrEmpty(client.Telephone))
            return BadRequest("Telephone is required");

        if (string.IsNullOrEmpty(client.Pesel))
            return BadRequest("PESEL is required");
        
        if (client.Pesel.Length != 11 || !client.Pesel.All(char.IsDigit))
            return BadRequest("PESEL must be 11 digits");
        
        if (!client.Email.Contains('@') || !client.Email.Contains('.'))
            return BadRequest("Invalid email format");

        var clientId = await _clientsService.CreateClient(client, cancellationToken);
        return CreatedAtAction(nameof(GetClientTrips), new { id = clientId }, new { Id = clientId });
    }

    // registers a client for a trip using enums to determine status codes, which should be returned
    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientForTrip(int id, int tripId, CancellationToken cancellationToken)
    {
        if (id <= 0 || tripId <= 0)
            return BadRequest("Invalid client or trip ID");

        var result = await _clientsService.RegisterClientForTrip(id, tripId, cancellationToken);
        
        return result switch
        {
            ClientTripRegistrationResult.Success => Ok("Client successfully registered for the trip"),
            ClientTripRegistrationResult.ClientNotFound => NotFound($"Client with ID {id} not found"),
            ClientTripRegistrationResult.TripNotFound => NotFound($"Trip with ID {tripId} not found"),
            ClientTripRegistrationResult.TripFull => BadRequest("The trip has reached its maximum number of participants"),
            ClientTripRegistrationResult.AlreadyRegistered => BadRequest("Client is already registered for this trip"),
            _ => StatusCode(500, "An error occurred while processing your request")
        };
    }

    // removes a specified client from the specified trip, uses enums to simplify determining the status codes
    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> RemoveClientFromTrip(int id, int tripId, CancellationToken cancellationToken)
    {
        if (id <= 0 || tripId <= 0)
            return BadRequest("Invalid client or trip ID");

        var result = await _clientsService.RemoveClientFromTrip(id, tripId, cancellationToken);
        
        return result switch
        {
            ClientTripRemovalResult.Success => Ok("Client successfully removed from the trip"),
            ClientTripRemovalResult.RegistrationNotFound => NotFound("Client is not registered for this trip"),
            _ => StatusCode(500, "An error occurred while processing your request")
        };
    }
}