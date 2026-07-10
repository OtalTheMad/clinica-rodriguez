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
            return await ObtenerTodosAsync(false);
        }

        public async Task<List<Especialista>> ObtenerTodosAsync(bool incluirInactivos)
        {
            const string query = @"
                SELECT
                    e.ID,
                    e.Nombre,
                    e.Apellido,
                    e.Especialidad,
                    e.GraduadoEn,
                    e.CreadoEn,
                    e.CreadoPor,
                    e.UsuarioID,
                    u.NombreUsuario,
                    u.RolID,
                    ISNULL(u.EsActivo, 1) AS EsUsuarioActivo
                FROM dbo.Especialistas e
                LEFT JOIN dbo.Usuarios u ON u.ID = e.UsuarioID
                WHERE (@IncluirInactivos = 1 OR ISNULL(u.EsActivo, 1) = 1)
                ORDER BY e.Nombre, e.Apellido;";

            var especialistas = new List<Especialista>();

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@IncluirInactivos", incluirInactivos);

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
                    e.ID,
                    e.Nombre,
                    e.Apellido,
                    e.Especialidad,
                    e.GraduadoEn,
                    e.CreadoEn,
                    e.CreadoPor,
                    e.UsuarioID,
                    u.NombreUsuario,
                    u.RolID,
                    ISNULL(u.EsActivo, 1) AS EsUsuarioActivo
                FROM dbo.Especialistas e
                INNER JOIN dbo.Usuarios u ON u.ID = e.UsuarioID
                WHERE e.UsuarioID = @UsuarioID
                  AND u.EsActivo = 1;";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@UsuarioID", usuarioId);

            await conexion.OpenAsync();

            using var reader = await comando.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return MapearEspecialista(reader);

            return null;
        }

        public async Task<int> CrearAsync(Especialista especialista, int creadoPor)
        {
            const string queryUsuario = @"
                INSERT INTO dbo.Usuarios
                (
                    Nombre,
                    Apellido,
                    NombreUsuario,
                    Clave,
                    EsAdmin,
                    EsActivo,
                    CreadoEn,
                    CreadoPor,
                    RolID
                )
                OUTPUT INSERTED.ID
                VALUES
                (
                    @Nombre,
                    @Apellido,
                    @NombreUsuario,
                    @Clave,
                    0,
                    1,
                    SYSDATETIME(),
                    @CreadoPor,
                    @RolID
                );";

            const string queryEspecialista = @"
                INSERT INTO dbo.Especialistas
                (
                    Nombre,
                    Apellido,
                    Especialidad,
                    GraduadoEn,
                    CreadoEn,
                    CreadoPor,
                    UsuarioID
                )
                OUTPUT INSERTED.ID
                VALUES
                (
                    @Nombre,
                    @Apellido,
                    @Especialidad,
                    @GraduadoEn,
                    SYSDATETIME(),
                    @CreadoPor,
                    @UsuarioID
                );";

            using var conexion = new SqlConnection(_connectionString);
            await conexion.OpenAsync();

            using var transaccion = conexion.BeginTransaction();

            try
            {
                using var comandoUsuario = new SqlCommand(queryUsuario, conexion, transaccion);
                comandoUsuario.Parameters.AddWithValue("@Nombre", ValorONull(especialista.Nombre));
                comandoUsuario.Parameters.AddWithValue("@Apellido", ValorONull(especialista.Apellido));
                comandoUsuario.Parameters.AddWithValue("@NombreUsuario", ValorONull(especialista.NombreUsuario));
                comandoUsuario.Parameters.AddWithValue("@Clave", ValorONull(especialista.ClaveTemporal));
                comandoUsuario.Parameters.AddWithValue("@CreadoPor", creadoPor);
                comandoUsuario.Parameters.AddWithValue("@RolID", especialista.RolId);

                var usuarioIdGenerado = await comandoUsuario.ExecuteScalarAsync();
                var usuarioId = Convert.ToInt32(usuarioIdGenerado);

                using var comandoEspecialista = new SqlCommand(queryEspecialista, conexion, transaccion);
                comandoEspecialista.Parameters.AddWithValue("@Nombre", ValorONull(especialista.Nombre));
                comandoEspecialista.Parameters.AddWithValue("@Apellido", ValorONull(especialista.Apellido));
                comandoEspecialista.Parameters.AddWithValue("@Especialidad", ValorONull(especialista.Especialidad));
                comandoEspecialista.Parameters.AddWithValue("@GraduadoEn", ValorONull(especialista.GraduadoEn));
                comandoEspecialista.Parameters.AddWithValue("@CreadoPor", creadoPor);
                comandoEspecialista.Parameters.AddWithValue("@UsuarioID", usuarioId);

                var especialistaIdGenerado = await comandoEspecialista.ExecuteScalarAsync();

                transaccion.Commit();

                return Convert.ToInt32(especialistaIdGenerado);
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        public async Task ActualizarAsync(Especialista especialista)
        {
            const string queryActualizarEspecialista = @"
                UPDATE dbo.Especialistas
                SET
                    Nombre = @Nombre,
                    Apellido = @Apellido,
                    Especialidad = @Especialidad,
                    GraduadoEn = @GraduadoEn
                WHERE ID = @ID;";

            const string queryActualizarUsuarioSinClave = @"
                UPDATE dbo.Usuarios
                SET
                    Nombre = @Nombre,
                    Apellido = @Apellido,
                    NombreUsuario = @NombreUsuario,
                    RolID = @RolID
                WHERE ID = @UsuarioID;";

            const string queryActualizarUsuarioConClave = @"
                UPDATE dbo.Usuarios
                SET
                    Nombre = @Nombre,
                    Apellido = @Apellido,
                    NombreUsuario = @NombreUsuario,
                    Clave = @Clave,
                    RolID = @RolID
                WHERE ID = @UsuarioID;";

            using var conexion = new SqlConnection(_connectionString);
            await conexion.OpenAsync();

            using var transaccion = conexion.BeginTransaction();

            try
            {
                using var comandoEspecialista = new SqlCommand(queryActualizarEspecialista, conexion, transaccion);
                comandoEspecialista.Parameters.AddWithValue("@ID", especialista.Id);
                comandoEspecialista.Parameters.AddWithValue("@Nombre", ValorONull(especialista.Nombre));
                comandoEspecialista.Parameters.AddWithValue("@Apellido", ValorONull(especialista.Apellido));
                comandoEspecialista.Parameters.AddWithValue("@Especialidad", ValorONull(especialista.Especialidad));
                comandoEspecialista.Parameters.AddWithValue("@GraduadoEn", ValorONull(especialista.GraduadoEn));

                await comandoEspecialista.ExecuteNonQueryAsync();

                if (especialista.UsuarioId.HasValue)
                {
                    var queryUsuario = string.IsNullOrWhiteSpace(especialista.ClaveTemporal)
                        ? queryActualizarUsuarioSinClave
                        : queryActualizarUsuarioConClave;

                    using var comandoUsuario = new SqlCommand(queryUsuario, conexion, transaccion);
                    comandoUsuario.Parameters.AddWithValue("@UsuarioID", especialista.UsuarioId.Value);
                    comandoUsuario.Parameters.AddWithValue("@Nombre", ValorONull(especialista.Nombre));
                    comandoUsuario.Parameters.AddWithValue("@Apellido", ValorONull(especialista.Apellido));
                    comandoUsuario.Parameters.AddWithValue("@NombreUsuario", ValorONull(especialista.NombreUsuario));
                    comandoUsuario.Parameters.AddWithValue("@RolID", especialista.RolId);

                    if (!string.IsNullOrWhiteSpace(especialista.ClaveTemporal))
                        comandoUsuario.Parameters.AddWithValue("@Clave", ValorONull(especialista.ClaveTemporal));

                    await comandoUsuario.ExecuteNonQueryAsync();
                }

                transaccion.Commit();
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        public async Task DesactivarUsuarioAsync(int especialistaId)
        {
            const string query = @"
                UPDATE u
                SET u.EsActivo = 0
                FROM dbo.Usuarios u
                INNER JOIN dbo.Especialistas e ON e.UsuarioID = u.ID
                WHERE e.ID = @EspecialistaID;";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@EspecialistaID", especialistaId);

            await conexion.OpenAsync();
            await comando.ExecuteNonQueryAsync();
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
                UsuarioId = ObtenerIntNullable(reader, "UsuarioID"),
                NombreUsuario = ObtenerString(reader, "NombreUsuario"),
                RolId = ObtenerIntNullable(reader, "RolID") ?? 1,
                EsUsuarioActivo = ObtenerBool(reader, "EsUsuarioActivo")
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

        private static bool ObtenerBool(SqlDataReader reader, string columna)
        {
            int ordinal = reader.GetOrdinal(columna);
            return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
        }
    }
}