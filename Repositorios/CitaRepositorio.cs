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

        public async Task<List<Cita>> ObtenerHistorialPorPacienteIdAsync(int pacienteId)
        {
            const string query = @"
                SELECT
                    ID,
                    PacienteID,
                    EspecialistaID,
                    FechaCita,
                    Estado,
                    Duracion,
                    CreadoEn,
                    CreadoPor
                FROM dbo.Citas
                WHERE PacienteID = @PacienteID
                  AND FechaCita <= SYSDATETIME()
                ORDER BY FechaCita DESC;";

            var citas = new List<Cita>();

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@PacienteID", pacienteId);

            await conexion.OpenAsync();

            using var reader = await comando.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                citas.Add(new Cita
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID")),
                    PacienteId = reader.GetInt32(reader.GetOrdinal("PacienteID")),
                    EspecialistaId = reader.GetInt32(reader.GetOrdinal("EspecialistaID")),
                    FechaCita = reader.GetDateTime(reader.GetOrdinal("FechaCita")),
                    Estado = reader.GetString(reader.GetOrdinal("Estado")),
                    Duracion = reader.GetInt32(reader.GetOrdinal("Duracion")),
                    CreadoEn = reader.GetDateTime(reader.GetOrdinal("CreadoEn")),
                    CreadoPor = reader.GetInt32(reader.GetOrdinal("CreadoPor"))
                });
            }

            return citas;
        }
    }
}