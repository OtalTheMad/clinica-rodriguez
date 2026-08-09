using System;

namespace ClinicaRodriguez.Modelos
{
    public class NotaClinica
    {
        public int Id { get; set; }

        public int ExpedienteId { get; set; }

        public int? CitaId { get; set; }

        public DateTime FechaNota { get; set; }

        public string MotivoConsulta { get; set; }

        public string Diagnostico { get; set; }

        public string Tratamiento { get; set; }

        public string Indicaciones { get; set; }

        public DateTime CreadoEn { get; set; }

        public int CreadoPor { get; set; }
    }
}