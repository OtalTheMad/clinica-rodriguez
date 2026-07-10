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
        public string Notas { get; set; }
        public DateTime CreadoEn { get; set; }
        public int CreadoPor { get; set; }

        public string NombrePaciente { get; set; }
        public string ApellidoPaciente { get; set; }
        public string NombreEspecialista { get; set; }
        public string ApellidoEspecialista { get; set; }

        public string NombreCompletoPaciente =>
            $"{NombrePaciente} {ApellidoPaciente}".Trim();

        public string NombreCompletoEspecialista =>
            $"{NombreEspecialista} {ApellidoEspecialista}".Trim();

        public string HoraCita =>
            FechaCita.ToString("hh:mm tt");
    }
}