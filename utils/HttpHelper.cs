using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KeyDrop_Sniffer.utils
{
    class HttpHelper
    {
        public static HttpHelper Instance
        {
            get
            {
                if (core == null)
                    core = new HttpHelper();
                return core;
            }
        }
        private static HttpHelper core;

        public async Task<List<Code>> FetchCodes(string token, int limit)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://discord.com/api/v9/channels/773158934432972810/messages?limit=" + limit);
            request.Method = "GET";
            request.Headers.Add("authorization", token);

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    List<Code> codes = (await DeserializeCodes(result)).ToList();
                    for (int i = 0; i < codes.Count; i++)
                    {
                        if (codes[i].content.Length > 0)
                        {
                            codes[i].content = await RemoveSpecialCharacters(codes[i].content);
                        }
                        else
                        {
                            dynamic json = JsonConvert.DeserializeObject<dynamic>(result);
                            codes[i].content = (json[i].embeds[0] != null) ? json[i].embeds[0].author.name : "invalid_parse";
                        }
                    }
                    return codes;
                }
            }
            catch (Exception)
            {
                /*System.Windows.MessageBox.Show(exc.Message);*/
                return null;
            }
        }

        public async Task<string> UseCode(string content, string cookie)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://key-drop.com/pl/Api/activation_code");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("cookie", cookie);
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            try
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string json = "{\"promoCode\":\"" + content + "\", \"recaptcha\":\"null\"}";
                    streamWriter.Write(json);
                }

                var response = (HttpWebResponse)request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    return result;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public dynamic GetBalance(string cookie)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://key-drop.com/pl/balance");
            request.Method = "GET";
            request.Headers.Add("cookie", cookie);

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(sr.ReadToEnd());
                    return result;
                }
            }
            catch (Exception)
            {
                return new { status=true, pkt=0.00, vdolce=0, gold=0 };
            }
        }

        private async static Task<IList<Code>> DeserializeCodes(string json)
        {
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };

            return System.Text.Json.JsonSerializer.Deserialize<IList<Code>>(json, options);
        }

        public async Task<string> RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }
    }
}
