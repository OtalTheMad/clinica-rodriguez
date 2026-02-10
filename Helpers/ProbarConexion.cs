using System;
using Microsoft.Data.SqlClient;

namespace ClinicaRodriguez.Helpers
{
    class ProbarConexion
    {
        public static bool ConectarPrueba()
        {
            var connectionString = Conexion.ObtenerConexion();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    Console.WriteLine("Conexión exitosa");
                    Console.WriteLine($"Base de datos: {connection.Database}");
                    Console.WriteLine($"Servidor: {connection.DataSource}");

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de conexión: {ex.Message}");
                return false;
            }
        }
    }
}
