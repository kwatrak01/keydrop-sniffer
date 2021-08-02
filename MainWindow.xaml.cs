using System;
using System.IO;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using KeyDrop_Sniffer.data;
using KeyDrop_Sniffer.views;
using KeyDrop_Sniffer.utils;
using System.Net;
using System.Net.Http;
using Dapper;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;

namespace KeyDrop_Sniffer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        protected Button CurrentMenuPosition
        {
            get {
                return currentMenuPosition;
            }
            set {
                if (currentMenuPosition != null)
                    currentMenuPosition.Background = Brushes.Transparent;

                currentMenuPosition = value;
                currentMenuPosition.Background = new SolidColorBrush(Color.FromRgb(17, 17, 20));
            }
        }
        private Button currentMenuPosition;
        private Config config;


        /*
         * 
         *    Instances of views
         * 
         *
        **/
        private HomeControl homeView;
        private HistoryControl historyView;


        public MainWindow()
        {
            Startup();
            InitializeComponent();
            Setup();
        }

        private async void Startup()
        {
            try
            {
                string appPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeydropSniffer");
                string configPath = System.IO.Path.Combine(appPath, "config.json");
                if (!Directory.Exists(appPath))
                    Directory.CreateDirectory(appPath);

                if (!File.Exists(configPath))
                {
                    var cfg = new Config { Cookie = "", Version = "BETA-1.0.0" };
                    using (StreamWriter writer = new StreamWriter(File.Create(configPath)))
                    {
                        writer.Write(System.Text.Json.JsonSerializer.Serialize(cfg));
                    }
                    config = cfg;
                }
                else
                {
                    using (StreamReader reader = new StreamReader(configPath))
                    {
                        var tmp = System.Text.Json.JsonSerializer.Deserialize<Config>(reader.ReadToEnd());
                        if (tmp == null)
                        {
                            MessageBox.Show("Błąd odczytu pliku konfiguracyjnego.", "I/O", MessageBoxButton.OK, MessageBoxImage.Error);
                            Application.Current.Shutdown();
                            return;
                        }
                        config = tmp;
                    }
                }
            }
            catch (Exception ignored)
            {
                MessageBox.Show("Błąd systemu I/O: " + ignored.Message, "I/O", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            bool sqlResult = await SQLiteHelper.Instance.CreateTables();
            if (!sqlResult)
            {
                MessageBox.Show("Nie można nawiązać połączenia z bazą danych!", "SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
        }

        public void InitializeObjects()
        {
            if (homeView == null)
                homeView = new HomeControl(this);
            if (historyView == null)
                historyView = new HistoryControl();
        }

        private void Setup()
        {
            if (config.Cookie.Length > 0 && config.DiscordToken.Length > 0)
            {
                InitializeObjects();
                ContentManager.Content = homeView;
                CurrentMenuPosition = HomeButton;
            } else
            {
                ContentManager.Content = new ConfigControl(this);
            }
            HomeButton.Click += MenuClickEvent;
            HistoryButton.Click += MenuClickEvent;
            SettingsButton.Click += MenuClickEvent;


            // TODO: Get cases price
            /*HtmlWeb web = new HtmlWeb();
            var doc = web.Load("https://key-drop.com/pl/Gold");
            
            HtmlNode[] nodes = doc.DocumentNode.SelectNodes("//div[@class='case__content']").ToArray();
            string output = "";
            foreach (var node in nodes)
            {
                var goldNodes = node.SelectNodes("./div");
                if (goldNodes.Count > 2)
                    continue;

                output += goldNodes[1].InnerText + " --> " + node.SelectNodes("./div")[0].InnerText + Environment.NewLine;
            }
            MessageBox.Show(output);*/
        }

        private void MenuClickEvent(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            ChangeView(btn);
        }

        public void ChangeView(Button btn)
        {
            if (CurrentMenuPosition != null && btn.Uid.Equals(CurrentMenuPosition.Uid))
                return;
            if (config.Cookie.Length < 5 || config.DiscordToken.Length < 5)
                return;

            CurrentMenuPosition = btn;

            switch (btn.Uid)
            {
                case "home":
                    ContentManager.Content = homeView;
                    break;
                case "history":
                    ContentManager.Content = historyView;
                    break;
                default:
                    break;
            }
        }

        public void CfgEdit(string property, string value)
        {
            switch (property)
            {
                case "cookie":
                    config.Cookie = value;
                    break;
                case "dc_token":
                    config.DiscordToken = value;
                    break;
                default:
                    break;
            }
        }

        public bool CfgSave()
        {
            string appPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeydropSniffer");
            string configPath = System.IO.Path.Combine(appPath, "config.json");
            try
            {
                using (StreamWriter writer = new StreamWriter(configPath))
                {
                    writer.Write(System.Text.Json.JsonSerializer.Serialize(config));
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string CfgGet(string property)
        {
            switch (property)
            {
                case "cookie":
                    return config.Cookie;
                case "dc_token":
                    return config.DiscordToken;
                default:
                    return "";
            }
        }

        public void ToggleUI()
        {
            HomeButton.IsEnabled = !HomeButton.IsEnabled;
            HistoryButton.IsEnabled = !HistoryButton.IsEnabled;
            SettingsButton.IsEnabled = !SettingsButton.IsEnabled;
            ShutdownButton.IsEnabled = !ShutdownButton.IsEnabled;
        }

        private void GlobalWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (homeView != null)
                homeView.Shutdown();
            if (historyView != null)
                historyView.Shutdown();
        }

        private void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (homeView != null)
                homeView.Shutdown();
            if (historyView != null)
                historyView.Shutdown();

            Application.Current.Shutdown();
        }
    }
}
