using KeyDrop_Sniffer.data;
using KeyDrop_Sniffer.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KeyDrop_Sniffer
{
    /// <summary>
    /// Logika interakcji dla klasy HistoryControl.xaml
    /// </summary>
    public partial class HistoryControl : UserControl
    {
        private List<HistoryData> histories;
        public HistoryControl()
        {
            Setup();
            InitializeComponent();
            Initialize();
        }

        private async void Setup()
        {
            if (histories == null)
                histories = new List<HistoryData>();

            histories = await SQLiteHelper.Instance.GetHistory();
        }

        private void Initialize()
        {
            CodesDG.ItemsSource = histories;
        }

        public void Shutdown()
        {

        }

        private async void GetDataButton_Click(object sender, RoutedEventArgs e)
        {
            histories = await SQLiteHelper.Instance.GetHistory();
            CodesDG.ItemsSource = histories;

            MessageBox.Show("Dane w tabeli zostały uzupełnione ponownie!", "Sukces!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
