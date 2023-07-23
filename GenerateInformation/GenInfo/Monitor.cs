using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateInformation
{
    public class Monitor
    {
        public bool Active { get; set; }
        public string ManufacturerName { get; set; }
        public string ProductCodeID { get; set;}
        public string SerialNumberID { get; set; }
        public string UserFriendlyName { get; set; }
        public int YearOfManufacture { get; set; }

        public Monitor() { }
        public Monitor(bool active, string manufacturerName, string productCodeID, string serialNumberID, string userFriendlyName, int yearOfManufacture)
        {
            Active = active;
            ManufacturerName = manufacturerName;
            ProductCodeID = productCodeID;
            SerialNumberID = serialNumberID;
            UserFriendlyName = userFriendlyName;
            YearOfManufacture = yearOfManufacture;
        }
    }
}
