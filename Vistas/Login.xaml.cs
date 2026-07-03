using ClinicaRodriguez.VistaModelos;
using System.Windows;
using System.Windows.Controls;

namespace ClinicaRodriguez.Vistas
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            DataContext = new LoginVM(this);
        }

        private void userClave_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginVM vistaModelo)
            {
                vistaModelo._clave = ((PasswordBox)sender).Password;
            }
        }
    }
}
