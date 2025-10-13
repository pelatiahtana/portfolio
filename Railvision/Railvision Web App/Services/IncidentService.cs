using System.Data;
using System.Data.SqlClient;
using TrainGenie.Models;

namespace TrainGenie.Services
{
    public class IncidentService
    {
        private readonly string _connectionString;

        public IncidentService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Default");
        }

        public async Task CreateIncident(Incident incident)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new SqlCommand(
                "INSERT INTO Incidents (Title, Description, Location, Severity, ReportedBy) " +
                "VALUES (@Title, @Description, @Location, @Severity, @ReportedBy); " +
                "SELECT SCOPE_IDENTITY();", connection);

            cmd.Parameters.AddWithValue("@Title", incident.Title);
            cmd.Parameters.AddWithValue("@Description", incident.Description);
            cmd.Parameters.AddWithValue("@Location", incident.Location ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Severity", incident.Severity);
            cmd.Parameters.AddWithValue("@ReportedBy", incident.ReportedBy);

            incident.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task UpdateIncident(Incident incident)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new SqlCommand(
                "UPDATE Incidents SET Status = @Status, AdminNotes = @AdminNotes, " +
                "ResolvedTime = CASE WHEN @Status = 'Resolved' THEN GETDATE() ELSE ResolvedTime END " +
                "WHERE Id = @Id", connection);

            cmd.Parameters.AddWithValue("@Id", incident.Id);
            cmd.Parameters.AddWithValue("@Status", incident.Status);
            cmd.Parameters.AddWithValue("@AdminNotes", incident.AdminNotes ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ClearAllIncidents()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var cmd = new SqlCommand("DELETE FROM Incidents", connection);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Incident>> GetAllIncidents()
        {
            var incidents = new List<Incident>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new SqlCommand("SELECT * FROM Incidents ORDER BY ReportTime DESC", connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                incidents.Add(new Incident
                {
                    Id = reader.GetInt32("Id"),
                    Title = reader.GetString("Title"),
                    Description = reader.GetString("Description"),
                    Location = reader.IsDBNull("Location") ? null : reader.GetString("Location"),
                    Severity = reader.GetString("Severity"),
                    Status = reader.GetString("Status"),
                    ReportedBy = reader.GetString("ReportedBy"),
                    ReportTime = reader.GetDateTime("ReportTime"),
                    ResolvedTime = reader.IsDBNull("ResolvedTime") ? null : reader.GetDateTime("ResolvedTime"),
                    AdminNotes = reader.IsDBNull("AdminNotes") ? null : reader.GetString("AdminNotes")
                });
            }

            return incidents;
        }
    }
}