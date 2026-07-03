using System;

namespace ClinicaRodriguez.Modelos
{
    public class Cita
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public int EspecialistaId { get; set; }
        public DateTime FechaCita { get; set; }
        public string Estado { get; set; }
        public int Duracion { get; set; }
        public DateTime CreadoEn { get; set; }
        public int CreadoPor { get; set; }
    }
}