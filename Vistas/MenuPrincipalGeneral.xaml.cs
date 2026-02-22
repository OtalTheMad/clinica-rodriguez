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

            string connectionString = @"Server=localhost;
                         Database=DevClinicaRodriguez;
                         Integrated Security=true;
                         TrustServerCertificate=true;";

            DataContext = new MenuPrincipalVM(usuario, connectionString);
        }
    }
}