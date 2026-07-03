using ClinicaRodriguez.Commands;
using ClinicaRodriguez.Helpers;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Repositorios;
using ClinicaRodriguez.Vistas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClinicaRodriguez.VistaModelos
{
    internal class LoginVM
    {
        private readonly UsuarioRepositorio _repositorio;
        private readonly Window _ventanaLogin;
        public string _nombreUsuario { get; set; }
        public string _clave { get; set; }

        public ICommand ComandoLogin { get; }


        public LoginVM(Window ventanaLogin)
        {
            _repositorio = new UsuarioRepositorio();
            ComandoLogin = new AsyncRelayCommand(async _ => await LoginAsync());
            _ventanaLogin = ventanaLogin;
        }

        private async Task LoginAsync()
        {
            //var passwordHash = PasswordHasher.HashPassword(_clave); //Habilitar en producción
            var usuario = await _repositorio.ValidarCredencialesAsync(_nombreUsuario, _clave);

            if (usuario != null)
            {
                Console.WriteLine("¡Inicio de sesión exitoso!");
                var menuPrincipal = new MenuPrincipalGeneral(usuario);
                menuPrincipal.Show();
                _ventanaLogin.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Credenciales inválidas. Por favor, intente de nuevo.");
            }
        }
    }
}
