namespace GenerateInformation.GenInfo
{
    public class Motherboard
    {
        public string Manufacturer { get; set; }
        public string Product { get; set; }
        public string SerialNumber { get; set; }

        public Motherboard() { }

        public Motherboard(string motherBoardManufacturer, string motherBoardProduct, string motherBoardSerial)
        {
            Manufacturer = motherBoardManufacturer;
            Product = motherBoardProduct;
            SerialNumber = motherBoardSerial;
        }
    }
}
