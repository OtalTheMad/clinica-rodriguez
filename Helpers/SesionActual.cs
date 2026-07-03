using ClinicaRodriguez.Modelos;

namespace ClinicaRodriguez.Helpers
{
    public static class SesionActual
    {
        public static Usuario UsuarioActual { get; set; }

        public static int RolId { get; set; } = RolesSistema.ConsultaGeneral;

        public static bool EsConsultaGeneral => RolId == RolesSistema.ConsultaGeneral;

        public static bool EsOdontologo => RolId == RolesSistema.Odontologia;
    }
}
