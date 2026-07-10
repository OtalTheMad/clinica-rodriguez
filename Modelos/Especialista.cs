using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaRodriguez.Modelos
{
    public class Especialista
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Especialidad { get; set; }
        public string GraduadoEn { get; set; }
        public DateTime CreadoEn { get; set; }
        public int CreadoPor { get; set; }
        public int? UsuarioId { get; set; }

        public string NombreUsuario { get; set; }
        public string ClaveTemporal { get; set; }
        public int RolId { get; set; }
        public bool EsUsuarioActivo { get; set; }

        public string NombreCompleto =>
            $"{Nombre} {Apellido}".Trim();

        public string EstadoUsuarioTexto =>
            EsUsuarioActivo ? "Activo" : "Inactivo";
    }
}
