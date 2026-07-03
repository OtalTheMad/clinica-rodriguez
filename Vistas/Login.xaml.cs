using ClinicaRodriguez.VistaModelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClinicaRodriguez.Vistas
{
    /// <summary>
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            DataContext = new LoginVM(this);
        }

        private void userClave_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginVM VistaModelo)
            {
                VistaModelo._clave = ((PasswordBox)sender).Password;
            }
        }
    }
}
