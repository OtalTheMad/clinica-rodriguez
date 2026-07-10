using ClinicaRodriguez.Modelos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicaRodriguez.Repositorios
{
    public class CitaRepositorio
    {
        private readonly string _connectionString;

        public CitaRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Cita>> ObtenerPorEspecialistaYRangoAsync(
            int especialistaId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            const string query = @"
                SELECT
                    c.ID,
                    c.PacienteID,
                    c.EspecialistaID,
                    c.FechaCita,
                    c.Estado,
                    c.Duracion,
                    c.Notas,
                    c.CreadoEn,
                    c.CreadoPor,
                    p.Nombre AS NombrePaciente,
                    p.Apellido AS ApellidoPaciente,
                    e.Nombre AS NombreEspecialista,
                    e.Apellido AS ApellidoEspecialista
                FROM dbo.Citas c
                INNER JOIN dbo.Pacientes p ON p.ID = c.PacienteID
                INNER JOIN dbo.Especialistas e ON e.ID = c.EspecialistaID
                WHERE c.EspecialistaID = @EspecialistaID
                  AND c.FechaCita >= @FechaInicio
                  AND c.FechaCita < @FechaFin
                ORDER BY c.FechaCita;";

            var citas = new List<Cita>();

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@EspecialistaID", especialistaId);
            comando.Parameters.AddWithValue("@FechaInicio", fechaInicio);
            comando.Parameters.AddWithValue("@FechaFin", fechaFin);

            await conexion.OpenAsync();

            using var reader = await comando.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                citas.Add(MapearCita(reader));
            }

            return citas;
        }

        public async Task<List<Cita>> ObtenerPorPacienteIdAsync(int pacienteId)
        {
            const string query = @"
                SELECT
                    c.ID,
                    c.PacienteID,
                    c.EspecialistaID,
                    c.FechaCita,
                    c.Estado,
                    c.Duracion,
                    c.Notas,
                    c.CreadoEn,
                    c.CreadoPor,
                    p.Nombre AS NombrePaciente,
                    p.Apellido AS ApellidoPaciente,
                    e.Nombre AS NombreEspecialista,
                    e.Apellido AS ApellidoEspecialista
                FROM dbo.Citas c
                INNER JOIN dbo.Pacientes p ON p.ID = c.PacienteID
                INNER JOIN dbo.Especialistas e ON e.ID = c.EspecialistaID
                WHERE c.PacienteID = @PacienteID
                ORDER BY c.FechaCita DESC;";

            var citas = new List<Cita>();

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@PacienteID", pacienteId);

            await conexion.OpenAsync();

            using var reader = await comando.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                citas.Add(MapearCita(reader));
            }

            return citas;
        }

        public async Task<int> CrearAsync(Cita cita)
        {
            const string query = @"
                INSERT INTO dbo.Citas
                (
                    PacienteID,
                    EspecialistaID,
                    FechaCita,
                    Estado,
                    Duracion,
                    Notas,
                    CreadoEn,
                    CreadoPor
                )
                OUTPUT INSERTED.ID
                VALUES
                (
                    @PacienteID,
                    @EspecialistaID,
                    @FechaCita,
                    @Estado,
                    @Duracion,
                    @Notas,
                    SYSDATETIME(),
                    @CreadoPor
                );";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@PacienteID", cita.PacienteId);
            comando.Parameters.AddWithValue("@EspecialistaID", cita.EspecialistaId);
            comando.Parameters.AddWithValue("@FechaCita", cita.FechaCita);
            comando.Parameters.AddWithValue("@Estado", ValorONull(cita.Estado));
            comando.Parameters.AddWithValue("@Duracion", cita.Duracion);
            comando.Parameters.AddWithValue("@Notas", ValorONull(cita.Notas));
            comando.Parameters.AddWithValue("@CreadoPor", cita.CreadoPor);

            await conexion.OpenAsync();

            var idGenerado = await comando.ExecuteScalarAsync();

            return Convert.ToInt32(idGenerado);
        }

        public async Task ActualizarEstadoYNotasAsync(int citaId, string estado, string notas)
        {
            const string query = @"
                UPDATE dbo.Citas
                SET
                    Estado = @Estado,
                    Notas = @Notas
                WHERE ID = @ID;";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@ID", citaId);
            comando.Parameters.AddWithValue("@Estado", ValorONull(estado));
            comando.Parameters.AddWithValue("@Notas", ValorONull(notas));

            await conexion.OpenAsync();
            await comando.ExecuteNonQueryAsync();
        }

        private static Cita MapearCita(SqlDataReader reader)
        {
            return new Cita
            {
                Id = ObtenerInt(reader, "ID"),
                PacienteId = ObtenerInt(reader, "PacienteID"),
                EspecialistaId = ObtenerInt(reader, "EspecialistaID"),
                FechaCita = ObtenerDateTime(reader, "FechaCita"),
                Estado = ObtenerString(reader, "Estado"),
                Duracion = ObtenerInt(reader, "Duracion"),
                Notas = ObtenerString(reader, "Notas"),
                CreadoEn = ObtenerDateTime(reader, "CreadoEn"),
                CreadoPor = ObtenerInt(reader, "CreadoPor"),
                NombrePaciente = ObtenerString(reader, "NombrePaciente"),
                ApellidoPaciente = ObtenerString(reader, "ApellidoPaciente"),
                NombreEspecialista = ObtenerString(reader, "NombreEspecialista"),
                ApellidoEspecialista = ObtenerString(reader, "ApellidoEspecialista")
            };
        }

        private static object ValorONull(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
        }

        private static int ObtenerInt(SqlDataReader reader, string columna)
        {
            int ordinal = reader.GetOrdinal(columna);
            return reader.GetInt32(ordinal);
        }

        private static string ObtenerString(SqlDataReader reader, string columna)
        {
            int ordinal = reader.GetOrdinal(columna);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }

        private static DateTime ObtenerDateTime(SqlDataReader reader, string columna)
        {
            int ordinal = reader.GetOrdinal(columna);
            return reader.GetDateTime(ordinal);
        }
    }
}