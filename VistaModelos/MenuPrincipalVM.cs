using ClinicaRodriguez.Commands;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Repositorios;
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
            CurrentView = CrearPacientesVM();
        }

        private PacientesVM CrearPacientesVM()
        {
            return new PacientesVM(_pacienteRepositorio, AbrirExpedienteDePaciente);
        }

        private PacienteFormularioVM CrearPacienteFormularioVM()
        {
            return new PacienteFormularioVM(
                _pacienteRepositorio,
                AbrirExpedienteDePaciente,
                () => CurrentView = CrearPacientesVM()
            );
        }

        private void AbrirExpedienteDePaciente(Paciente paciente)
        {
            if (paciente == null)
                return;

            CurrentView = new ExpedientesVM(
                UsuarioActual,
                _connectionString,
                paciente
            );
        }

        private void Navigate(string destino)
        {
            CurrentView = destino switch
            {
                "Pacientes" => CrearPacientesVM(),
                "NuevoPaciente" => CrearPacienteFormularioVM(),
                "Expedientes" => new ExpedientesVM(UsuarioActual, _connectionString),
                "Citas" => new CitasVM(UsuarioActual, _connectionString, AbrirExpedienteDePaciente),
                "Especialistas" => new EspecialistasVM(UsuarioActual, _connectionString),
                _ => null
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}