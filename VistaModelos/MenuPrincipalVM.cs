using ClinicaRodriguez.Commands;
using ClinicaRodriguez.Helpers;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Repositorios;
using Microsoft.Data.SqlClient;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ClinicaRodriguez.VistaModelos
{
    public class MenuPrincipalVM : INotifyPropertyChanged
    {
        private readonly string _connectionString;
        private readonly PacienteRepositorio _pacienteRepositorio;

        public Usuario UsuarioActual { get; set; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public ICommand NavigateCommand { get; }

        public MenuPrincipalVM(Usuario usuario, string connectionString)
        {
            UsuarioActual = usuario;
            _connectionString = connectionString;

            _pacienteRepositorio = new PacienteRepositorio(_connectionString);

            NavigateCommand = new RelayCommand<string>(Navigate);
        }

        private void Navigate(string destino)
        {
            CurrentView = destino switch
            {
                "Pacientes" => new PacientesVM(_pacienteRepositorio),
                "Citas" => null,         // Add CitasViewModel later
                "Especialistas" => null, // Add EspecialistasViewModel later
                "Expedientes" => null,   // Add ExpedientesViewModel later
                _ => null
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
