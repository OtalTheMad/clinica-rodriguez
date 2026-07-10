using ClinicaRodriguez.Commands;
using ClinicaRodriguez.Helpers;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Repositorios;
using ClinicaRodriguez.Vistas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClinicaRodriguez.VistaModelos
{
    public class CitasVM : INotifyPropertyChanged
    {
        private readonly Usuario _usuarioActual;
        private readonly string _connectionString;
        private readonly Action<Paciente> _abrirExpedienteCallback;
        private readonly CitaRepositorio _citaRepositorio;
        private readonly EspecialistaRepositorio _especialistaRepositorio;
        private readonly PacienteRepositorio _pacienteRepositorio;

        private bool _estaInicializando;
        private int _versionCargaCalendario;

        public ObservableCollection<DiaCalendario> DiasCalendario { get; set; }
        public ObservableCollection<Paciente> PacientesEncontrados { get; set; }
        public ObservableCollection<Especialista> Especialistas { get; set; }
        public ObservableCollection<string> HorasDisponibles { get; set; }

        private Especialista _especialistaSeleccionado;
        public Especialista EspecialistaSeleccionado
        {
            get => _especialistaSeleccionado;
            set
            {
                if (_especialistaSeleccionado == value)
                    return;

                _especialistaSeleccionado = value;
                OnPropertyChanged(nameof(EspecialistaSeleccionado));
                OnPropertyChanged(nameof(EspecialistaActivoTexto));

                if (!_estaInicializando)
                    _ = CargarCalendarioAsync();
            }
        }

        private Paciente _pacienteSeleccionado;
        public Paciente PacienteSeleccionado
        {
            get => _pacienteSeleccionado;
            set
            {
                _pacienteSeleccionado = value;
                OnPropertyChanged(nameof(PacienteSeleccionado));
                OnPropertyChanged(nameof(PacienteSeleccionadoTexto));
            }
        }

        private string _filtroPaciente;
        public string FiltroPaciente
        {
            get => _filtroPaciente;
            set
            {
                _filtroPaciente = value;
                OnPropertyChanged(nameof(FiltroPaciente));
            }
        }

        private DateTime _fechaSemanaInicio;
        public DateTime FechaSemanaInicio
        {
            get => _fechaSemanaInicio;
            set
            {
                _fechaSemanaInicio = value;
                OnPropertyChanged(nameof(FechaSemanaInicio));
                OnPropertyChanged(nameof(TituloSemana));
            }
        }

        private DateTime? _fechaCitaSeleccionada;
        public DateTime? FechaCitaSeleccionada
        {
            get => _fechaCitaSeleccionada;
            set
            {
                _fechaCitaSeleccionada = value;
                OnPropertyChanged(nameof(FechaCitaSeleccionada));
            }
        }

        private string _horaCitaTexto;
        public string HoraCitaTexto
        {
            get => _horaCitaTexto;
            set
            {
                _horaCitaTexto = value;
                OnPropertyChanged(nameof(HoraCitaTexto));
            }
        }

        private string _duracionTexto;
        public string DuracionTexto
        {
            get => _duracionTexto;
            set
            {
                _duracionTexto = value;
                OnPropertyChanged(nameof(DuracionTexto));
            }
        }

        private string _notasNuevaCita;
        public string NotasNuevaCita
        {
            get => _notasNuevaCita;
            set
            {
                _notasNuevaCita = value;
                OnPropertyChanged(nameof(NotasNuevaCita));
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

        public string EspecialistaActivoTexto =>
            EspecialistaSeleccionado == null
                ? "Sin especialista asignado a este usuario"
                : $"{EspecialistaSeleccionado.NombreCompleto} - {EspecialistaSeleccionado.Especialidad}";

        public string PacienteSeleccionadoTexto =>
            PacienteSeleccionado == null
                ? "Sin paciente seleccionado"
                : $"{PacienteSeleccionado.Nombre} {PacienteSeleccionado.Apellido}";

        public string TituloSemana =>
            $"{FechaSemanaInicio:dd/MM/yyyy} - {FechaSemanaInicio.AddDays(6):dd/MM/yyyy}";

        public ICommand SemanaAnteriorCommand { get; }
        public ICommand SemanaSiguienteCommand { get; }
        public ICommand SemanaActualCommand { get; }
        public ICommand BuscarPacienteCommand { get; }
        public ICommand SeleccionarPacienteCommand { get; }
        public ICommand CrearCitaCommand { get; }
        public ICommand AbrirCitaCommand { get; }

        public CitasVM(Usuario usuarioActual, string connectionString, Action<Paciente> abrirExpedienteCallback)
        {
            _usuarioActual = usuarioActual;
            _connectionString = connectionString;
            _abrirExpedienteCallback = abrirExpedienteCallback;

            _citaRepositorio = new CitaRepositorio(_connectionString);
            _especialistaRepositorio = new EspecialistaRepositorio(_connectionString);
            _pacienteRepositorio = new PacienteRepositorio(_connectionString);

            DiasCalendario = new ObservableCollection<DiaCalendario>();
            PacientesEncontrados = new ObservableCollection<Paciente>();
            Especialistas = new ObservableCollection<Especialista>();
            HorasDisponibles = new ObservableCollection<string>(GenerarHorasDisponibles());

            FechaSemanaInicio = ObtenerInicioSemana(DateTime.Today);
            FechaCitaSeleccionada = DateTime.Today;
            HoraCitaTexto = "08:00";
            DuracionTexto = "30";
            NotasNuevaCita = string.Empty;

            SemanaAnteriorCommand = new AsyncRelayCommand(async _ => await CambiarSemanaAsync(-7));
            SemanaSiguienteCommand = new AsyncRelayCommand(async _ => await CambiarSemanaAsync(7));
            SemanaActualCommand = new AsyncRelayCommand(async _ => await IrSemanaActualAsync());
            BuscarPacienteCommand = new AsyncRelayCommand(async _ => await BuscarPacientesAsync());
            SeleccionarPacienteCommand = new RelayCommand<Paciente>(SeleccionarPaciente);
            CrearCitaCommand = new AsyncRelayCommand(async _ => await CrearCitaAsync());
            AbrirCitaCommand = new AsyncRelayCommand(async cita => await AbrirCitaAsync(cita as Cita));

            _ = InicializarAsync();
        }

        private async Task InicializarAsync()
        {
            try
            {
                _estaInicializando = true;
                EstaCargando = true;

                Especialistas.Clear();

                var especialistaUsuario = await _especialistaRepositorio.ObtenerPorUsuarioIdAsync(_usuarioActual.ID);

                if (especialistaUsuario == null)
                {
                    DiasCalendario.Clear();
                    MessageBox.Show("El usuario actual no tiene un especialista asociado. No se puede cargar el calendario de citas.");
                    return;
                }

                Especialistas.Add(especialistaUsuario);
                EspecialistaSeleccionado = especialistaUsuario;

                _estaInicializando = false;

                await CargarCalendarioAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar citas: {ex.Message}");
            }
            finally
            {
                _estaInicializando = false;
                EstaCargando = false;
            }
        }

        private async Task CambiarSemanaAsync(int dias)
        {
            FechaSemanaInicio = FechaSemanaInicio.AddDays(dias);
            await CargarCalendarioAsync();
        }

        private async Task IrSemanaActualAsync()
        {
            FechaSemanaInicio = ObtenerInicioSemana(DateTime.Today);
            FechaCitaSeleccionada = DateTime.Today;
            await CargarCalendarioAsync();
        }

        private async Task CargarCalendarioAsync()
        {
            if (EspecialistaSeleccionado == null)
                return;

            int versionActual = ++_versionCargaCalendario;

            try
            {
                EstaCargando = true;

                var nuevosDias = new List<DiaCalendario>();

                for (int i = 0; i < 7; i++)
                {
                    var fecha = FechaSemanaInicio.AddDays(i);

                    nuevosDias.Add(new DiaCalendario
                    {
                        Fecha = fecha,
                        Titulo = fecha.ToString("dddd dd/MM", new CultureInfo("es-HN")),
                        Citas = new ObservableCollection<Cita>()
                    });
                }

                var fechaFin = FechaSemanaInicio.AddDays(7);

                var citas = await _citaRepositorio.ObtenerPorEspecialistaYRangoAsync(
                    EspecialistaSeleccionado.Id,
                    FechaSemanaInicio,
                    fechaFin);

                if (versionActual != _versionCargaCalendario)
                    return;

                foreach (var cita in citas)
                {
                    var dia = nuevosDias.FirstOrDefault(d => d.Fecha.Date == cita.FechaCita.Date);

                    if (dia != null)
                        dia.Citas.Add(cita);
                }

                DiasCalendario.Clear();

                foreach (var dia in nuevosDias)
                    DiasCalendario.Add(dia);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar calendario: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task BuscarPacientesAsync()
        {
            try
            {
                PacientesEncontrados.Clear();

                if (string.IsNullOrWhiteSpace(FiltroPaciente))
                {
                    MessageBox.Show("Ingrese nombre, apellido, identidad o teléfono.");
                    return;
                }

                EstaCargando = true;

                var pacientes = await _pacienteRepositorio.ObtenerTodosAsync();
                var busqueda = NormalizarTexto(FiltroPaciente);

                var filtrados = pacientes
                    .Where(p =>
                    {
                        var nombre = NormalizarTexto(p.Nombre);
                        var apellido = NormalizarTexto(p.Apellido);
                        var dni = NormalizarTexto(p.DNI);
                        var telefono = NormalizarTexto(p.TelefonoPrimario);
                        var nombreCompleto = NormalizarTexto($"{p.Nombre} {p.Apellido}");
                        var nombreCompletoInvertido = NormalizarTexto($"{p.Apellido} {p.Nombre}");

                        return nombre.Contains(busqueda)
                            || apellido.Contains(busqueda)
                            || dni.Contains(busqueda)
                            || telefono.Contains(busqueda)
                            || nombreCompleto.Contains(busqueda)
                            || nombreCompletoInvertido.Contains(busqueda);
                    })
                    .ToList();

                foreach (var paciente in filtrados)
                    PacientesEncontrados.Add(paciente);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar pacientes: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void SeleccionarPaciente(Paciente paciente)
        {
            if (paciente == null)
                return;

            PacienteSeleccionado = paciente;
            PacientesEncontrados.Clear();
            FiltroPaciente = string.Empty;
        }

        private async Task CrearCitaAsync()
        {
            try
            {
                if (PacienteSeleccionado == null)
                {
                    MessageBox.Show("Seleccione un paciente antes de crear la cita.");
                    return;
                }

                if (EspecialistaSeleccionado == null)
                {
                    MessageBox.Show("El usuario actual no tiene especialista asociado.");
                    return;
                }

                if (!FechaCitaSeleccionada.HasValue)
                {
                    MessageBox.Show("Seleccione una fecha para la cita.");
                    return;
                }

                if (!TryParseHora(HoraCitaTexto, out TimeSpan hora))
                {
                    MessageBox.Show("Seleccione una hora válida.");
                    return;
                }

                if (!int.TryParse(DuracionTexto, out int duracion) || duracion <= 0)
                {
                    MessageBox.Show("Ingrese una duración válida en minutos.");
                    return;
                }

                EstaCargando = true;

                var fechaCita = FechaCitaSeleccionada.Value.Date.Add(hora);

                var cita = new Cita
                {
                    PacienteId = PacienteSeleccionado.Id,
                    EspecialistaId = EspecialistaSeleccionado.Id,
                    FechaCita = fechaCita,
                    Estado = EstadosCita.Pendiente,
                    Duracion = duracion,
                    Notas = NotasNuevaCita ?? string.Empty,
                    CreadoPor = _usuarioActual.ID
                };

                cita.Id = await _citaRepositorio.CrearAsync(cita);

                FechaSemanaInicio = ObtenerInicioSemana(fechaCita);
                FechaCitaSeleccionada = fechaCita.Date;
                HoraCitaTexto = ObtenerSiguienteHoraDisponible(fechaCita, duracion);
                NotasNuevaCita = string.Empty;

                await CargarCalendarioAsync();

                MessageBox.Show("Cita creada correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear cita: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task AbrirCitaAsync(Cita cita)
        {
            if (cita == null)
                return;

            var popup = new CitaDetallePopup();
            var vm = new CitaDetalleVM(
                cita,
                _citaRepositorio,
                _abrirExpedienteCallback,
                () => popup.Close());

            popup.DataContext = vm;
            popup.Owner = Application.Current.MainWindow;
            popup.ShowDialog();

            await CargarCalendarioAsync();
        }

        private static DateTime ObtenerInicioSemana(DateTime fecha)
        {
            int diferencia = ((int)fecha.DayOfWeek + 6) % 7;
            return fecha.Date.AddDays(-diferencia);
        }

        private static bool TryParseHora(string texto, out TimeSpan hora)
        {
            hora = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(texto))
                return false;

            return TimeSpan.TryParseExact(texto.Trim(), "hh\\:mm", CultureInfo.InvariantCulture, out hora);
        }

        private static IEnumerable<string> GenerarHorasDisponibles()
        {
            var horaInicio = new TimeSpan(6, 0, 0);
            var horaFin = new TimeSpan(20, 0, 0);

            for (var hora = horaInicio; hora <= horaFin; hora = hora.Add(TimeSpan.FromMinutes(15)))
                yield return hora.ToString(@"hh\:mm");
        }

        private string ObtenerSiguienteHoraDisponible(DateTime fechaCita, int duracion)
        {
            var siguienteHora = fechaCita.AddMinutes(duracion).TimeOfDay;
            var texto = siguienteHora.ToString(@"hh\:mm");

            return HorasDisponibles.Contains(texto)
                ? texto
                : "08:00";
        }

        private static string NormalizarTexto(string texto)
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

    public class DiaCalendario
    {
        public DateTime Fecha { get; set; }
        public string Titulo { get; set; }
        public ObservableCollection<Cita> Citas { get; set; }
    }
}