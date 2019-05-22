using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace TWPoster
{
    public partial class MainWindow : Window
    {
        private delegate void SafeCallDelegate(string e);

        System.Timers.Timer timer = new System.Timers.Timer();

        public MainWindow()
        {
            InitializeComponent();
            MainProcess.Instance.OnDeckObtained += new MainProcess.DeckObtainedEventRaiser(ArchivosAnalizados_Evento);
        }

        /// <summary>
        /// Método encargado de manejar el evento "Archivos analizados"
        /// </summary>
        private void ArchivosAnalizados_Evento(List<string> decks)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in decks)
            {
                stringBuilder.AppendLine(item);
            }
            var d = new SafeCallDelegate(RefrescarOperacion);
            d.Invoke(stringBuilder.ToString());
        }

        /// <summary>
        /// Método encargado de refrescar el tamaño de los archivos a descargar.
        /// </summary>
        private void RefrescarOperacion(string operacion)
        {
            try
            {
                Dispatcher.BeginInvoke(
                new ThreadStart(() => textBlock.Text = operacion));
            }
            catch (Exception)
            {
                MessageBox.Show("Error a la hora de actualizar la interfaz.");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Comenzar_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
            MainProcess.Instance.Start();
            btnComenzar.IsEnabled = true;
        }
    }
}
