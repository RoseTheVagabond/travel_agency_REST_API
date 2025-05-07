using Microsoft.Data.SqlClient;
using System.Data;
using TravelAgency.DTOs;

namespace TravelAgency.Repositories;

public class ClientsRepository : IClientsRepository
{
    private readonly string _connectionString;

    public ClientsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<bool> DoesClientExist(int clientId, CancellationToken cancellationToken)
    {
        const string commandText = "SELECT 1 FROM Client WHERE IdClient = @ClientId";
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(commandText, connection);
        
        command.Parameters.AddWithValue("@ClientId", clientId);
        
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        
        return result != null;
    }

    public async Task<List<ClientTripDTO>> GetClientTrips(int clientId, CancellationToken cancellationToken)
    {
        var clientTrips = new List<ClientTripDTO>();
        
        const string commandText = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, 
                   ct.RegisteredAt, ct.PaymentDate
            FROM Trip t
            JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
            WHERE ct.IdClient = @ClientId";
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(commandText, connection);
        
        command.Parameters.AddWithValue("@ClientId", clientId);
        
        await connection.OpenAsync(cancellationToken);
        
        using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        
        while (await reader.ReadAsync(cancellationToken))
        {
            clientTrips.Add(new ClientTripDTO
            {
                Trip = new TripDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                    DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                    MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople"))
                },
                RegisteredAt = ConvertToDateTime(reader.GetInt32(reader.GetOrdinal("RegisteredAt"))),
                PaymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate"))
                    ? (DateTime?)null 
                    : ConvertToDateTime(reader.GetInt32(reader.GetOrdinal("PaymentDate")))
            });
        }
        
        // Get countries for each trip
        foreach (var clientTrip in clientTrips)
        {
            clientTrip.Trip.Countries = await GetTripCountries(clientTrip.Trip.Id, connection, cancellationToken);
        }
        
        return clientTrips;
    }

    private async Task<List<CountryDTO>> GetTripCountries(int tripId, SqlConnection existingConnection, CancellationToken cancellationToken)
    {
        var countries = new List<CountryDTO>();
        
        const string commandText = @"
            SELECT c.IdCountry, c.Name
            FROM Country c
            JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry
            WHERE ct.IdTrip = @TripId";
        
        using SqlCommand command = new SqlCommand(commandText, existingConnection);
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

    public async Task<int> CreateClient(ClientDTO client, CancellationToken cancellationToken)
    {
        const string commandText = @"
            INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
            OUTPUT INSERTED.IdClient
            VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(commandText, connection);
        
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);
        
        await connection.OpenAsync(cancellationToken);
        
        var clientId = (int)await command.ExecuteScalarAsync(cancellationToken);
        return clientId;
    }

    public async Task<bool> IsClientRegisteredForTrip(int clientId, int tripId, CancellationToken cancellationToken)
    {
        const string commandText = @"
            SELECT 1 FROM Client_Trip 
            WHERE IdClient = @ClientId AND IdTrip = @TripId";
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(commandText, connection);
        
        command.Parameters.AddWithValue("@ClientId", clientId);
        command.Parameters.AddWithValue("@TripId", tripId);
        
        await connection.OpenAsync(cancellationToken);
        
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result != null;
    }

    public async Task<bool> RegisterClientForTrip(int clientId, int tripId, CancellationToken cancellationToken)
    {
        const string commandText = @"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
            VALUES (@ClientId, @TripId, @RegisteredAt, NULL)";
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(commandText, connection);
        
        command.Parameters.AddWithValue("@ClientId", clientId);
        command.Parameters.AddWithValue("@TripId", tripId);
        command.Parameters.AddWithValue("@RegisteredAt", DateTime.Now);
        
        await connection.OpenAsync(cancellationToken);
        
        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    public async Task<bool> RemoveClientFromTrip(int clientId, int tripId, CancellationToken cancellationToken)
    {
        const string commandText = @"
            DELETE FROM Client_Trip 
            WHERE IdClient = @ClientId AND IdTrip = @TripId";
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(commandText, connection);
        
        command.Parameters.AddWithValue("@ClientId", clientId);
        command.Parameters.AddWithValue("@TripId", tripId);
        
        await connection.OpenAsync(cancellationToken);
        
        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }
    
    //Used to handle PaymentDate and RegisteredAt conversion to DateTime
    private DateTime ConvertToDateTime(int dateInt)
    {
        int year = dateInt / 10000;
        int month = (dateInt % 10000) / 100;
        int day = dateInt % 100;
    
        return new DateTime(year, month, day);
    }
}