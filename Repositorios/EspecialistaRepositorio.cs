using ClinicaRodriguez.Modelos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicaRodriguez.Repositorios
{
    public class EspecialistaRepositorio
    {
        private readonly string _connectionString;

        public EspecialistaRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Especialista>> ObtenerTodosAsync()
        {
            const string query = @"
                SELECT
                    ID,
                    Nombre,
                    Apellido,
                    Especialidad,
                    GraduadoEn,
                    CreadoEn,
                    CreadoPor,
                    UsuarioID
                FROM dbo.Especialistas
                ORDER BY Nombre, Apellido;";

            var especialistas = new List<Especialista>();

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            await conexion.OpenAsync();

            using var reader = await comando.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                especialistas.Add(MapearEspecialista(reader));
            }

            return especialistas;
        }

        public async Task<Especialista> ObtenerPorUsuarioIdAsync(int usuarioId)
        {
            const string query = @"
                SELECT TOP 1
                    ID,
                    Nombre,
                    Apellido,
                    Especialidad,
                    GraduadoEn,
                    CreadoEn,
                    CreadoPor,
                    UsuarioID
                FROM dbo.Especialistas
                WHERE UsuarioID = @UsuarioID;";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@UsuarioID", usuarioId);

            await conexion.OpenAsync();

            using var reader = await comando.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return MapearEspecialista(reader);

            return null;
        }

        private static Especialista MapearEspecialista(SqlDataReader reader)
        {
            return new Especialista
            {
                Id = ObtenerInt(reader, "ID"),
                Nombre = ObtenerString(reader, "Nombre"),
                Apellido = ObtenerString(reader, "Apellido"),
                Especialidad = ObtenerString(reader, "Especialidad"),
                GraduadoEn = ObtenerString(reader, "GraduadoEn"),
                CreadoEn = ObtenerDateTime(reader, "CreadoEn"),
                CreadoPor = ObtenerInt(reader, "CreadoPor"),
                UsuarioId = ObtenerIntNullable(reader, "UsuarioID")
            };
        }

        private static int ObtenerInt(SqlDataReader reader, string columna)
        {
            int ordinal = reader.GetOrdinal(columna);
            return reader.GetInt32(ordinal);
        }

        private static int? ObtenerIntNullable(SqlDataReader reader, string columna)
        {
            int ordinal = reader.GetOrdinal(columna);
            return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
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