using Microsoft.Data.SqlClient;
using System.Data;
using TravelAgency.DTOs;

namespace TravelAgency.Repositories;

public class TripsRepository : ITripsRepository
{
    private readonly string _connectionString;

    public TripsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public async Task<List<TripDTO>> GetTrips(CancellationToken cancellationToken)
    {
        var trips = new List<TripDTO>();

        // returns all information about all trips
        const string commandText = @"
            SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople 
            FROM Trip
            ORDER BY DateFrom";
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        using (SqlCommand command = new SqlCommand(commandText, connection))
        using (SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var trip = new TripDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                    DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                    MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                    Countries = new List<CountryDTO>()
                };
                
                trips.Add(trip);
            }
        }
        
        // gets countries for each trip using a separate connection
        foreach (var trip in trips)
        {
            trip.Countries = await GetTripCountriesWithNewConnection(trip.Id, cancellationToken);
        }
        return trips;
    }
    
    public async Task<TripDTO> GetTrip(int tripId, CancellationToken cancellationToken)
    {
        TripDTO trip = null;
        
        //returns all information about a specific trip
        const string commandText = @"
            SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople 
            FROM Trip
            WHERE IdTrip = @TripId";
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        using (SqlCommand command = new SqlCommand(commandText, connection))
        {
            command.Parameters.AddWithValue("@TripId", tripId);
            
            using (SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    trip = new TripDTO
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                        DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                        MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                        Countries = new List<CountryDTO>()
                    };
                }
            }
        }
        
        if (trip != null)
        {
            // gets countries using a separate connection
            trip.Countries = await GetTripCountriesWithNewConnection(trip.Id, cancellationToken);
        }
        return trip;
    }
    
    private async Task<List<CountryDTO>> GetTripCountriesWithNewConnection(int tripId, CancellationToken cancellationToken)
    {
        var countries = new List<CountryDTO>();
        
        // fetches all names and id's of countries associated with a specific trip
        const string commandText = @"
            SELECT c.IdCountry, c.Name
            FROM Country c
            JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry
            WHERE ct.IdTrip = @TripId";
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        using SqlCommand command = new SqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@TripId", tripId);
        
        using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        
        while (await reader.ReadAsync(cancellationToken))
        {
            countries.Add(new CountryDTO
            {
                Id = reader.GetInt32(reader.GetOrdinal("IdCountry")),
                Name = reader.GetString(reader.GetOrdinal("Name"))
            });
        }
        return countries;
    }
    
    public async Task<bool> DoesTripExist(int tripId, CancellationToken cancellationToken)
    {
        // checks if the trip with a specific Id exists in the database
        const string commandText = "SELECT 1 FROM Trip WHERE IdTrip = @TripId";
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(commandText, connection);
        
        command.Parameters.AddWithValue("@TripId", tripId);
        
        await connection.OpenAsync(cancellationToken);
        
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result != null;
    }
    
    public async Task<bool> IsTripFull(int tripId, CancellationToken cancellationToken)
    {
        // checks is the number of clients assigned to the trip is not greater or equal to the maximum allowed number of people on it
        const string commandText = @"
            SELECT 
                CASE 
                    WHEN COUNT(ct.IdClient) >= t.MaxPeople THEN 1 
                    ELSE 0 
                END AS IsFull
            FROM Trip t
            LEFT JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
            WHERE t.IdTrip = @TripId
            GROUP BY t.MaxPeople";
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(commandText, connection);
        
        command.Parameters.AddWithValue("@TripId", tripId);
        
        await connection.OpenAsync(cancellationToken);
        
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result != null && (int)result == 1;
    }
}