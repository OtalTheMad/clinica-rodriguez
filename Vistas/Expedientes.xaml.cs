using System.Windows;
using System.Windows.Controls;

namespace ClinicaRodriguez.Vistas
{
    public partial class Expedientes : UserControl
    {
        public Expedientes()
        {
            InitializeComponent();
        }

        private void ScrollAElemento(FrameworkElement elemento)
        {
            if (elemento == null)
                return;

            elemento.BringIntoView();
        }

        private void BtnIdentificacion_Click(object sender, RoutedEventArgs e)
        {
            ScrollAElemento(SeccionIdentificacion);
        }

        private void BtnSignosVitales_Click(object sender, RoutedEventArgs e)
        {
            ScrollAElemento(SeccionSignosVitales);
        }

        private void BtnOdontologia_Click(object sender, RoutedEventArgs e)
        {
            ScrollAElemento(SeccionOdontologia);
        }

        private void BtnHistorial_Click(object sender, RoutedEventArgs e)
        {
            ScrollAElemento(SeccionHistorial);
        }
    }
}