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
using System.Windows.Input;

namespace ClinicaRodriguez.VistaModelos
{
    internal class LoginVM
    {
        private readonly UsuarioRepositorio _repositorio;
        public string _nombreUsuario { get; set; }
        public string _clave { get; set; }

        public ICommand ComandoLogin { get; }


        public LoginVM()
        {
            _repositorio = new UsuarioRepositorio();
            ComandoLogin = new AsyncRelayCommand(async _ => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            //var passwordHash = PasswordHasher.HashPassword(_clave); //Habilitar en producción
            var usuario = await _repositorio.ValidarCredencialesAsync(_nombreUsuario, _clave);

            if (usuario != null)
            {
                System.Windows.MessageBox.Show("¡Inicio de sesión exitoso!");
            }
            else
            {
                System.Windows.MessageBox.Show("Credenciales inválidas. Por favor, intente de nuevo.");
            }
        }
    }
}
