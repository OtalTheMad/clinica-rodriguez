// VistaModelos/PacientesVM.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ClinicaRodriguez.Commands;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Repositorios;
using ClinicaRodriguez.Utilidades;

namespace ClinicaRodriguez.VistaModelos
{
    public class PacientesVM : INotifyPropertyChanged
    {
        private readonly PacienteRepositorio _repositorio;
        private readonly Action<Paciente> _abrirExpedienteCallback;

        private List<Paciente> _pacientesAll;

        private ObservableCollection<Paciente> _pacientes;
        public ObservableCollection<Paciente> Pacientes
        {
            get => _pacientes;
            set
            {
                _pacientes = value;
                OnPropertyChanged(nameof(Pacientes));
            }
        }

        private Paciente _pacienteSeleccionado;
        public Paciente PacienteSeleccionado
        {
            get => _pacienteSeleccionado;
            set
            {
                if (_pacienteSeleccionado == value)
                    return;

                _pacienteSeleccionado = value;
                OnPropertyChanged(nameof(PacienteSeleccionado));

                if (_pacienteSeleccionado != null)
                {
                    AbrirExpediente(_pacienteSeleccionado);
                }
            }
        }

        private string _textoBusqueda;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged(nameof(TextoBusqueda));
                FiltrarPacientes();
            }
        }

        private bool _estaCargando;
        public bool EstaCargando
        {
            get => _estaCargando;
            set
            {
                _estaCargando = value;
                OnPropertyChanged(nameof(EstaCargando));
            }
        }

        public ICommand AbrirExpedienteCommand { get; }

        public PacientesVM(PacienteRepositorio repositorio)
            : this(repositorio, null)
        {
        }

        public PacientesVM(PacienteRepositorio repositorio, Action<Paciente> abrirExpedienteCallback)
        {
            _repositorio = repositorio;
            _abrirExpedienteCallback = abrirExpedienteCallback;

            _pacientesAll = new List<Paciente>();
            _pacientes = new ObservableCollection<Paciente>();

            AbrirExpedienteCommand = new RelayCommand<Paciente>(AbrirExpediente);

            _ = CargarPacientesAsync();
        }

        private void AbrirExpediente(Paciente paciente)
        {
            try
            {
                if (paciente == null)
                    return;

                _abrirExpedienteCallback?.Invoke(paciente);
            }
            catch (Exception ex)
            {
                ControladorExcepciones.ManejarExcepcion(ex, "AbrirExpediente");
            }
        }

        private async Task CargarPacientesAsync()
        {
            try
            {
                EstaCargando = true;

                _pacientesAll = await _repositorio.ObtenerTodosAsync();
                Pacientes = new ObservableCollection<Paciente>(_pacientesAll);
            }
            catch (Exception ex)
            {
                ControladorExcepciones.ManejarExcepcion(ex, "CargarPacientesAsync");

                _pacientesAll = new List<Paciente>();
                Pacientes = new ObservableCollection<Paciente>();
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void FiltrarPacientes()
        {
            try
            {
                if (_pacientesAll == null)
                    return;

                if (string.IsNullOrWhiteSpace(TextoBusqueda))
                {
                    Pacientes = new ObservableCollection<Paciente>(_pacientesAll);
                    return;
                }

                var busqueda = NormalizarTexto(TextoBusqueda);

                var filtrados = _pacientesAll
                    .Where(p =>
                    {
                        var nombre = NormalizarTexto(p.Nombre);
                        var apellido = NormalizarTexto(p.Apellido);
                        var dni = NormalizarTexto(p.DNI);

                        var nombreCompleto = NormalizarTexto($"{p.Nombre} {p.Apellido}");
                        var nombreCompletoInvertido = NormalizarTexto($"{p.Apellido} {p.Nombre}");

                        return nombre.Contains(busqueda)
                            || apellido.Contains(busqueda)
                            || nombreCompleto.Contains(busqueda)
                            || nombreCompletoInvertido.Contains(busqueda)
                            || dni.Contains(busqueda);
                    })
                    .ToList();

                Pacientes = new ObservableCollection<Paciente>(filtrados);
            }
            catch (Exception ex)
            {
                ControladorExcepciones.ManejarExcepcion(ex, "FiltrarPacientes");
            }
        }

        private string NormalizarTexto(string texto)
        {
            return string.IsNullOrWhiteSpace(texto)
                ? string.Empty
                : texto.Trim().ToLower();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}