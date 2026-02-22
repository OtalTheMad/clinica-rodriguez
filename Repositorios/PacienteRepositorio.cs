using ClinicaRodriguez.Modelos;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PacienteRepositorio
{
    private readonly string _connectionString;

    public PacienteRepositorio(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<Paciente>> ObtenerTodosAsync()
    {
        var pacientes = new List<Paciente>();

        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            var query = @"SELECT 
	                           ID
                              ,Nombre
                              ,Apellido
                              ,DNI
                              ,Correo
                              ,FechaNacimiento
                              ,Residencia
                              ,Ocupacion
                              ,TelefonoPrimario
                              ,UltimaCita
                              ,TipoDeCita
                              ,ProximaCita
                              ,TipoProximaCita
                              ,Edad
                          FROM Pacientes";

            try
            {
                using (var cmd = new SqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var paciente = new Paciente
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                DNI = reader.GetString(3),
                                Correo = reader.GetString(4),
                                FechaNacimiento = reader.GetDateTime(5),
                                Residencia = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Ocupacion = reader.GetString(7),
                                TelefonoPrimario = reader.GetString(8),
                                UltimaCita = reader.IsDBNull(9) ? DateTime.MinValue : reader.GetDateTime(9),
                                TipoDeCita = reader.IsDBNull(10) ? null : reader.GetString(10),
                                ProximaCita = reader.IsDBNull(11) ? DateTime.MinValue : reader.GetDateTime(11),
                                TipoProximaCita = reader.IsDBNull(12) ? null : reader.GetString(12),
                                Edad = reader.GetInt32(13)
                            };
                            pacientes.Add(paciente);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error de base de datos: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener pacientes: {ex.Message}", ex);
            }
        }

        return pacientes;
    }

    public async Task<Paciente> ObtenerPorIdAsync(int id)
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"SELECT 
	                           ID
                              ,Nombre
                              ,Apellido
                              ,DNI
                              ,Correo
                              ,FechaNacimiento
                              ,Residencia
                              ,Ocupacion
                              ,TelefonoPrimario
                              ,UltimaCita
                              ,TipoDeCita
                              ,ProximaCita
                              ,TipoProximaCita
                              ,Edad
                          FROM Pacientes
                          WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Paciente
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                DNI = reader.GetString(3),
                                Correo = reader.GetString(4),
                                FechaNacimiento = reader.GetDateTime(5),
                                Residencia = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Ocupacion = reader.GetString(7),
                                TelefonoPrimario = reader.GetString(8),
                                UltimaCita = reader.IsDBNull(9) ? DateTime.MinValue : reader.GetDateTime(9),
                                TipoDeCita = reader.IsDBNull(10) ? null : reader.GetString(10),
                                ProximaCita = reader.IsDBNull(11) ? DateTime.MinValue : reader.GetDateTime(11),
                                TipoProximaCita = reader.IsDBNull(12) ? null : reader.GetString(12),
                                Edad = reader.GetInt32(13)
                            };
                        }
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            throw new Exception($"Error de base de datos: {ex.Message}", ex);
        }

        return null;
    }
}