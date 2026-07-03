using System;
using System.Windows;

namespace ClinicaRodriguez.Utilidades
{
    public static class ControladorExcepciones
    {
        public static void MostrarError(string mensaje, string titulo = "Error")
        {
            MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void MostrarAdvertencia(string mensaje, string titulo = "Advertencia")
        {
            MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void MostrarInfo(string mensaje, string titulo = "Información")
        {
            MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void RegistrarError(Exception ex, string contexto = "")
        {
            // Log to console (you can later change this to a file or database)
            Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Contexto: {contexto}");
            Console.WriteLine($"Mensaje: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            Console.WriteLine(new string('-', 50));
        }

        public static void ManejarExcepcion(Exception ex, string contexto = "", bool mostrarUsuario = true)
        {
            RegistrarError(ex, contexto);

            if (mostrarUsuario)
            {
                var mensajeUsuario = ObtenerMensajeAmigable(ex);
                MostrarError(mensajeUsuario);
            }
        }

        private static string ObtenerMensajeAmigable(Exception ex)
        {
            return ex switch
            {
                Microsoft.Data.SqlClient.SqlException sqlEx => ObtenerMensajeSql(sqlEx),
                TimeoutException => "La operación tardó demasiado. Por favor, intente de nuevo.",
                InvalidOperationException => "Operación no válida. Por favor, intente de nuevo.",
                ArgumentNullException => "Faltan datos requeridos. Por favor, complete todos los campos.",
                _ => "Ocurrió un error inesperado. Por favor, intente de nuevo o contacte al administrador."
            };
        }

        private static string ObtenerMensajeSql(Microsoft.Data.SqlClient.SqlException ex)
        {
            return ex.Number switch
            {
                -2 => "Tiempo de espera agotado al conectar con la base de datos.",
                53 => "No se pudo conectar al servidor de base de datos.",
                4060 => "No se pudo acceder a la base de datos.",
                18456 => "Credenciales de base de datos inválidas.",
                2627 => "Ya existe un registro con esos datos.",
                547 => "No se puede eliminar este registro porque tiene datos relacionados.",
                _ => "Error al acceder a la base de datos. Por favor, intente de nuevo."
            };
        }
    }
}