using ClinicaRodriguez.Commands;
using ClinicaRodriguez.Helpers;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Repositorios;
using ClinicaRodriguez.Vistas;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClinicaRodriguez.VistaModelos
{
    internal class LoginVM : INotifyPropertyChanged
    {
        private readonly UsuarioRepositorio _repositorio;
        private readonly Window _ventanaLogin;
        private string _nombreUsuarioValor;
        private string _claveValor;

        public string _nombreUsuario
        {
            get => _nombreUsuarioValor;
            set
            {
                if (_nombreUsuarioValor == value)
                    return;

                _nombreUsuarioValor = value;
                OnPropertyChanged();
            }
        }

        public string _clave
        {
            get => _claveValor;
            set
            {
                if (_claveValor == value)
                    return;

                _claveValor = value;
                OnPropertyChanged();
            }
        }

        public ICommand ComandoLogin { get; }

        public LoginVM(Window ventanaLogin)
        {
            _repositorio = new UsuarioRepositorio();
            _ventanaLogin = ventanaLogin;
            ComandoLogin = new AsyncRelayCommand(async _ => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(_nombreUsuario) || string.IsNullOrWhiteSpace(_clave))
            {
                MessageBox.Show("Ingrese usuario y contraseña.", "Inicio de sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // var passwordHash = PasswordHasher.HashPassword(_clave); // Habilitar en producción.
            var usuario = await _repositorio.ValidarCredencialesAsync(_nombreUsuario, _clave);

            if (usuario == null)
            {
                MessageBox.Show("Credenciales inválidas. Por favor, intente de nuevo.", "Inicio de sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int rolId = ObtenerRolId(usuario);

            if (rolId != RolesSistema.ConsultaGeneral && rolId != RolesSistema.Odontologia)
            {
                MessageBox.Show(
                    "El usuario no tiene un rol válido asignado. Verifique el campo RolID en la tabla Usuarios.",
                    "Rol no válido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SesionActual.UsuarioActual = usuario;
            SesionActual.RolId = rolId;

            Window menuPrincipal = rolId switch
            {
                RolesSistema.Odontologia => new MenuPrincipalOdontologia(usuario),
                _ => new MenuPrincipalGeneral(usuario)
            };

            menuPrincipal.Show();
            _ventanaLogin.Close();
        }

        private static int ObtenerRolId(Usuario usuario)
        {
            PropertyInfo propiedadRol = typeof(Usuario).GetProperty("RolID")
                ?? typeof(Usuario).GetProperty("RolID");

            if (propiedadRol == null)
                return 0;

            object valor = propiedadRol.GetValue(usuario);

            if (valor is int rolId)
                return rolId;

            if (valor != null && int.TryParse(valor.ToString(), out int rolIdConvertido))
                return rolIdConvertido;

            return 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
