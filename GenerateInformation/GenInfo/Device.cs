namespace GenerateInformation.GenInfo
{
    public class Device
    {
        public string Caption { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public Device() { }
        public Device(string caption, string description, string status)
        {
            Caption = caption;
            Description = description;
            Status = status;
        }
    }
}
