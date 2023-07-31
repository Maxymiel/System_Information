namespace GenerateInformation.GenInfo
{
    public class Monitor
    {
        public Monitor() { }
        public bool Active { get; set; }
        public string ManufacturerName { get; set; }
        public string ProductCodeID { get; set; }
        public string SerialNumberID { get; set; }
        public string UserFriendlyName { get; set; }
        public int YearOfManufacture { get; set; }
    }
}