using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInformation.DeviceCollectors.Windows.VersionCollector
{
    public class WindowsVersionCollector
    {
        public static OperationSystemType GetOperatingSystem(OSVERSIONINFOEX rtl)
        {
            var IsServer = rtl.ProductType == ProductType.DomainController || rtl.ProductType == ProductType.Server;
            var IsWorkstation = rtl.ProductType == ProductType.Workstation;

            if (rtl.MajorVersion == 10 && rtl.MinorVersion == 0 && rtl.BuildNumber >= 22000 && IsWorkstation)
                return OperationSystemType.Windows11;
            else if (rtl.MajorVersion == 10 && rtl.MinorVersion == 0 && rtl.BuildNumber >= 20348 && IsServer)
                return OperationSystemType.WindowsServer2022;
            else if (rtl.MajorVersion == 10 && rtl.MinorVersion == 0 && IsWorkstation)
                return OperationSystemType.Windows10;
            else if (rtl.MajorVersion == 10 && rtl.MinorVersion == 0 && rtl.BuildNumber >= 17763 && IsServer)
                return OperationSystemType.WindowsServer2019;
            else if (rtl.MajorVersion == 10 && rtl.MinorVersion == 0 && IsServer)
                return OperationSystemType.WindowsServer2016;
            else if (rtl.MajorVersion == 6 && rtl.MinorVersion == 3 && IsServer)
                return OperationSystemType.WindowsServer2012R2;
            else if (rtl.MajorVersion == 6 && rtl.MinorVersion == 3 && IsWorkstation)
                return OperationSystemType.Windows81;
            else if (rtl.MajorVersion == 6 && rtl.MinorVersion == 2 && IsWorkstation)
                return OperationSystemType.Windows8;
            else if (rtl.MajorVersion == 6 && rtl.MinorVersion == 2 && IsServer)
                return OperationSystemType.WindowsServer2012;
            else if (rtl.MajorVersion == 6 && rtl.MinorVersion == 1 && IsWorkstation)
                return OperationSystemType.Windows7;
            else if (rtl.MajorVersion == 6 && rtl.MinorVersion == 1 && IsServer)
                return OperationSystemType.WindowsServer2008R2;
            else if (rtl.MajorVersion == 6 && rtl.MinorVersion == 0 && IsServer)
                return OperationSystemType.WindowsServer2008;
            else if (rtl.MajorVersion == 6 && rtl.MinorVersion == 0 && IsWorkstation)
                return OperationSystemType.WindowsVista;
            else if (rtl.MajorVersion == 5 &&
                     rtl.MinorVersion == 2 &&
                     IsServer &&
                     WindowsNativeMethods.GetSystemMetrics(SystemMetric.SM_SERVERR2) != 0)
                return OperationSystemType.WindowsServer2003R2;
            else if (rtl.MajorVersion == 5 &&
                     rtl.MinorVersion == 2 &&
                     IsServer &&
                     WindowsNativeMethods.GetSystemMetrics(SystemMetric.SM_SERVERR2) == 0 &&
                     (rtl.SuiteMask & SuiteMask.VER_SUITE_WH_SERVER) != SuiteMask.VER_SUITE_WH_SERVER)
                return OperationSystemType.WindowsServer2003;
            else if (rtl.MajorVersion == 5 && rtl.MinorVersion == 2 && (rtl.SuiteMask & SuiteMask.VER_SUITE_WH_SERVER) == SuiteMask.VER_SUITE_WH_SERVER)
                return OperationSystemType.WindowsHomeServer;
            else if (rtl.MajorVersion == 5 && rtl.MinorVersion == 2 && IsWorkstation && Environment.Is64BitOperatingSystem)
                return OperationSystemType.WindowsXPProx64;
            else if (rtl.MajorVersion == 5 && rtl.MinorVersion == 1)
                return OperationSystemType.WindowsXP;
            else if (rtl.MajorVersion == 5 && rtl.MinorVersion == 0)
                return OperationSystemType.Windows2000;

            return OperationSystemType.Unknown;
        }

        public static string GetOperationSystemTypeString()
        {
            string edition = null;

            try
            {
                var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

                edition = (string)reg.GetValue("EditionID");
            }
            catch { }

            string productName;

            try
            {
                var versionInfo = new OSVERSIONINFOEX();
                versionInfo.OSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));

                var result = WindowsNativeMethods.RtlGetVersion(ref versionInfo);

                if (result != NTSTATUS.STATUS_SUCCESS)
                {
                    throw new Exception("Failed to get OS version information. NTSTATUS: " + result);
                }

                var osType = GetOperatingSystem(versionInfo);

                productName = ConvertVersionToString(osType);
            }
            catch
            {
                return "Unknown";
            }

            if (!string.IsNullOrEmpty(edition))
            {
                productName = $"{productName} {edition}";
            }

            return productName;
        }

        private static string ConvertVersionToString(OperationSystemType osType)
        {
            switch (osType)
            {
                case OperationSystemType.Windows11:
                    return "Windows 11";
                case OperationSystemType.Windows10:
                    return "Windows 10";
                case OperationSystemType.Windows8:
                    return "Windows 8";
                case OperationSystemType.Windows81:
                    return "Windows 8.1";
                case OperationSystemType.Windows7:
                    return "Windows 7";
                case OperationSystemType.WindowsVista:
                    return "Windows Vista";
                case OperationSystemType.Windows2000:
                    return "Windows 2000";
                case OperationSystemType.WindowsXP:
                    return "Windows XP";
                case OperationSystemType.WindowsXPProx64:
                    return "Windows XP Professional x64 Edition";
                case OperationSystemType.WindowsHomeServer:
                    return "Windows Home Server";
                case OperationSystemType.WindowsServer2003:
                    return "Windows Server 2003";
                case OperationSystemType.WindowsServer2003R2:
                    return "Windows Server 2003 R2";
                case OperationSystemType.WindowsServer2008:
                    return "Windows Server 2008";
                case OperationSystemType.WindowsServer2008R2:
                    return "Windows Server 2008 R2";
                case OperationSystemType.WindowsServer2012:
                    return "Windows Server 2012";
                case OperationSystemType.WindowsServer2012R2:
                    return "Windows Server 2012 R2";
                case OperationSystemType.WindowsServer2016:
                    return "Windows Server 2016";
                case OperationSystemType.WindowsServer2019:
                    return "Windows Server 2019";
                case OperationSystemType.WindowsServer2022:
                    return "Windows Server 2022";
                default:
                    return "Unknown";
            }
        }
    }
}
