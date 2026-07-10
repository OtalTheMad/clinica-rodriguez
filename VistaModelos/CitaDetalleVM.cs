using ClinicaRodriguez.Commands;
using ClinicaRodriguez.Helpers;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Repositorios;
using System;
using System.Windows;
using System.Windows.Input;

namespace ClinicaRodriguez.VistaModelos
{
    public class CitaDetalleVM
    {
        private readonly CitaRepositorio _citaRepositorio;
        private readonly Action<Paciente> _abrirExpedienteCallback;
        private readonly Action _cerrarCallback;

        public Cita Cita { get; }
        public string[] EstadosDisponibles => EstadosCita.Todos;
        public string EstadoSeleccionado { get; set; }
        public string Notas { get; set; }

        public ICommand GuardarCommand { get; }
        public ICommand AbrirExpedienteCommand { get; }
        public ICommand CerrarCommand { get; }

        public CitaDetalleVM(
            Cita cita,
            CitaRepositorio citaRepositorio,
            Action<Paciente> abrirExpedienteCallback,
            Action cerrarCallback)
        {
            Cita = cita;
            _citaRepositorio = citaRepositorio;
            _abrirExpedienteCallback = abrirExpedienteCallback;
            _cerrarCallback = cerrarCallback;

            EstadoSeleccionado = string.IsNullOrWhiteSpace(cita.Estado)
                ? EstadosCita.Pendiente
                : cita.Estado;

            Notas = cita.Notas ?? string.Empty;

            GuardarCommand = new AsyncRelayCommand(async _ => await GuardarAsync());
            AbrirExpedienteCommand = new RelayCommand<object>(_ => AbrirExpediente());
            CerrarCommand = new RelayCommand<object>(_ => _cerrarCallback?.Invoke());
        }

        private async System.Threading.Tasks.Task GuardarAsync()
        {
            try
            {
                await _citaRepositorio.ActualizarEstadoYNotasAsync(Cita.Id, EstadoSeleccionado, Notas);

                Cita.Estado = EstadoSeleccionado;
                Cita.Notas = Notas;

                MessageBox.Show("Cita actualizada correctamente.");
                _cerrarCallback?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la cita: {ex.Message}");
            }
        }

        private void AbrirExpediente()
        {
            var paciente = new Paciente
            {
                Id = Cita.PacienteId,
                Nombre = Cita.NombrePaciente,
                Apellido = Cita.ApellidoPaciente
            };

            _abrirExpedienteCallback?.Invoke(paciente);
            _cerrarCallback?.Invoke();
        }
    }
}