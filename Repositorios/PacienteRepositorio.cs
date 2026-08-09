using ClinicaRodriguez.Modelos;
using Microsoft.Data.SqlClient;
using System;
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

        comando.Parameters.AddWithValue(
            "@Nombre",
            ValorONull(paciente.Nombre)
        );

        comando.Parameters.AddWithValue(
            "@Apellido",
            ValorONull(paciente.Apellido)
        );

        comando.Parameters.AddWithValue(
            "@DNI",
            ValorONull(paciente.DNI)
        );

        comando.Parameters.AddWithValue(
            "@Correo",
            ValorONull(paciente.Correo)
        );

        comando.Parameters.AddWithValue(
            "@FechaNacimiento",
            ValorONull(paciente.FechaNacimiento)
        );

        comando.Parameters.AddWithValue(
            "@Residencia",
            ValorONull(paciente.Residencia)
        );

        comando.Parameters.AddWithValue(
            "@Ocupacion",
            ValorONull(paciente.Ocupacion)
        );

        comando.Parameters.AddWithValue(
            "@TelefonoPrimario",
            ValorONull(paciente.TelefonoPrimario)
        );

        comando.Parameters.AddWithValue(
            "@UltimaCita",
            ValorONull(paciente.UltimaCita)
        );

        comando.Parameters.AddWithValue(
            "@TipoDeCita",
            ValorONull(paciente.TipoDeCita)
        );

        comando.Parameters.AddWithValue(
            "@ProximaCita",
            ValorONull(paciente.ProximaCita)
        );

        comando.Parameters.AddWithValue(
            "@TipoProximaCita",
            ValorONull(paciente.TipoProximaCita)
        );

        await conexion.OpenAsync();

        var idGenerado = await comando.ExecuteScalarAsync();

        return Convert.ToInt32(idGenerado);
    }

    public async Task<List<Paciente>> ObtenerTodosAsync()
    {
        const string query = @"
            SELECT
                ID,
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
                TipoProximaCita,
                Edad
            FROM dbo.Pacientes
            ORDER BY Nombre, Apellido;";

        var pacientes = new List<Paciente>();

        try
        {
            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            await conexion.OpenAsync();

            using var reader = await comando.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                pacientes.Add(MapearPaciente(reader));
            }

            return pacientes;
        }
        catch (SqlException ex)
        {
            throw new Exception(
                $"Error de base de datos al obtener los pacientes: {ex.Message}",
                ex
            );
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error al obtener los pacientes: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Paciente> ObtenerPorIdAsync(int id)
    {
        const string query = @"
            SELECT TOP 1
                ID,
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
                TipoProximaCita,
                Edad
            FROM dbo.Pacientes
            WHERE ID = @ID;";

        try
        {
            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@ID", id);

            await conexion.OpenAsync();

            using var reader = await comando.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapearPaciente(reader);
            }

            return null;
        }
        catch (SqlException ex)
        {
            throw new Exception(
                $"Error de base de datos al obtener el paciente: {ex.Message}",
                ex
            );
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error al obtener el paciente: {ex.Message}",
                ex
            );
        }
    }

    private static Paciente MapearPaciente(SqlDataReader reader)
    {
        return new Paciente
        {
            Id = ObtenerInt(reader, "ID"),

            Nombre = ObtenerString(reader, "Nombre"),

            Apellido = ObtenerString(reader, "Apellido"),

            DNI = ObtenerString(reader, "DNI"),

            Correo = ObtenerString(reader, "Correo"),

            FechaNacimiento = ObtenerDateTime(reader, "FechaNacimiento"),

            Residencia = ObtenerString(reader, "Residencia"),

            Ocupacion = ObtenerString(reader, "Ocupacion"),

            TelefonoPrimario = ObtenerString(reader, "TelefonoPrimario"),

            UltimaCita = ObtenerDateTime(reader, "UltimaCita"),

            TipoDeCita = ObtenerString(reader, "TipoDeCita"),

            ProximaCita = ObtenerDateTime(reader, "ProximaCita"),

            TipoProximaCita = ObtenerString(reader, "TipoProximaCita"),

            Edad = ObtenerInt(reader, "Edad")
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

        return reader.IsDBNull(posicion)
            ? 0
            : reader.GetInt32(posicion);
    }

    private static DateTime ObtenerDateTime(
        SqlDataReader reader,
        string columna)
    {
        int posicion = reader.GetOrdinal(columna);

        return reader.IsDBNull(posicion)
            ? DateTime.MinValue
            : reader.GetDateTime(posicion);
    }

    private static object ValorONull(string valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? DBNull.Value
            : valor.Trim();
    }

    private static object ValorONull(DateTime fecha)
    {
        return fecha == DateTime.MinValue
            ? DBNull.Value
            : fecha;
    }
}