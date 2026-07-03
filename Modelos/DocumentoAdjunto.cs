using System;

namespace ClinicaRodriguez.Modelos
{
    public class DocumentoAdjunto
    {
        public int Id { get; set; }
        public int ExpedienteId { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
        public int CreadoPor { get; set; }
    }
}
