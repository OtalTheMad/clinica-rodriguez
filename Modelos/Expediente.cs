using System;

namespace ClinicaRodriguez.Modelos
{
    public class Expediente
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public string Servicio { get; set; } = string.Empty;
        public string PresionArterial { get; set; } = string.Empty;
        public decimal? Temperatura { get; set; }
        public int? FrecuenciaRespiratoria { get; set; }
        public int? FrecuenciaCardiaca { get; set; }
        public decimal? Peso { get; set; }
        public decimal? Talla { get; set; }
        public decimal? IndiceMasaCorporal { get; set; }
        public string NotasHistorial { get; set; } = string.Empty;
        public string AntecedentesPatologicosFamiliares { get; set; }
        public string MotivoConsulta { get; set; }
        public string Odontograma { get; set; } = string.Empty;
        public string TipoMordida { get; set; } = string.Empty;
        public string EnfermedadesPeridontales { get; set; } = string.Empty;
        public string ProtesisDentales { get; set; } = string.Empty;
        public string UltimaTratamientoDental { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
        public int CreadoPor { get; set; }
        public DateTime? ModificadoEn { get; set; }
        public int? ModificadoPor { get; set; }
    }
}
