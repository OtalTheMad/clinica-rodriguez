using ClinicaRodriguez.Modelos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicaRodriguez.Repositorios
{
    public class NotaClinicaRepositorio
    {
        private readonly string _connectionString;

        public NotaClinicaRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CrearAsync(NotaClinica nota)
        {
            const string query = @"
                INSERT INTO dbo.NotasClinicas
                (
                    ExpedienteID,
                    CitaID,
                    FechaNota,
                    MotivoConsulta,
                    Diagnostico,
                    Tratamiento,
                    Indicaciones,
                    CreadoEn,
                    CreadoPor
                )
                OUTPUT INSERTED.ID
                VALUES
                (
                    @ExpedienteID,
                    @CitaID,
                    @FechaNota,
                    @MotivoConsulta,
                    @Diagnostico,
                    @Tratamiento,
                    @Indicaciones,
                    SYSDATETIME(),
                    @CreadoPor
                );";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue(
                "@ExpedienteID",
                nota.ExpedienteId
            );

            comando.Parameters.AddWithValue(
                "@CitaID",
                nota.CitaId.HasValue
                    ? nota.CitaId.Value
                    : DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@FechaNota",
                nota.FechaNota
            );

            comando.Parameters.AddWithValue(
                "@MotivoConsulta",
                nota.MotivoConsulta.Trim()
            );

            comando.Parameters.AddWithValue(
                "@Diagnostico",
                ValorONull(nota.Diagnostico)
            );

            comando.Parameters.AddWithValue(
                "@Tratamiento",
                ValorONull(nota.Tratamiento)
            );

            comando.Parameters.AddWithValue(
                "@Indicaciones",
                ValorONull(nota.Indicaciones)
            );

            comando.Parameters.AddWithValue(
                "@CreadoPor",
                nota.CreadoPor
            );

            await conexion.OpenAsync();

            var idGenerado = await comando.ExecuteScalarAsync();

            return Convert.ToInt32(idGenerado);
        }

        public async Task<List<NotaClinica>> ObtenerPorExpedienteIdAsync(
            int expedienteId)
        {
            const string query = @"
                SELECT
                    ID,
                    ExpedienteID,
                    CitaID,
                    FechaNota,
                    MotivoConsulta,
                    Diagnostico,
                    Tratamiento,
                    Indicaciones,
                    CreadoEn,
                    CreadoPor
                FROM dbo.NotasClinicas
                WHERE ExpedienteID = @ExpedienteID
                ORDER BY FechaNota DESC, ID DESC;";

            var notas = new List<NotaClinica>();

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue(
                "@ExpedienteID",
                expedienteId
            );

            await conexion.OpenAsync();

            using var reader = await comando.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                notas.Add(MapearNota(reader));
            }

            return notas;
        }

        private static NotaClinica MapearNota(SqlDataReader reader)
        {
            return new NotaClinica
            {
                Id = ObtenerInt(reader, "ID"),

                ExpedienteId = ObtenerInt(
                    reader,
                    "ExpedienteID"
                ),

                CitaId = ObtenerIntNullable(
                    reader,
                    "CitaID"
                ),

                FechaNota = ObtenerDateTime(
                    reader,
                    "FechaNota"
                ),

                MotivoConsulta = ObtenerString(
                    reader,
                    "MotivoConsulta"
                ),

                Diagnostico = ObtenerString(
                    reader,
                    "Diagnostico"
                ),

                Tratamiento = ObtenerString(
                    reader,
                    "Tratamiento"
                ),

                Indicaciones = ObtenerString(
                    reader,
                    "Indicaciones"
                ),

                CreadoEn = ObtenerDateTime(
                    reader,
                    "CreadoEn"
                ),

                CreadoPor = ObtenerInt(
                    reader,
                    "CreadoPor"
                )
            };
        }

        private static string ObtenerString(
            SqlDataReader reader,
            string columna)
        {
            int posicion = reader.GetOrdinal(columna);

            return reader.IsDBNull(posicion)
                ? string.Empty
                : reader.GetString(posicion);
        }

        private static int ObtenerInt(
            SqlDataReader reader,
            string columna)
        {
            int posicion = reader.GetOrdinal(columna);

            return reader.GetInt32(posicion);
        }

        private static int? ObtenerIntNullable(
            SqlDataReader reader,
            string columna)
        {
            int posicion = reader.GetOrdinal(columna);

            return reader.IsDBNull(posicion)
                ? null
                : reader.GetInt32(posicion);
        }

        private static DateTime ObtenerDateTime(
            SqlDataReader reader,
            string columna)
        {
            int posicion = reader.GetOrdinal(columna);

            return reader.GetDateTime(posicion);
        }

        private static object ValorONull(string valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? DBNull.Value
                : valor.Trim();
        }
    }
}