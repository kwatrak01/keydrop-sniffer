using KeyDrop_Sniffer.utils;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Data;
using System.Data.SQLite;
using Dapper;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto;

namespace KeyDrop_Sniffer.views
{
    /// <summary>
    /// Logika interakcji dla klasy ConfigControl.xaml
    /// </summary>
    public partial class ConfigControl : UserControl
    {
        private MainWindow mw;

        public ConfigControl(MainWindow mw)
        {
            this.mw = mw;
            InitializeComponent();
            Setup();
        }

        private void Setup()
        {
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default");
            string cookie = "";
            if (Directory.Exists(path))
            {
                var cookieFile = System.IO.Path.Combine(path, "Cookies");
                if (File.Exists(cookieFile))
                {
                    var stateFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Local State");
                    if (File.Exists(stateFile))
                    {
                        string json = null;
                        using (StreamReader reader = new StreamReader(stateFile))
                        {
                            json = reader.ReadToEnd();
                        }

                        if (json != null)
                        {
                            using (IDbConnection conn = new SQLiteConnection("Data Source=" + cookieFile + ";Version=3;"))
                            {
                                var result = conn.Query("SELECT * FROM cookies WHERE host_key LIKE '%key-drop.com'");

                                foreach (dynamic d in result.ToList())
                                {
                                    var enc = JObject.Parse(json)["os_crypt"]["encrypted_key"].ToString();
                                    var decodedKey = System.Security.Cryptography.ProtectedData.Unprotect(Convert.FromBase64String(enc).Skip(5).ToArray(), null, System.Security.Cryptography.DataProtectionScope.LocalMachine);
                                    var val = decryptWithKey(d.encrypted_value, decodedKey, 3);
                                    cookie += d.name + "=" + val + ";";
                                }
                            }

                        }
                    }
                }
            }

            if (cookie.Length > 0)
            {
                CookieInput.Text = cookie;
            }
        }

        private string decryptWithKey(byte[] message, byte[] key, int nonSecretPayloadLength)
        {
            const int KEY_BIT_SIZE = 256;
            const int MAC_BIT_SIZE = 128;
            const int NONCE_BIT_SIZE = 96;

            if (key == null || key.Length != KEY_BIT_SIZE / 8)
                throw new ArgumentException(String.Format("Key needs to be {0} bit!", KEY_BIT_SIZE), "key");
            if (message == null || message.Length == 0)
                throw new ArgumentException("Message required!", "message");

            using (var cipherStream = new MemoryStream(message))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                var nonSecretPayload = cipherReader.ReadBytes(nonSecretPayloadLength);
                var nonce = cipherReader.ReadBytes(NONCE_BIT_SIZE / 8);
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), MAC_BIT_SIZE, nonce);
                cipher.Init(false, parameters);
                var cipherText = cipherReader.ReadBytes(message.Length);
                var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];
                try
                {
                    var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                    cipher.DoFinal(plainText, len);
                }
                catch (InvalidCipherTextException)
                {
                    return null;
                }
                return Encoding.Default.GetString(plainText);
            }
        }

        private async void SaveCfgButton_Click(object sender, RoutedEventArgs e)
        {
            if (CookieInput.Text.Length < 5 || DCTokenInput.Text.Length < 5)
                return;

            mw.CfgEdit("cookie", CookieInput.Text);
            mw.CfgEdit("dc_token", DCTokenInput.Text);
            if (mw.CfgSave())
            {
                mw.InitializeObjects();
                mw.ChangeView(mw.HomeButton);
            }
            else
            {
                MessageBox.Show("Nie można zapisać konfiguracji!");
            }
        }
    }
}
