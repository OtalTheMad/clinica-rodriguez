using ClinicaRodriguez.Commands;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Repositorios;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClinicaRodriguez.VistaModelos
{
    public class EspecialistasVM : INotifyPropertyChanged
    {
        private readonly Usuario _usuarioActual;
        private readonly EspecialistaRepositorio _especialistaRepositorio;
        private ObservableCollection<Especialista> _especialistasOriginales;

        public ObservableCollection<Especialista> Especialistas { get; set; }
        public ObservableCollection<RolUsuarioOpcion> RolesDisponibles { get; set; }

        private int _id;
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
                OnPropertyChanged(nameof(EsEdicion));
                OnPropertyChanged(nameof(TextoBotonGuardar));
            }
        }

        private int? _usuarioId;
        public int? UsuarioId
        {
            get => _usuarioId;
            set
            {
                _usuarioId = value;
                OnPropertyChanged(nameof(UsuarioId));
            }
        }

        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set
            {
                _nombre = value;
                OnPropertyChanged(nameof(Nombre));
            }
        }

        private string _apellido;
        public string Apellido
        {
            get => _apellido;
            set
            {
                _apellido = value;
                OnPropertyChanged(nameof(Apellido));
            }
        }

        private string _especialidad;
        public string Especialidad
        {
            get => _especialidad;
            set
            {
                _especialidad = value;
                OnPropertyChanged(nameof(Especialidad));
            }
        }

        private string _graduadoEn;
        public string GraduadoEn
        {
            get => _graduadoEn;
            set
            {
                _graduadoEn = value;
                OnPropertyChanged(nameof(GraduadoEn));
            }
        }

        private string _nombreUsuario;
        public string NombreUsuario
        {
            get => _nombreUsuario;
            set
            {
                _nombreUsuario = value;
                OnPropertyChanged(nameof(NombreUsuario));
            }
        }

        private string _claveTemporal;
        public string ClaveTemporal
        {
            get => _claveTemporal;
            set
            {
                _claveTemporal = value;
                OnPropertyChanged(nameof(ClaveTemporal));
            }
        }

        private int _rolIdSeleccionado;
        public int RolIdSeleccionado
        {
            get => _rolIdSeleccionado;
            set
            {
                _rolIdSeleccionado = value;
                OnPropertyChanged(nameof(RolIdSeleccionado));
            }
        }

        private string _filtroBusqueda;
        public string FiltroBusqueda
        {
            get => _filtroBusqueda;
            set
            {
                _filtroBusqueda = value;
                OnPropertyChanged(nameof(FiltroBusqueda));
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

        public bool EsEdicion => Id > 0;

        public string TextoBotonGuardar => EsEdicion ? "Actualizar especialista" : "Crear especialista";

        public ICommand CargarCommand { get; }
        public ICommand BuscarCommand { get; }
        public ICommand NuevoCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand SeleccionarCommand { get; }
        public ICommand DesactivarCommand { get; }

        public EspecialistasVM(Usuario usuarioActual, string connectionString)
        {
            _usuarioActual = usuarioActual;
            _especialistaRepositorio = new EspecialistaRepositorio(connectionString);

            _especialistasOriginales = new ObservableCollection<Especialista>();
            Especialistas = new ObservableCollection<Especialista>();

            RolesDisponibles = new ObservableCollection<RolUsuarioOpcion>
            {
                new RolUsuarioOpcion { Id = 1, Nombre = "Consulta General" },
                new RolUsuarioOpcion { Id = 2, Nombre = "Odontología" }
            };

            RolIdSeleccionado = 1;

            CargarCommand = new AsyncRelayCommand(async _ => await CargarEspecialistasAsync());
            BuscarCommand = new RelayCommand<object>(_ => AplicarFiltro());
            NuevoCommand = new RelayCommand<object>(_ => LimpiarFormulario());
            GuardarCommand = new AsyncRelayCommand(async _ => await GuardarAsync());
            SeleccionarCommand = new RelayCommand<Especialista>(SeleccionarEspecialista);
            DesactivarCommand = new AsyncRelayCommand(async especialista => await DesactivarAsync(especialista as Especialista));

            _ = CargarEspecialistasAsync();
        }

        private async Task CargarEspecialistasAsync()
        {
            try
            {
                EstaCargando = true;

                var especialistas = await _especialistaRepositorio.ObtenerTodosAsync(false);

                _especialistasOriginales.Clear();
                Especialistas.Clear();

                foreach (var especialista in especialistas)
                {
                    _especialistasOriginales.Add(especialista);
                    Especialistas.Add(especialista);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar especialistas: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task GuardarAsync()
        {
            try
            {
                if (!Validar())
                    return;

                EstaCargando = true;

                var especialista = new Especialista
                {
                    Id = Id,
                    UsuarioId = UsuarioId,
                    Nombre = Nombre,
                    Apellido = Apellido,
                    Especialidad = Especialidad,
                    GraduadoEn = GraduadoEn,
                    NombreUsuario = NombreUsuario,
                    ClaveTemporal = ClaveTemporal,
                    RolId = RolIdSeleccionado,
                    CreadoPor = _usuarioActual.ID
                };

                if (Id == 0)
                {
                    await _especialistaRepositorio.CrearAsync(especialista, _usuarioActual.ID);
                    MessageBox.Show("Especialista creado correctamente.");
                }
                else
                {
                    await _especialistaRepositorio.ActualizarAsync(especialista);
                    MessageBox.Show("Especialista actualizado correctamente.");
                }

                LimpiarFormulario();
                await CargarEspecialistasAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar especialista: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void SeleccionarEspecialista(Especialista especialista)
        {
            if (especialista == null)
                return;

            Id = especialista.Id;
            UsuarioId = especialista.UsuarioId;
            Nombre = especialista.Nombre;
            Apellido = especialista.Apellido;
            Especialidad = especialista.Especialidad;
            GraduadoEn = especialista.GraduadoEn;
            NombreUsuario = especialista.NombreUsuario;
            ClaveTemporal = string.Empty;
            RolIdSeleccionado = especialista.RolId <= 0 ? 1 : especialista.RolId;
        }

        private async Task DesactivarAsync(Especialista especialista)
        {
            try
            {
                if (especialista == null)
                {
                    MessageBox.Show("Seleccione un especialista.");
                    return;
                }

                var confirmacion = MessageBox.Show(
                    $"¿Desea desactivar al usuario de {especialista.NombreCompleto}?",
                    "Desactivar especialista",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmacion != MessageBoxResult.Yes)
                    return;

                EstaCargando = true;

                await _especialistaRepositorio.DesactivarUsuarioAsync(especialista.Id);

                if (Id == especialista.Id)
                    LimpiarFormulario();

                await CargarEspecialistasAsync();

                MessageBox.Show("Usuario desactivado correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desactivar especialista: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void AplicarFiltro()
        {
            Especialistas.Clear();

            var busqueda = Normalizar(FiltroBusqueda);

            var filtrados = _especialistasOriginales
                .Where(e =>
                    string.IsNullOrWhiteSpace(busqueda)
                    || Normalizar(e.Nombre).Contains(busqueda)
                    || Normalizar(e.Apellido).Contains(busqueda)
                    || Normalizar(e.NombreCompleto).Contains(busqueda)
                    || Normalizar(e.Especialidad).Contains(busqueda)
                    || Normalizar(e.GraduadoEn).Contains(busqueda)
                    || Normalizar(e.NombreUsuario).Contains(busqueda))
                .ToList();

            foreach (var especialista in filtrados)
                Especialistas.Add(especialista);
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MessageBox.Show("Ingrese el nombre del especialista.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Apellido))
            {
                MessageBox.Show("Ingrese el apellido del especialista.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Especialidad))
            {
                MessageBox.Show("Ingrese la especialidad.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                MessageBox.Show("Ingrese el nombre de usuario.");
                return false;
            }

            if (Id == 0 && string.IsNullOrWhiteSpace(ClaveTemporal))
            {
                MessageBox.Show("Ingrese una clave temporal para el usuario.");
                return false;
            }

            if (RolIdSeleccionado <= 0)
            {
                MessageBox.Show("Seleccione el rol del usuario.");
                return false;
            }

            return true;
        }

        private void LimpiarFormulario()
        {
            Id = 0;
            UsuarioId = null;
            Nombre = string.Empty;
            Apellido = string.Empty;
            Especialidad = string.Empty;
            GraduadoEn = string.Empty;
            NombreUsuario = string.Empty;
            ClaveTemporal = string.Empty;
            RolIdSeleccionado = 1;
        }

        private static string Normalizar(string texto)
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

    public class RolUsuarioOpcion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}