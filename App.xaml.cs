using ClinicaRodriguez.Helpers;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ClinicaRodriguez
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!ProbarConexion.ConectarPrueba())
            {
                MessageBox.Show(
                    "No se puede conectar a la base de datos.\n\nVerifique que SQL Server esté ejecutándose.",
                    "Error de Conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                Shutdown();
            }
            else
            {
                ProbarConexion.ConectarPrueba();
            }
        }
    }

}
