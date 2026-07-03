using ClinicaRodriguez.Modelos;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace ClinicaRodriguez.Repositorios
{
    public class ExpedienteRepositorio
    {
        private readonly string _connectionString;

        public ExpedienteRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Expediente> ObtenerPorPacienteIdAsync(int pacienteId)
        {
            const string query = @"
                SELECT TOP 1
                    ID,
                    PacienteID,
                    Servicio,
                    PresionArterial,
                    Temperatura,
                    FrecuenciaRespiratoria,
                    FrecuenciaCardiaca,
                    Peso,
                    Talla,
                    IndiceMasaCorporal,
                    NotasHistorial,
                    Odontograma,
                    TipoMordida,
                    EnfermedadesPeridontales,
                    ProtesisDentales,
                    UltimaTratamientoDental,
                    CreadoEn,
                    CreadoPor,
                    ModificadoEn,
                    ModificadoPor
                FROM dbo.Expedientes
                WHERE PacienteID = @PacienteID
                ORDER BY ID DESC;";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@PacienteID", pacienteId);

            await conexion.OpenAsync();

            using var reader = await comando.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return MapearExpediente(reader);

            return null;
        }

        public async Task<Expediente> ObtenerOCrearPorPacienteIdAsync(int pacienteId, int usuarioId, string servicio)
        {
            var expedienteExistente = await ObtenerPorPacienteIdAsync(pacienteId);

            if (expedienteExistente != null)
                return expedienteExistente;

            var nuevoExpediente = new Expediente
            {
                PacienteId = pacienteId,
                Servicio = servicio,
                CreadoPor = usuarioId,
                CreadoEn = DateTime.Now
            };

            nuevoExpediente.Id = await CrearAsync(nuevoExpediente);

            return nuevoExpediente;
        }

        public async Task<int> CrearAsync(Expediente expediente)
        {
            const string query = @"
                INSERT INTO dbo.Expedientes
                (
                    PacienteID,
                    Servicio,
                    PresionArterial,
                    Temperatura,
                    FrecuenciaRespiratoria,
                    FrecuenciaCardiaca,
                    Peso,
                    Talla,
                    IndiceMasaCorporal,
                    NotasHistorial,
                    Odontograma,
                    TipoMordida,
                    EnfermedadesPeridontales,
                    ProtesisDentales,
                    UltimaTratamientoDental,
                    CreadoEn,
                    CreadoPor
                )
                OUTPUT INSERTED.ID
                VALUES
                (
                    @PacienteID,
                    @Servicio,
                    @PresionArterial,
                    @Temperatura,
                    @FrecuenciaRespiratoria,
                    @FrecuenciaCardiaca,
                    @Peso,
                    @Talla,
                    @IndiceMasaCorporal,
                    @NotasHistorial,
                    @Odontograma,
                    @TipoMordida,
                    @EnfermedadesPeridontales,
                    @ProtesisDentales,
                    @UltimaTratamientoDental,
                    SYSDATETIME(),
                    @CreadoPor
                );";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            AgregarParametrosBase(comando, expediente);
            comando.Parameters.AddWithValue("@CreadoPor", expediente.CreadoPor);

            await conexion.OpenAsync();

            var idGenerado = await comando.ExecuteScalarAsync();

            return Convert.ToInt32(idGenerado);
        }

        public async Task ActualizarConsultaGeneralAsync(Expediente expediente)
        {
            const string query = @"
                UPDATE dbo.Expedientes
                SET
                    Servicio = @Servicio,
                    PresionArterial = @PresionArterial,
                    Temperatura = @Temperatura,
                    FrecuenciaRespiratoria = @FrecuenciaRespiratoria,
                    FrecuenciaCardiaca = @FrecuenciaCardiaca,
                    Peso = @Peso,
                    Talla = @Talla,
                    IndiceMasaCorporal = @IndiceMasaCorporal,
                    NotasHistorial = @NotasHistorial,
                    ModificadoEn = SYSDATETIME(),
                    ModificadoPor = @ModificadoPor
                WHERE ID = @ID;";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            comando.Parameters.AddWithValue("@ID", expediente.Id);
            comando.Parameters.AddWithValue("@Servicio", ValorONull(expediente.Servicio));
            comando.Parameters.AddWithValue("@PresionArterial", ValorONull(expediente.PresionArterial));
            comando.Parameters.AddWithValue("@Temperatura", ValorONull(expediente.Temperatura));
            comando.Parameters.AddWithValue("@FrecuenciaRespiratoria", ValorONull(expediente.FrecuenciaRespiratoria));
            comando.Parameters.AddWithValue("@FrecuenciaCardiaca", ValorONull(expediente.FrecuenciaCardiaca));
            comando.Parameters.AddWithValue("@Peso", ValorONull(expediente.Peso));
            comando.Parameters.AddWithValue("@Talla", ValorONull(expediente.Talla));
            comando.Parameters.AddWithValue("@IndiceMasaCorporal", ValorONull(expediente.IndiceMasaCorporal));
            comando.Parameters.AddWithValue("@NotasHistorial", ValorONull(expediente.NotasHistorial));
            comando.Parameters.AddWithValue("@ModificadoPor", ValorONull(expediente.ModificadoPor));

            await conexion.OpenAsync();
            await comando.ExecuteNonQueryAsync();
        }

        public async Task ActualizarOdontologiaAsync(Expediente expediente)
        {
            const string query = @"
                UPDATE dbo.Expedientes
                SET
                    Servicio = @Servicio,
                    PresionArterial = @PresionArterial,
                    Temperatura = @Temperatura,
                    FrecuenciaRespiratoria = @FrecuenciaRespiratoria,
                    FrecuenciaCardiaca = @FrecuenciaCardiaca,
                    Peso = @Peso,
                    Talla = @Talla,
                    IndiceMasaCorporal = @IndiceMasaCorporal,
                    NotasHistorial = @NotasHistorial,
                    Odontograma = @Odontograma,
                    TipoMordida = @TipoMordida,
                    EnfermedadesPeridontales = @EnfermedadesPeridontales,
                    ProtesisDentales = @ProtesisDentales,
                    UltimaTratamientoDental = @UltimaTratamientoDental,
                    ModificadoEn = SYSDATETIME(),
                    ModificadoPor = @ModificadoPor
                WHERE ID = @ID;";

            using var conexion = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(query, conexion);

            AgregarParametrosBase(comando, expediente);
            comando.Parameters.AddWithValue("@ID", expediente.Id);
            comando.Parameters.AddWithValue("@ModificadoPor", ValorONull(expediente.ModificadoPor));

            await conexion.OpenAsync();
            await comando.ExecuteNonQueryAsync();
        }

        private static void AgregarParametrosBase(SqlCommand comando, Expediente expediente)
        {
            comando.Parameters.AddWithValue("@PacienteID", expediente.PacienteId);
            comando.Parameters.AddWithValue("@Servicio", ValorONull(expediente.Servicio));
            comando.Parameters.AddWithValue("@PresionArterial", ValorONull(expediente.PresionArterial));
            comando.Parameters.AddWithValue("@Temperatura", ValorONull(expediente.Temperatura));
            comando.Parameters.AddWithValue("@FrecuenciaRespiratoria", ValorONull(expediente.FrecuenciaRespiratoria));
            comando.Parameters.AddWithValue("@FrecuenciaCardiaca", ValorONull(expediente.FrecuenciaCardiaca));
            comando.Parameters.AddWithValue("@Peso", ValorONull(expediente.Peso));
            comando.Parameters.AddWithValue("@Talla", ValorONull(expediente.Talla));
            comando.Parameters.AddWithValue("@IndiceMasaCorporal", ValorONull(expediente.IndiceMasaCorporal));
            comando.Parameters.AddWithValue("@NotasHistorial", ValorONull(expediente.NotasHistorial));
            comando.Parameters.AddWithValue("@Odontograma", ValorONull(expediente.Odontograma));
            comando.Parameters.AddWithValue("@TipoMordida", ValorONull(expediente.TipoMordida));
            comando.Parameters.AddWithValue("@EnfermedadesPeridontales", ValorONull(expediente.EnfermedadesPeridontales));
            comando.Parameters.AddWithValue("@ProtesisDentales", ValorONull(expediente.ProtesisDentales));
            comando.Parameters.AddWithValue("@UltimaTratamientoDental", ValorONull(expediente.UltimaTratamientoDental));
        }

        private static Expediente MapearExpediente(SqlDataReader reader)
        {
            return new Expediente
            {
                Id = ObtenerInt(reader, "ID"),
                PacienteId = ObtenerInt(reader, "PacienteID"),
                Servicio = ObtenerString(reader, "Servicio"),
                PresionArterial = ObtenerString(reader, "PresionArterial"),
                Temperatura = ObtenerDecimalNullable(reader, "Temperatura"),
                FrecuenciaRespiratoria = ObtenerIntNullable(reader, "FrecuenciaRespiratoria"),
                FrecuenciaCardiaca = ObtenerIntNullable(reader, "FrecuenciaCardiaca"),
                Peso = ObtenerDecimalNullable(reader, "Peso"),
                Talla = ObtenerDecimalNullable(reader, "Talla"),
                IndiceMasaCorporal = ObtenerDecimalNullable(reader, "IndiceMasaCorporal"),
                NotasHistorial = ObtenerString(reader, "NotasHistorial"),
                Odontograma = ObtenerString(reader, "Odontograma"),
                TipoMordida = ObtenerString(reader, "TipoMordida"),
                EnfermedadesPeridontales = ObtenerString(reader, "EnfermedadesPeridontales"),
                ProtesisDentales = ObtenerString(reader, "ProtesisDentales"),
                UltimaTratamientoDental = ObtenerString(reader, "UltimaTratamientoDental"),
                CreadoEn = ObtenerDateTime(reader, "CreadoEn"),
                CreadoPor = ObtenerInt(reader, "CreadoPor"),
                ModificadoEn = ObtenerDateTimeNullable(reader, "ModificadoEn"),
                ModificadoPor = ObtenerIntNullable(reader, "ModificadoPor")
            };
        }

        private static object ValorONull(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor;
        }

        private static object ValorONull(int? valor)
        {
            return valor.HasValue ? valor.Value : DBNull.Value;
        }

        private static object ValorONull(decimal? valor)
        {
            return valor.HasValue ? valor.Value : DBNull.Value;
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

        private static decimal? ObtenerDecimalNullable(SqlDataReader reader, string columna)
        {
            int ordinal = reader.GetOrdinal(columna);
            return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
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

        private static DateTime? ObtenerDateTimeNullable(SqlDataReader reader, string columna)
        {
            int ordinal = reader.GetOrdinal(columna);
            return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
        }
    }
}