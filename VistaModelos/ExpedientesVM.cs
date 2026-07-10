using ClinicaRodriguez.Commands;
using ClinicaRodriguez.Helpers;
using ClinicaRodriguez.Modelos;
using ClinicaRodriguez.Repositorios;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClinicaRodriguez.VistaModelos
{
    public class ExpedientesVM : INotifyPropertyChanged
    {
        private readonly string _connectionString;
        private readonly Usuario _usuarioActual;
        private readonly ExpedienteRepositorio _expedienteRepositorio;
        private readonly PacienteRepositorio _pacienteRepositorio;
        private readonly DocumentoAdjuntoRepositorio _documentoAdjuntoRepositorio;
        private readonly CitaRepositorio _citaRepositorio;
        private readonly EspecialistaRepositorio _especialistaRepositorio;

        private string _temperaturaTexto;
        private string _pesoTexto;
        private string _tallaTexto;
        private string _imcTexto;

        public ObservableCollection<Paciente> PacientesEncontrados { get; set; }
        public ObservableCollection<DocumentoAdjunto> DocumentosAdjuntos { get; set; }
        public ObservableCollection<Cita> HistorialCitas { get; set; }

        private Paciente _pacienteSeleccionado;
        public Paciente PacienteSeleccionado
        {
            get => _pacienteSeleccionado;
            set
            {
                _pacienteSeleccionado = value;
                OnPropertyChanged(nameof(PacienteSeleccionado));
                OnPropertyChanged(nameof(TienePacienteSeleccionado));
                OnPropertyChanged(nameof(MostrarSelectorPaciente));
                OnPropertyChanged(nameof(TituloExpediente));
            }
        }

        private Expediente _expedienteSeleccionado;
        public Expediente ExpedienteSeleccionado
        {
            get => _expedienteSeleccionado;
            set
            {
                _expedienteSeleccionado = value;
                SincronizarCamposClinicosDesdeExpediente();
                OnPropertyChanged(nameof(ExpedienteSeleccionado));
                OnPropertyChanged(nameof(NumeroExpediente));
            }
        }

        private DocumentoAdjunto _documentoSeleccionado;
        public DocumentoAdjunto DocumentoSeleccionado
        {
            get => _documentoSeleccionado;
            set
            {
                _documentoSeleccionado = value;
                OnPropertyChanged(nameof(DocumentoSeleccionado));
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

        public string NumeroExpediente =>
            ExpedienteSeleccionado != null && ExpedienteSeleccionado.Id > 0
                ? ExpedienteSeleccionado.Id.ToString()
                : "Nuevo";

        public string TemperaturaTexto
        {
            get => _temperaturaTexto;
            set
            {
                _temperaturaTexto = value;
                OnPropertyChanged(nameof(TemperaturaTexto));

                if (ExpedienteSeleccionado == null)
                    return;

                ExpedienteSeleccionado.Temperatura = ConvertirDecimalFlexible(value);
            }
        }

        public string PesoTexto
        {
            get => _pesoTexto;
            set
            {
                _pesoTexto = value;
                OnPropertyChanged(nameof(PesoTexto));

                if (ExpedienteSeleccionado == null)
                    return;

                ExpedienteSeleccionado.Peso = ConvertirDecimalFlexible(value);
                CalcularIMC();
            }
        }

        public string TallaTexto
        {
            get => _tallaTexto;
            set
            {
                _tallaTexto = value;
                OnPropertyChanged(nameof(TallaTexto));

                if (ExpedienteSeleccionado == null)
                    return;

                ExpedienteSeleccionado.Talla = ConvertirDecimalFlexible(value);
                CalcularIMC();
            }
        }

        public string IMCTexto
        {
            get => _imcTexto;
            set
            {
                _imcTexto = value;
                OnPropertyChanged(nameof(IMCTexto));
            }
        }

        public bool EsOdontologo => SesionActual.EsOdontologo;

        public bool TienePacienteSeleccionado => PacienteSeleccionado != null;

        public bool MostrarSelectorPaciente => PacienteSeleccionado == null;

        public string TituloExpediente =>
            PacienteSeleccionado == null
                ? "Nuevo expediente - seleccione un paciente"
                : $"Expediente de {PacienteSeleccionado.Nombre} {PacienteSeleccionado.Apellido}";

        public ICommand BuscarPacienteCommand { get; }
        public ICommand AsociarPacienteCommand { get; }
        public ICommand GuardarExpedienteCommand { get; }
        public ICommand AdjuntarDocumentoCommand { get; }
        public ICommand AbrirDocumentoCommand { get; }
        public ICommand EliminarDocumentoCommand { get; }
        public ICommand SalirCommand { get; }

        public ExpedientesVM(Usuario usuarioActual, string connectionString)
            : this(usuarioActual, connectionString, null)
        {
        }

        public ExpedientesVM(Usuario usuarioActual, string connectionString, Paciente paciente)
        {
            _usuarioActual = usuarioActual;
            _connectionString = connectionString;

            _expedienteRepositorio = new ExpedienteRepositorio(_connectionString);
            _pacienteRepositorio = new PacienteRepositorio(_connectionString);
            _documentoAdjuntoRepositorio = new DocumentoAdjuntoRepositorio(_connectionString);
            _citaRepositorio = new CitaRepositorio(_connectionString);
            _especialistaRepositorio = new EspecialistaRepositorio(_connectionString);

            PacientesEncontrados = new ObservableCollection<Paciente>();
            DocumentosAdjuntos = new ObservableCollection<DocumentoAdjunto>();
            HistorialCitas = new ObservableCollection<Cita>();

            BuscarPacienteCommand = new AsyncRelayCommand(async _ => await BuscarPacientesAsync());
            AsociarPacienteCommand = new RelayCommand<Paciente>(AsociarPaciente);
            GuardarExpedienteCommand = new AsyncRelayCommand(async _ => await GuardarExpedienteAsync());
            AdjuntarDocumentoCommand = new AsyncRelayCommand(async _ => await AdjuntarDocumentoAsync());
            AbrirDocumentoCommand = new RelayCommand<DocumentoAdjunto>(AbrirDocumento);
            EliminarDocumentoCommand = new AsyncRelayCommand(async documento => await EliminarDocumentoAsync(documento as DocumentoAdjunto));
            SalirCommand = new RelayCommand<object>(_ => SalirDelExpediente());

            if (paciente != null)
            {
                AsociarPaciente(paciente);
            }
            else
            {
                ExpedienteSeleccionado = new Expediente
                {
                    CreadoPor = _usuarioActual.ID,
                    CreadoEn = DateTime.Now
                };
            }
        }

        private async Task BuscarPacientesAsync()
        {
            try
            {
                PacientesEncontrados.Clear();

                if (string.IsNullOrWhiteSpace(FiltroPaciente))
                {
                    MessageBox.Show("Ingrese un nombre, apellido, identidad o teléfono para buscar.");
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
                            || nombreCompleto.Contains(busqueda)
                            || nombreCompletoInvertido.Contains(busqueda)
                            || dni.Contains(busqueda)
                            || telefono.Contains(busqueda);
                    })
                    .ToList();

                foreach (var paciente in filtrados)
                {
                    PacientesEncontrados.Add(paciente);
                }
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

        private void AsociarPaciente(Paciente paciente)
        {
            if (paciente == null)
                return;

            PacienteSeleccionado = paciente;

            _ = CargarExpedientePorPacienteAsync(paciente);
        }

        private async Task CargarExpedientePorPacienteAsync(Paciente paciente)
        {
            try
            {
                if (paciente == null)
                    return;

                EstaCargando = true;

                string servicio = EsOdontologo ? "Odontologia" : "ConsultaGeneral";

                ExpedienteSeleccionado = await _expedienteRepositorio.ObtenerOCrearPorPacienteIdAsync(
                    paciente.Id,
                    _usuarioActual.ID,
                    servicio
                );

                await CargarDocumentosAdjuntosAsync();
                await CargarHistorialCitasAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el expediente: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task GuardarExpedienteAsync()
        {
            await GuardarExpedienteInternoAsync(true);
        }

        private async Task<bool> GuardarExpedienteInternoAsync(bool mostrarMensaje)
        {
            try
            {
                if (PacienteSeleccionado == null)
                {
                    MessageBox.Show("Debe seleccionar un paciente antes de guardar el expediente.");
                    return false;
                }

                if (ExpedienteSeleccionado == null)
                {
                    MessageBox.Show("No hay expediente para guardar.");
                    return false;
                }

                EstaCargando = true;

                AplicarCamposClinicosAlExpediente();

                ExpedienteSeleccionado.PacienteId = PacienteSeleccionado.Id;
                ExpedienteSeleccionado.ModificadoPor = _usuarioActual.ID;

                if (ExpedienteSeleccionado.Id == 0)
                {
                    ExpedienteSeleccionado.CreadoPor = _usuarioActual.ID;
                    ExpedienteSeleccionado.CreadoEn = DateTime.Now;
                    ExpedienteSeleccionado.Id = await _expedienteRepositorio.CrearAsync(ExpedienteSeleccionado);
                    OnPropertyChanged(nameof(NumeroExpediente));
                }
                else
                {
                    if (EsOdontologo)
                    {
                        await _expedienteRepositorio.ActualizarOdontologiaAsync(ExpedienteSeleccionado);
                    }
                    else
                    {
                        await _expedienteRepositorio.ActualizarConsultaGeneralAsync(ExpedienteSeleccionado);
                    }
                }

                await CargarDocumentosAdjuntosAsync();

                if (mostrarMensaje)
                    MessageBox.Show("Expediente guardado correctamente.");

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el expediente: {ex.Message}");
                return false;
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task AdjuntarDocumentoAsync()
        {
            try
            {
                if (PacienteSeleccionado == null)
                {
                    MessageBox.Show("Debe seleccionar un paciente antes de adjuntar documentos.");
                    return;
                }

                if (ExpedienteSeleccionado == null)
                {
                    MessageBox.Show("No hay expediente activo.");
                    return;
                }

                if (ExpedienteSeleccionado.Id == 0)
                {
                    bool guardado = await GuardarExpedienteInternoAsync(false);

                    if (!guardado)
                        return;
                }

                var dialogo = new OpenFileDialog
                {
                    Title = "Seleccionar documento adjunto",
                    Filter = "Documentos e imágenes|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.png;*.jpg;*.jpeg;*.txt|Todos los archivos|*.*",
                    Multiselect = true
                };

                bool? resultado = dialogo.ShowDialog();

                if (resultado != true)
                    return;

                EstaCargando = true;

                foreach (var archivoOrigen in dialogo.FileNames)
                {
                    var documento = await GuardarArchivoFisicoYRegistrarAsync(archivoOrigen);
                    DocumentosAdjuntos.Insert(0, documento);
                }

                MessageBox.Show("Documento adjuntado correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al adjuntar documento: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task<DocumentoAdjunto> GuardarArchivoFisicoYRegistrarAsync(string archivoOrigen)
        {
            var carpetaDestino = ObtenerCarpetaExpediente();
            Directory.CreateDirectory(carpetaDestino);

            var nombreOriginal = Path.GetFileName(archivoOrigen);
            var extension = Path.GetExtension(archivoOrigen);
            var nombreSinExtension = Path.GetFileNameWithoutExtension(archivoOrigen);
            var nombreSeguro = LimpiarNombreArchivo(nombreSinExtension);
            var nombreFinal = $"{nombreSeguro}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            var rutaDestino = Path.Combine(carpetaDestino, nombreFinal);

            File.Copy(archivoOrigen, rutaDestino, false);

            var documento = new DocumentoAdjunto
            {
                ExpedienteId = ExpedienteSeleccionado.Id,
                NombreArchivo = nombreOriginal,
                RutaArchivo = rutaDestino,
                TipoArchivo = string.IsNullOrWhiteSpace(extension) ? "archivo" : extension.TrimStart('.').ToLower(),
                Descripcion = string.Empty,
                CreadoEn = DateTime.Now,
                CreadoPor = _usuarioActual.ID
            };

            documento.Id = await _documentoAdjuntoRepositorio.CrearAsync(documento);

            return documento;
        }

        private async Task CargarDocumentosAdjuntosAsync()
        {
            DocumentosAdjuntos.Clear();
            DocumentoSeleccionado = null;

            if (ExpedienteSeleccionado == null || ExpedienteSeleccionado.Id == 0)
                return;

            var documentos = await _documentoAdjuntoRepositorio.ObtenerPorExpedienteIdAsync(ExpedienteSeleccionado.Id);

            foreach (var documento in documentos)
            {
                DocumentosAdjuntos.Add(documento);
            }
        }

        private async Task CargarHistorialCitasAsync()
        {
            try
            {
                HistorialCitas.Clear();

                if (PacienteSeleccionado == null)
                    return;

                var especialistaActual = await _especialistaRepositorio.ObtenerPorUsuarioIdAsync(_usuarioActual.ID);

                if (especialistaActual == null)
                    return;

                var citas = await _citaRepositorio.ObtenerHistorialPorPacienteYEspecialistaAsync(
                    PacienteSeleccionado.Id,
                    especialistaActual.Id
                );

                foreach (var cita in citas)
                {
                    HistorialCitas.Add(cita);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial de citas: {ex.Message}");
            }
        }

        private void AbrirDocumento(DocumentoAdjunto documento)
        {
            try
            {
                documento ??= DocumentoSeleccionado;

                if (documento == null)
                {
                    MessageBox.Show("Seleccione un documento para abrir.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(documento.RutaArchivo) || !File.Exists(documento.RutaArchivo))
                {
                    MessageBox.Show("El archivo no existe en la ruta registrada.");
                    return;
                }

                var proceso = new ProcessStartInfo
                {
                    FileName = documento.RutaArchivo,
                    UseShellExecute = true
                };

                Process.Start(proceso);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir documento: {ex.Message}");
            }
        }

        private async Task EliminarDocumentoAsync(DocumentoAdjunto documento)
        {
            try
            {
                documento ??= DocumentoSeleccionado;

                if (documento == null)
                {
                    MessageBox.Show("Seleccione un documento para eliminar.");
                    return;
                }

                var confirmacion = MessageBox.Show(
                    $"¿Desea eliminar el documento \"{documento.NombreArchivo}\"?",
                    "Eliminar documento",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmacion != MessageBoxResult.Yes)
                    return;

                EstaCargando = true;

                if (!string.IsNullOrWhiteSpace(documento.RutaArchivo) && File.Exists(documento.RutaArchivo))
                {
                    File.Delete(documento.RutaArchivo);
                }

                await _documentoAdjuntoRepositorio.EliminarAsync(documento.Id);

                DocumentosAdjuntos.Remove(documento);
                DocumentoSeleccionado = null;

                MessageBox.Show("Documento eliminado correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar documento: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void SalirDelExpediente()
        {
            PacienteSeleccionado = null;
            ExpedienteSeleccionado = new Expediente
            {
                CreadoPor = _usuarioActual.ID,
                CreadoEn = DateTime.Now
            };

            DocumentoSeleccionado = null;
            FiltroPaciente = string.Empty;

            PacientesEncontrados.Clear();
            DocumentosAdjuntos.Clear();
            HistorialCitas.Clear();

            OnPropertyChanged(nameof(NumeroExpediente));
        }

        private void SincronizarCamposClinicosDesdeExpediente()
        {
            _temperaturaTexto = FormatearDecimal(ExpedienteSeleccionado?.Temperatura);
            _pesoTexto = FormatearDecimal(ExpedienteSeleccionado?.Peso);
            _tallaTexto = FormatearDecimal(ExpedienteSeleccionado?.Talla);
            _imcTexto = FormatearDecimal(ExpedienteSeleccionado?.IndiceMasaCorporal);

            OnPropertyChanged(nameof(TemperaturaTexto));
            OnPropertyChanged(nameof(PesoTexto));
            OnPropertyChanged(nameof(TallaTexto));
            OnPropertyChanged(nameof(IMCTexto));
        }

        private void AplicarCamposClinicosAlExpediente()
        {
            if (ExpedienteSeleccionado == null)
                return;

            ExpedienteSeleccionado.Temperatura = ConvertirDecimalFlexible(TemperaturaTexto);
            ExpedienteSeleccionado.Peso = ConvertirDecimalFlexible(PesoTexto);
            ExpedienteSeleccionado.Talla = ConvertirDecimalFlexible(TallaTexto);

            CalcularIMC();
        }

        private void CalcularIMC()
        {
            if (ExpedienteSeleccionado == null)
                return;

            if (!ExpedienteSeleccionado.Peso.HasValue || !ExpedienteSeleccionado.Talla.HasValue)
            {
                ExpedienteSeleccionado.IndiceMasaCorporal = null;
                IMCTexto = string.Empty;
                return;
            }

            decimal peso = ExpedienteSeleccionado.Peso.Value;
            decimal talla = ExpedienteSeleccionado.Talla.Value;

            if (peso <= 0 || talla <= 0)
            {
                ExpedienteSeleccionado.IndiceMasaCorporal = null;
                IMCTexto = string.Empty;
                return;
            }

            if (talla > 3)
                talla /= 100;

            ExpedienteSeleccionado.IndiceMasaCorporal = Math.Round(peso / (talla * talla), 2);
            IMCTexto = FormatearDecimal(ExpedienteSeleccionado.IndiceMasaCorporal);
        }

        private decimal? ConvertirDecimalFlexible(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            valor = valor.Trim().Replace(',', '.');

            if (valor == "." || valor == "-")
                return null;

            if (decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal resultado))
                return resultado;

            return null;
        }

        private string FormatearDecimal(decimal? valor)
        {
            return valor.HasValue
                ? valor.Value.ToString("0.##", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private string ObtenerCarpetaExpediente()
        {
            var carpetaBase = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Docs",
                "Expedientes");

            return Path.Combine(carpetaBase, ExpedienteSeleccionado.Id.ToString());
        }

        private static string LimpiarNombreArchivo(string nombre)
        {
            var caracteresInvalidos = Path.GetInvalidFileNameChars();

            foreach (var caracter in caracteresInvalidos)
            {
                nombre = nombre.Replace(caracter, '_');
            }

            return string.IsNullOrWhiteSpace(nombre) ? "documento" : nombre.Trim();
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
}