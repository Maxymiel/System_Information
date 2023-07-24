using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInformation
{
    public class Drive
    {
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public object MediaType { get; set; }
        public ulong Size { get; set; }
        public string InstanceName { get; set; }
        public bool IsPrimary { get; set; }
        public string DeviceID { get; set; }
        public ushort BusType { get; set; }
        public string InterfaceType { get; set; }
        public string Path { get; set; }

        public Drive() { }
        public Drive(string model, string serialNumber, ushort mediaType, string instanceName, ulong size, string deviceId)
        {
            Model = model;
            SerialNumber = serialNumber;
            MediaType = mediaType;
            Size = size;
            InstanceName = instanceName;
            DeviceID = deviceId;
        }

        public Dictionary<int, Smart> SmartAttributes = new Dictionary<int, Smart>()
        {
                {0x01, new Smart("Raw Read Error Rate")},
                {0x02, new Smart("Throughput Performance")},
                {0x03, new Smart("Spin-Up Time")},
                {0x04, new Smart("Start/Stop Count")},
                {0x05, new Smart("Reallocated Sectors Count")},
                {0x06, new Smart("Read Channel Margin")},
                {0x07, new Smart("Seek Error Rate ")},
                {0x08, new Smart("Seek Time Performance ")},
                {0x09, new Smart("Power-on Time Count")},
                {0x0A, new Smart("Spin-Up Retry Count")},
                {0x0B, new Smart("Recalibration Retries")},
                {0x0C, new Smart("Device Power Cycle Count")},
                {0x0D, new Smart("Soft Read Error Rate")},

                {0xB4, new Smart("Unused Reserved Block Count Total")},
                {0xB7, new Smart("SATA Downshift Error Count")},
                {0xB9, new Smart("Head Stability")},
                {0xBD, new Smart("High Fly Writes")},
                {0xD2, new Smart("Vibration During Write")},
                {0xD3, new Smart("Vibration During Write")},
                {0xD4, new Smart("Shock During Write")},
                {0xE8, new Smart("SSD Endurance Remaining")},
                {0xE9, new Smart("Power-On Hours / Intel SSD Media Wearout Indicator")},

                {0xB8, new Smart("End-to-End error")},
                {0xBB, new Smart("Reported UNC Errors")},
                {0xBC, new Smart("Command Timeout")},
                {0xBE, new Smart("Airflow Temperature (WDC)")},
                {0xBF, new Smart("G-sense error rate")},
                {0xC0, new Smart("Power-off retract count")},
                {0xC1, new Smart("Load/Unload Cycle")},
                {0xC2, new Smart("HDA temperature")},
                {0xC3, new Smart("Hardware ECC Recovered")},
                {0xC4, new Smart("Reallocation Event Count")},
                {0xC5, new Smart("Current Pending Sector Count")},
                {0xC6, new Smart("Uncorrectable Sector Count")},
                {0xC7, new Smart("UltraDMA CRC Error Count")},
                {0xC8, new Smart("Write Error Rate / <br> Multi-Zone Error Rate")},
                {0xC9, new Smart("Soft read error rate")},
                {0xCA, new Smart("Data Address Mark errors")},
                {0xCB, new Smart("Run out cancel")},
                {0xCC, new Smart("Soft ECC correction")},
                {0xCD, new Smart("Thermal asperity rate (TAR)")},
                {0xCE, new Smart("Flying height")},
                {0xCF, new Smart("Spin high current")},
                {0xD0, new Smart("Spin buzz")},
                {0xD1, new Smart("Offline seek performance")},
                {0xDC, new Smart("Disk Shift")},
                {0xDD, new Smart("G-Sense Error Rate")},
                {0xDE, new Smart("Loaded Hours")},
                {0xDF, new Smart("Load/Unload Retry Count")},
                {0xE0, new Smart("Load Friction")},
                {0xE1, new Smart("Load Cycle Count")},
                {0xE2, new Smart("Load 'In'-time")},
                {0xE3, new Smart("Torque Amplification Count")},
                {0xE4, new Smart("Power-Off Retract Cycle")},
                {0xE6, new Smart("GMR Head Amplitude")},
                {0xE7, new Smart("Temperature")},
                {0xEA, new Smart("Average erase count AND Maximum Erase Count")},
                {0xF0, new Smart("Head flying hours")},
                {0xF1, new Smart("Total LBAs Written")},
                {0xF2, new Smart("Total LBAs Read")},
                {0xFA, new Smart("Read error retry rate")},
                {0xFE, new Smart("Free Fall Protection")},
            };
    }

    public class Smart
    {
        public bool HasData
        {
            get
            {
                if (Current == 0 && Worst == 0 && Threshold == 0 && Data == 0)
                    return false;
                return true;
            }
        }
        public string Attribute { get; set; }
        public int Current { get; set; }
        public int Worst { get; set; }
        public int Threshold { get; set; }
        public int Data { get; set; }
        public bool IsOK { get; set; }
        public int Status { get; set; }

        public Smart()
        {

        }

        public Smart(string attributeName)
        {
            this.Attribute = attributeName;
        }
    }
}
