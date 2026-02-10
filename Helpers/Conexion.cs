using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaRodriguez.Helpers
{
    class Conexion
    {
        public static string ObtenerConexion()
        {
            #if DEBUG
                return @"Server=localhost;
                         Database=DevClinicaRodriguez;
                         Integrated Security=true;
                         TrustServerCertificate=true;";
            #else
                return @"Server=localhost\SQLEXPRESS;
                         Database=ProdClinicaRodriguez;
                         Integrated Security=true;
                         TrustServerCertificate=true;";
            #endif

        }
    }
}
