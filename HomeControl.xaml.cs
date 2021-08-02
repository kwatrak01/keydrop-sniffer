using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Text.RegularExpressions;
using KeyDrop_Sniffer.utils;
using System.Threading;
using KeyDrop_Sniffer.data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.Toolkit.Uwp.Notifications;

namespace KeyDrop_Sniffer
{
    /// <summary>
    /// Logika interakcji dla klasy HomeControl.xaml
    /// </summary>
    public partial class HomeControl : UserControl
    {
        private MainWindow mw;
        private Timer timer;

        private int interval;
        private int limit;

        private int gold;
        private double pkt;

        public HomeControl(MainWindow mw)
        {
            this.mw = mw;
            InitializeComponent();
            Setup();
        }

        private bool running = false;
        
        private async void Setup()
        {
            dynamic obj = HttpHelper.Instance.GetBalance(mw.CfgGet("cookie"));
            if ((bool)obj.status)
            {
                gold = obj.gold;
                pkt = obj.pkt;
                UpdateUI();
            }
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                StopTask();
                ToggleUI();
                return;
            }

            limit = 1;
            try { limit = int.Parse(CodeCount.Text); } catch (Exception) { }
            interval = 5000;
            try { interval = int.Parse(CodeInterval.Text) * 1000; } catch (Exception) { }

            RunButton.IsEnabled = false;

            if (CodeSource.SelectedIndex == 0)
            {
                LogManager.Instance.Success(HomeLogBox, "APP", "Startowanie wątku...");
                LogManager.Instance.Clear(HomeLogBox.Name);
                times = 1;
                timer = new Timer(Tick, null, interval, Timeout.Infinite);

                ToggleUI();
                running = true;
                RunButton.Content = "Zatrzymaj";
                LogManager.Instance.Success(HomeLogBox, "APP", "Wątek wystartował!");
            } else
            {
                MessageBox.Show("Wybierz źródło pobrania kodów!", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            await Task.Delay(2000);
            RunButton.IsEnabled = true;
        }

        private int times;
        private Dictionary<string, int> codes;
        private async void Tick(object state)
        {
            await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Success(HomeLogBox, "Worker-" + times, "Pobieranie kodów...") ));
            var result = await HttpHelper.Instance.FetchCodes(mw.CfgGet("dc_token"), limit);
            if (result == null)
            {
                await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Fail(HomeLogBox, "Worker-" + times, "Nie można pobrać i załadować kodów!")));
                StopTask();
                return;
            }
            else
            {
                await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Success(HomeLogBox, "Worker-" + times, "Sprawdzanie kodów...")));

                if (codes == null || codes.Count < 1 || times >= 10)
                {
                    if (codes == null)
                        codes = new Dictionary<string, int>();

                    await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Success(HomeLogBox, "Worker-" + times, "Odświerzanie schowka...")));
                    var tmpCodes = await SQLiteHelper.Instance.GetCache();
                    if (tmpCodes == null)
                    {
                        await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Fail(HomeLogBox, "Worker-" + times, "Nie można pobrać schwoka!")));
                        StopTask();
                        return;
                    }
                    else
                    {
                        foreach (var tmp in tmpCodes)
                        {
                            if (!codes.ContainsKey(tmp.code))
                                codes.Add(tmp.code, tmp.success);
                        }
                    }
                }

                await RunButton.Dispatcher.BeginInvoke((Action)(() => RunButton.IsEnabled = false));
                List<SQLCode> toInsert = new List<SQLCode>();
                foreach (var item in result)
                {
                    if (codes.ContainsKey(item.content))
                        continue;

                    string output = await HttpHelper.Instance.UseCode(item.content, mw.CfgGet("cookie"));
                    dynamic dynOutput = JsonConvert.DeserializeObject<dynamic>(output);

                    if (dynOutput.promoCode != null)
                    {
                        if (dynOutput.status == false)
                        {
                            await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Success(HomeLogBox, "Worker-" + times, "Kod " + item.content + " -> " + dynOutput.info)));
                        }
                        else
                        {
                            dynamic obj = HttpHelper.Instance.GetBalance(mw.CfgGet("cookie"));
                            if ((bool)obj.status)
                            {
                                gold = obj.gold;
                                pkt = obj.pkt;
                                UpdateUIThread();
                            }

                            new ToastContentBuilder()
                            .AddText("Znaleziono kod")
                            .AddText("Zrealizowano nowy kod: " + item.content)
                            .Show();

                            await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Success(HomeLogBox, "Worker-" + times, "Kod " + item.content + " -> Złoto: " + ((dynOutput.goldBonus == null) ? "Brak" : "+" + dynOutput.goldBonus) + " | Depozyt: " + ((dynOutput.depositBonus == null) ? "Brak" : "+" + dynOutput.depositBonus))));
                        }
                    }
                    else if (dynOutput.promoCode == null && dynOutput.info != null)
                    {
                        await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Fail(HomeLogBox, "Worker-" + times, dynOutput.info)));
                    }
                    int isSuccessed = dynOutput.status == false ? 0 : 1;
                    codes.Add(item.content, isSuccessed);
                    toInsert.Add(new SQLCode() { code = item.content, success = isSuccessed });
                    await Task.Delay(10 * 1000);
                }
                await RunButton.Dispatcher.BeginInvoke((Action)(() => RunButton.IsEnabled = true));
                if (toInsert.Count > 0)
                {
                    if (await SQLiteHelper.Instance.AddCodes(toInsert))
                        await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Success(HomeLogBox, "Worker-" + times, "Zapisano nowe kody!")));
                    else
                        await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Warning(HomeLogBox, "Worker-" + times, "Kody nie zostały zapisane! Przerwij sprawdzanie.")));
                }
                else
                {
                    await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Warning(HomeLogBox, "Worker-" + times, "Nie ma nowych kodów")));
                }

                await HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Warning(HomeLogBox, "Worker-" + times, "------------------------------------")));

                if (running)
                    timer?.Change(interval, Timeout.Infinite);
            }

            if (times >= 10) times = 1;
            else times++;
        }

        private void StopTask()
        {
            running = false;
            timer.Dispose();
            RunButton.Dispatcher.BeginInvoke((Action)(() => RunButton.Content = "Uruchom machinę" ));
            HomeLogBox.Dispatcher.BeginInvoke((Action)(() => LogManager.Instance.Success(HomeLogBox, "APP", "Zatrzymano wątek!")));
        }

        private void ToggleUI()
        {
            CodeSource.IsEnabled = !CodeSource.IsEnabled;
            CodeCount.IsEnabled = !CodeCount.IsEnabled;
            CodeInterval.IsEnabled = !CodeInterval.IsEnabled;

            mw.ToggleUI();
        }

        private void UpdateUI()
        {
            AwardedGold.Content = gold.ToString();
            AwardedDeposit.Content = pkt.ToString("C2");
        }

        private void UpdateUIThread()
        {
            AwardedGold.Dispatcher.BeginInvoke((Action)(() => AwardedGold.Content = gold.ToString()));
            AwardedDeposit.Dispatcher.BeginInvoke((Action)(() => AwardedDeposit.Content = pkt.ToString("C2")));
        }

        public async void Shutdown()
        {
            
        }
    }

    internal class Code
    {
        public string content { get; set; }
    }
}
