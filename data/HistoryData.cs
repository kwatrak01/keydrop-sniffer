using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyDrop_Sniffer.data
{
    public class HistoryData
    {
        public string code { get; set; }
        public string success
        {
            get
            {
                return localStatus;
            }
            set
            {
                if (value == "0")
                    localStatus = "Nie działa";
                else
                    localStatus = "Użyty";
            }
        }
        private string localStatus;
    }
}
