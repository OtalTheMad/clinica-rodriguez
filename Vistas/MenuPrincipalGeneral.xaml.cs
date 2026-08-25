using ClinicaRodriguez.Helpers;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.VistaModelos;
using System.Windows;

namespace ClinicaRodriguez.Vistas
{
    public partial class MenuPrincipalGeneral : Window
    {
        public MenuPrincipalGeneral(Usuario usuario)
        {
            InitializeComponent();

            SesionActual.UsuarioActual = usuario;
            SesionActual.RolId = RolesSistema.ConsultaGeneral;

            string connectionString = Conexion.ObtenerConexion();

            DataContext = new MenuPrincipalVM(usuario, connectionString);
        }
    }
}
