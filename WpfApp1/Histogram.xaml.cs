using System;
using System.ComponentModel;
using System.Windows;
using OxyPlot;
using OxyPlot.Series;

namespace WpfApp1
{
    public partial class Histogram : Window
    {
        private MainWindow currentWindow { set; get;  }
        public Histogram(MainWindow window)
        {
            InitializeComponent();
            currentWindow = window;
            this.Closing += Histogram_Closing;
        }

        private void ButtonClickBack(object sender, RoutedEventArgs e)
        {
            currentWindow.Show();
            Close();
        }

        private void Histogram_Closing(object sender, CancelEventArgs e)
        {
            currentWindow.Show();
        }
    }
}
