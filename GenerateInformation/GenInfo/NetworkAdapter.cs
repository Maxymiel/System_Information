using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInformation
{
    public class NetworkAdapter
    {
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public string Desription { get; set; }
        public string DnsSuffix { get; set; }
        public long Speed { get; set; }
        public string MAC { get; set; }

        public NetworkAdapter(string name, string iPAddress, string desription, string dnsSuffix, long speed, string mac)
        {
            Name = name;
            IPAddress = iPAddress;
            Desription = desription;
            DnsSuffix = dnsSuffix;
            Speed = speed;
            MAC = mac;
        }
    }
}
