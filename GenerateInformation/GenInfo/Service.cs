namespace GenerateInformation.GenInfo
{
    public class Service
    {
        public string DisplayName { get; set; }
        public string ServiceName { get; set; }
        public string Status { get; set; }

        public Service(string displayName, string serviceName, string status)
        {
            DisplayName = displayName;
            ServiceName = serviceName;
            Status = status;
        }
    }
}
