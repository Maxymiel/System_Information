namespace GenerateInformation.GenInfo
{
    public class NetworkAdapter
    {
        public NetworkAdapter(string name, string iPAddress, string desription, string dnsSuffix, long speed, string mac)
        {
            Name = name;
            IPAddress = iPAddress;
            Desription = desription;
            DnsSuffix = dnsSuffix;
            Speed = speed;
            MAC = mac;
        }

        public string Name { get; set; }
        public string IPAddress { get; set; }
        public string Desription { get; set; }
        public string DnsSuffix { get; set; }
        public long Speed { get; set; }
        public string MAC { get; set; }
    }
}