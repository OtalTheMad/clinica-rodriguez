using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaRodriguez.Modelos
{
    public class Paciente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public string Correo { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Residencia { get; set; }
        public string Ocupacion { get; set; }
        public string TelefonoPrimario { get; set; }
        public DateTime UltimaCita { get; set; }
        public string TipoDeCita { get; set; }
        public DateTime ProximaCita { get; set; }
        public string TipoProximaCita { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Edad{ get; set; }
    }
}
