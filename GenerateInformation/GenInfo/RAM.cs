﻿using System.Collections.Generic;
using System.Linq;

namespace GenerateInformation.GenInfo
{
    public class RAMArray
    {
        public List<RAM> RAM = new List<RAM>();
        public long MaxCapacity { get; set; }
        public int MemoryDevices { get; set; }

        public ulong RAMCapacity
        {
            get { return (ulong)RAM.Sum(v => (long)v.Capacity); }
        }
    }

    public class RAM
    {
        public RAM() { }
        public string BankLabel { get; set; }
        public ushort MemoryType { get; set; }
        public uint SMBIOSMemoryType { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
        public ulong Capacity { get; set; }
        public uint Speed { get; set; }

        public string DDRType
        {
            get
            {
                if (SMBIOSMemoryType != 0)
                    return FormatDDR(SMBIOSMemoryType);
                if (MemoryType != 0)
                    return FormatDDR(MemoryType);
                return "";
            }
            set { }
        }

        private string FormatDDR(uint RAMSMBIOSMemoryType)
        {
            switch (RAMSMBIOSMemoryType)
            {
                case 1:
                    return "Other";
                case 2:
                    return "DRAM";
                case 3:
                    return "Synchronous DRAM";
                case 4:
                    return "Cache DRAM";
                case 5:
                    return "EDO";
                case 6:
                    return "EDRAM";
                case 7:
                    return "VRAM";
                case 8:
                    return "SRAM";
                case 9:
                    return "RAM";
                case 10:
                    return "ROM";
                case 11:
                    return "Flash";
                case 12:
                    return "EEPROM";
                case 13:
                    return "FEPROM";
                case 14:
                    return "EPROM";
                case 15:
                    return "CDRAM";
                case 16:
                    return "3DRAM";
                case 17:
                    return "SDRAM";
                case 18:
                    return "SGRAM";
                case 19:
                    return "RDRAM";
                case 20:
                    return "DDR";
                case 21:
                    return "DDR2";
                case 22:
                    return "DDR2 FB-DIMM";
                case 23:
                    return "";
                case 24:
                    return "DDR3";
                case 25:
                    return "FBD2";
                case 26:
                    return "DDR4";
                case 34:
                    return "DDR5";
                default:
                    return "Unknown";
            }
        }
    }
}