using ClinicaRodriguez.Modelos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicaRodriguez.Repositorios
{
    public class DocumentoAdjuntoRepositorio
    {
        private readonly string _connectionString;

        public DocumentoAdjuntoRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<DocumentoAdjunto>> ObtenerPorExpedienteIdAsync(int expedienteId)
        {
            const string query = @"
                SELECT
                    ID,
                    ExpedienteID,
                    NombreArchivo,
                    RutaArchivo,
                    TipoArchivo,
                    Descripcion,
                    CreadoEn,
                    CreadoPor
                FROM dbo.DocumentosAdjuntos
                WHERE ExpedienteID = @ExpedienteID
                ORDER BY CreadoEn DESC;";

            var documentos = new List<DocumentoAdjunto>();

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@ExpedienteID", expedienteId);

            await conexion.OpenAsync();

            using var reader = await comando.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                documentos.Add(MapearDocumento(reader));
            }

            return documentos;
        }

        public async Task<int> CrearAsync(DocumentoAdjunto documento)
        {
            const string query = @"
                INSERT INTO dbo.DocumentosAdjuntos
                (
                    ExpedienteID,
                    NombreArchivo,
                    RutaArchivo,
                    TipoArchivo,
                    Descripcion,
                    CreadoEn,
                    CreadoPor
                )
                OUTPUT INSERTED.ID
                VALUES
                (
                    @ExpedienteID,
                    @NombreArchivo,
                    @RutaArchivo,
                    @TipoArchivo,
                    @Descripcion,
                    SYSDATETIME(),
                    @CreadoPor
                );";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@ExpedienteID", documento.ExpedienteId);
            comando.Parameters.AddWithValue("@NombreArchivo", ValorONull(documento.NombreArchivo));
            comando.Parameters.AddWithValue("@RutaArchivo", ValorONull(documento.RutaArchivo));
            comando.Parameters.AddWithValue("@TipoArchivo", ValorONull(documento.TipoArchivo));
            comando.Parameters.AddWithValue("@Descripcion", ValorONull(documento.Descripcion));
            comando.Parameters.AddWithValue("@CreadoPor", documento.CreadoPor);

            await conexion.OpenAsync();

            var idGenerado = await comando.ExecuteScalarAsync();

            return Convert.ToInt32(idGenerado);
        }

        public async Task EliminarAsync(int documentoId)
        {
            const string query = @"
                DELETE FROM dbo.DocumentosAdjuntos
                WHERE ID = @ID;";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@ID", documentoId);

            await conexion.OpenAsync();
            await comando.ExecuteNonQueryAsync();
        }

        private static DocumentoAdjunto MapearDocumento(SqlDataReader reader)
        {
            return new DocumentoAdjunto
            {
                Id = ObtenerInt(reader, "ID"),
                ExpedienteId = ObtenerInt(reader, "ExpedienteID"),
                NombreArchivo = ObtenerString(reader, "NombreArchivo"),
                RutaArchivo = ObtenerString(reader, "RutaArchivo"),
                TipoArchivo = ObtenerString(reader, "TipoArchivo"),
                Descripcion = ObtenerString(reader, "Descripcion"),
                CreadoEn = ObtenerDateTime(reader, "CreadoEn"),
                CreadoPor = ObtenerInt(reader, "CreadoPor")
            };
        }

        private static object ValorONull(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor;
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