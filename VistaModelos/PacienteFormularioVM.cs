using ClinicaRodriguez.Commands;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Repositorios;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClinicaRodriguez.VistaModelos
{
    public class PacienteFormularioVM : INotifyPropertyChanged
    {
        private readonly PacienteRepositorio _pacienteRepositorio;
        private readonly Action<Paciente> _pacienteCreadoCallback;
        private readonly Action _cancelarCallback;

        private Paciente _paciente;
        public Paciente Paciente
        {
            get => _paciente;
            set
            {
                _paciente = value;
                OnPropertyChanged(nameof(Paciente));
            }
        }

        private bool _estaGuardando;
        public bool EstaGuardando
        {
            get => _estaGuardando;
            set
            {
                _estaGuardando = value;
                OnPropertyChanged(nameof(EstaGuardando));
            }
        }

        public ICommand GuardarPacienteCommand { get; }
        public ICommand CancelarCommand { get; }

        public PacienteFormularioVM(
            PacienteRepositorio pacienteRepositorio,
            Action<Paciente> pacienteCreadoCallback,
            Action cancelarCallback)
        {
            _pacienteRepositorio = pacienteRepositorio;
            _pacienteCreadoCallback = pacienteCreadoCallback;
            _cancelarCallback = cancelarCallback;

            Paciente = new Paciente
            {
                FechaNacimiento = DateTime.Today
            };

            GuardarPacienteCommand = new AsyncRelayCommand(async _ => await GuardarPacienteAsync());
            CancelarCommand = new RelayCommand<object>(_ => Cancelar());
        }

        private async Task GuardarPacienteAsync()
        {
            try
            {
                if (!ValidarPaciente())
                    return;

                EstaGuardando = true;

                Paciente.Id = await _pacienteRepositorio.CrearAsync(Paciente);

                MessageBox.Show("Paciente registrado correctamente.");

                _pacienteCreadoCallback?.Invoke(Paciente);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar paciente: {ex.Message}");
            }
            finally
            {
                EstaGuardando = false;
            }
        }

        private bool ValidarPaciente()
        {
            if (string.IsNullOrWhiteSpace(Paciente.Nombre))
            {
                MessageBox.Show("Ingrese el nombre del paciente.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Paciente.Apellido))
            {
                MessageBox.Show("Ingrese el apellido del paciente.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Paciente.TelefonoPrimario))
            {
                MessageBox.Show("Ingrese el teléfono principal del paciente.");
                return false;
            }

            if (Paciente.FechaNacimiento == DateTime.MinValue)
            {
                MessageBox.Show("Ingrese la fecha de nacimiento del paciente.");
                return false;
            }

            return true;
        }

        private void Cancelar()
        {
            _cancelarCallback?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}