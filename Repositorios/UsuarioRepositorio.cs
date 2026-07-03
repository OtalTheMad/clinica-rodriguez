using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Helpers;

namespace ClinicaRodriguez.Repositorios
{
    internal class UsuarioRepositorio
    {
        private readonly string _connectionString;

        public UsuarioRepositorio()
        {
            _connectionString = Conexion.ObtenerConexion();
        }

        public async Task<Usuario> ValidarCredencialesAsync(string nombreUsuario, string passwordHash)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var query = @"SELECT ID, NombreCompleto, NombreUsuario, EsAdmin, RolID
                             FROM Usuarios
                             WHERE NombreUsuario = @Usuario AND Clave = @Clave AND EsActivo = 1";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Usuario", nombreUsuario);
                        cmd.Parameters.AddWithValue("@Clave", passwordHash);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Usuario
                                {
                                    ID = reader.GetInt32(0),
                                    NombreCompleto = reader.GetString(1),
                                    NombreUsuario = reader.GetString(2),
                                    EsAdmin = reader.GetBoolean(3),
                                    RolID = reader.GetInt32(4)
                                };
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
