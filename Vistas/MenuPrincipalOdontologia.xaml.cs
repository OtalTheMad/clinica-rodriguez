using ClinicaRodriguez.Helpers;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.VistaModelos;
using System.Windows;

namespace ClinicaRodriguez.Vistas
{
    public partial class MenuPrincipalOdontologia : Window
    {
        public MenuPrincipalOdontologia()
        {
            InitializeComponent();
            SesionActual.RolId = RolesSistema.Odontologia;
        }

        public MenuPrincipalOdontologia(Usuario usuario) : this()
        {
            SesionActual.UsuarioActual = usuario;

            string connectionString = @"Server=localhost;
                         Database=DevClinicaRodriguez;
                         Integrated Security=true;
                         TrustServerCertificate=true;";

            DataContext = new MenuPrincipalVM(usuario, connectionString);
        }
    }
}
