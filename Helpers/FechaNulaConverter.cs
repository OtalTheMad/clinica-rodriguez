using Microsoft.VisualBasic;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace ClinicaRodriguez.Helpers
{
    public class FechaNulaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            if (value is DateTime fecha)
            {
                if (fecha == DateTime.MinValue || fecha == default)
                    return string.Empty;

                return fecha.ToString("dd/MM/yyyy");
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return null;

            if (DateTime.TryParse(value.ToString(), out DateTime fecha))
                return fecha;

            return null;
        }
    }
}