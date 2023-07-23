using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConverterHTML
{
    public partial class Helpers
    {
        public static string GetSocket(ushort UpgradeMethod)
        {
            switch (UpgradeMethod)
            {
                case 2:
                    return "Unknown";
                case 3:
                    return "Daughter Board";
                case 4:
                    return "ZIF Socket";
                case 5:
                    return "Replaceable Piggy Back";
                case 6:
                    return "None";
                case 7:
                    return "LIF Socket";
                case 8:
                    return "Slot 1";
                case 9:
                    return "Slot 2";
                case 10:
                    return "370-pin socket";
                case 11:
                    return "Slot A";
                case 12:
                    return "Slot M";
                case 13:
                    return "423";
                case 14:
                    return "A (Socket 462)";
                case 15:
                    return "478";
                case 16:
                    return "754";
                case 17:
                    return "940";
                case 18:
                    return "939";
                case 19:
                    return "mPGA604";
                case 20:
                    return "LGA771";
                case 21:
                    return "LGA775";
                case 22:
                    return "S1";
                case 23:
                    return "AM2";
                case 24:
                    return "F (1207)";
                case 25:
                    return "LGA1366";
                case 26:
                    return "G34";
                case 27:
                    return "AM3";
                case 28:
                    return "C32";
                case 29:
                    return "LGA1156";
                case 30:
                    return "LGA1567";
                case 31:
                    return "PGA988A";
                case 32:
                    return "BGA1288";
                case 33:
                    return "rPGA988B";
                case 34:
                    return "BGA1023";
                case 35:
                    return "BGA1224";
                case 36:
                    return "LGA1155";
                case 37:
                    return "LGA1356";
                case 38:
                    return "LGA2011";
                case 39:
                    return "FS1";
                case 40:
                    return "FS2";
                case 41:
                    return "FM1";
                case 42:
                    return "FM2";
                case 43:
                    return "LGA2011-3";
                case 44:
                    return "LGA1356-3";
                case 45:
                    return "LGA1150";
                case 46:
                    return "BGA1168";
                case 47:
                    return "BGA1234";
                case 48:
                    return "BGA1364";
                case 49:
                    return "AM4";
                case 50:
                    return "LGA1151";
                case 51:
                    return "BGA1356";
                case 52:
                    return "BGA1440";
                case 53:
                    return "BGA1515";
                case 54:
                    return "LGA3647-1";
                case 55:
                    return "SP3";
                case 56:
                    return "SP3r2";
                default:
                    return "Other";
            }
        }

        /// <summary>
        /// Get disk type by MediaType (https://learn.microsoft.com/en-us/windows-hardware/drivers/storage/msft-physicaldisk#members)
        /// </summary>
        public static string GetDiskType(object MediaType)
        {
            ushort mediaType;
            ushort.TryParse(MediaType.ToString(), out mediaType);

            switch (mediaType)
            {
                case 3:
                    return "HDD";
                case 4:
                    return "SSD";
                case 5:
                    return "SCM";
                default:
                    return "Unspecified";
            }
        }

        /// <summary>
        /// Get disk interface by BusType (https://learn.microsoft.com/en-us/windows-hardware/drivers/storage/msft-physicaldisk#members)
        /// </summary>
        public static string GetDiskInterface(ushort BusType)
        {
            switch (BusType)
            {
                case 1:
                    return "SCSI";
                case 2:
                    return "ATAPI";
                case 3:
                    return "ATA";
                case 4:
                    return "1394";
                case 5:
                    return "SSA";
                case 6:
                    return "Fibre Channel";
                case 7:
                    return "USB";
                case 8:
                    return "RAID";
                case 9:
                    return "iSCSI";
                case 10:
                    return "SAS";
                case 11:
                    return "SATA";
                case 12:
                    return "SD";
                case 13:
                    return "MMC";
                case 14:
                    return "MAX";
                case 15:
                    return "File Backed Virtual";
                case 16:
                    return "Storage Spaces";
                case 17:
                    return "NVMe";
                case 18:
                    return "Microsoft Reserved (maybe Virtual)";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Get monitor manufacturer by manufacturer code from registry
        /// </summary>
        public static string GetMonitorManufacturer(string ManufacturerName)
        {
            switch (ManufacturerName)
            {
                case "AAC":
                    return "AcerView";
                case "ACR":
                    return "Acer";
                case "AOC":
                    return "AOC";
                case "AIC":
                    return "AG Neovo";
                case "APP":
                    return "Apple Computer";
                case "AST":
                    return "AST Research";
                case "AUO":
                    return "Asus";
                case "BNQ":
                    return "BenQ";
                case "CMO":
                    return "Acer";
                case "CPL":
                    return "Compal";
                case "CPQ":
                    return "Compaq";
                case "CPT":
                    return "Chunghwa Pciture Tubes, Ltd.";
                case "CTX":
                    return "CTX";
                case "DEC":
                    return "DEC";
                case "DEL":
                    return "DELL";
                case "DPC":
                    return "Delta";
                case "DWE":
                    return "Daewoo";
                case "EIZ":
                    return "EIZO";
                case "ELS":
                    return "ELSA";
                case "ENC":
                    return "EIZO";
                case "EPI":
                    return "Envision";
                case "FCM":
                    return "Funai";
                case "FUJ":
                    return "Fujitsu";
                case "FUS":
                    return "Fujitsu-Siemens";
                case "GSM":
                    return "LG Electronics";
                case "GWY":
                    return "Gateway 2000";
                case "HEI":
                    return "Hyundai";
                case "HIT":
                    return "Hyundai";
                case "HSL":
                    return "Hansol";
                case "HTC":
                    return "Hitachi/Nissei";
                case "HWP":
                    return "HP";
                case "IBM":
                    return "IBM";
                case "ICL":
                    return "Fujitsu ICL";
                case "IVM":
                    return "Iiyama";
                case "KDS":
                    return "Korea Data Systems";
                case "LEN":
                    return "Lenovo";
                case "LGD":
                    return "Asus";
                case "LPL":
                    return "Fujitsu";
                case "MAX":
                    return "Belinea";
                case "MEI":
                    return "Panasonic";
                case "MEL":
                    return "Mitsubishi Electronics";
                case "MS_":
                    return "Panasonic";
                case "NAN":
                    return "Nanao";
                case "NEC":
                    return "NEC";
                case "NOK":
                    return "Nokia Data";
                case "NVD":
                    return "Fujitsu";
                case "OPT":
                    return "Optoma";
                case "PHL":
                    return "Philips";
                case "REL":
                    return "Relisys";
                case "SAN":
                    return "Samsung";
                case "SAM":
                    return "Samsung";
                case "SBI":
                    return "Smarttech";
                case "SGI":
                    return "SGI";
                case "SNY":
                    return "Sony";
                case "SRC":
                    return "Shamrock";
                case "SUN":
                    return "Sun Microsystems";
                case "SEC":
                    return "Hewlett-Packard";
                case "TAT":
                    return "Tatung";
                case "TOS":
                    return "Toshiba";
                case "TSB":
                    return "Toshiba";
                case "VSC":
                    return "ViewSonic";
                case "ZCM":
                    return "Zenith";
                case "UNK":
                    return "Unknown";
                case "_YV":
                    return "Fujitsu";
                default:
                    return ManufacturerName;
            }
        }
    }
}