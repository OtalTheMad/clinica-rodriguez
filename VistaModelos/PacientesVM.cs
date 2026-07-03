// VistaModelos/PacientesVM.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Repositorios;
using ClinicaRodriguez.Utilidades;

namespace ClinicaRodriguez.VistaModelos
{
    public class PacientesVM : INotifyPropertyChanged
    {
        private readonly PacienteRepositorio _repositorio;
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

        public PacientesVM(PacienteRepositorio repositorio)
        {
            _repositorio = repositorio;
            _pacientes = new ObservableCollection<Paciente>();
            _ = CargarPacientesAsync();
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
                if (_pacientesAll == null) return;

                if (string.IsNullOrWhiteSpace(TextoBusqueda))
                {
                    Pacientes = new ObservableCollection<Paciente>(_pacientesAll);
                }
                else
                {
                    var busqueda = TextoBusqueda.ToLower();
                    var filtrados = _pacientesAll
                        .Where(p =>
                            (p.Nombre?.ToLower().Contains(busqueda) ?? false) ||
                            (p.Apellido?.ToLower().Contains(busqueda) ?? false) ||
                            (p.DNI?.ToLower().Contains(busqueda) ?? false))
                        .ToList();
                    Pacientes = new ObservableCollection<Paciente>(filtrados);
                }
            }
            catch (Exception ex)
            {
                ControladorExcepciones.ManejarExcepcion(ex, "FiltrarPacientes");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}