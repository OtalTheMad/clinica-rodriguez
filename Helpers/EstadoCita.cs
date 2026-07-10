using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaRodriguez.Helpers
{
    public static class EstadosCita
    {
        public const string Pendiente = "Pendiente";
        public const string Completada = "Completada";
        public const string Cancelada = "Cancelada";
        public const string NoAsistio = "No asistió";

        public static string[] Todos => new[]
        {
            Pendiente,
            Completada,
            Cancelada,
            NoAsistio
        };
    }
}
