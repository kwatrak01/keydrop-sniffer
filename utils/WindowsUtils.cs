using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KeyDrop_Sniffer.utils
{
    internal class WindowsUtils
    {
        public WindowsUtils Instance
        {
            get
            {
                if (core == null)
                    core = new WindowsUtils();
                return core;
            }
        }

        private WindowsUtils core;

        public void SendToast(string title, string content)
        {
            
        }
    }
}
