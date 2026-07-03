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

    public async Task<int> CrearAsync(Paciente paciente)
    {
        const string query = @"
        INSERT INTO dbo.Pacientes
        (
            Nombre,
            Apellido,
            DNI,
            Correo,
            FechaNacimiento,
            Residencia,
            Ocupacion,
            TelefonoPrimario,
            UltimaCita,
            TipoDeCita,
            ProximaCita,
            TipoProximaCita
        )
        OUTPUT INSERTED.ID
        VALUES
        (
            @Nombre,
            @Apellido,
            @DNI,
            @Correo,
            @FechaNacimiento,
            @Residencia,
            @Ocupacion,
            @TelefonoPrimario,
            @UltimaCita,
            @TipoDeCita,
            @ProximaCita,
            @TipoProximaCita
        );";

        using var conexion = new SqlConnection(_connectionString);
        using var comando = new SqlCommand(query, conexion);

        comando.Parameters.AddWithValue("@Nombre", ValorONull(paciente.Nombre));
        comando.Parameters.AddWithValue("@Apellido", ValorONull(paciente.Apellido));
        comando.Parameters.AddWithValue("@DNI", ValorONull(paciente.DNI));
        comando.Parameters.AddWithValue("@Correo", ValorONull(paciente.Correo));
        comando.Parameters.AddWithValue("@FechaNacimiento", ValorONull(paciente.FechaNacimiento));
        comando.Parameters.AddWithValue("@Residencia", ValorONull(paciente.Residencia));
        comando.Parameters.AddWithValue("@Ocupacion", ValorONull(paciente.Ocupacion));
        comando.Parameters.AddWithValue("@TelefonoPrimario", ValorONull(paciente.TelefonoPrimario));
        comando.Parameters.AddWithValue("@UltimaCita", ValorONull(paciente.UltimaCita));
        comando.Parameters.AddWithValue("@TipoDeCita", ValorONull(paciente.TipoDeCita));
        comando.Parameters.AddWithValue("@ProximaCita", ValorONull(paciente.ProximaCita));
        comando.Parameters.AddWithValue("@TipoProximaCita", ValorONull(paciente.TipoProximaCita));

        await conexion.OpenAsync();

        var idGenerado = await comando.ExecuteScalarAsync();

        return Convert.ToInt32(idGenerado);
    }

    private static object ValorONull(string valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
    }

    private static object ValorONull(DateTime fecha)
    {
        return fecha == DateTime.MinValue ? DBNull.Value : fecha;
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