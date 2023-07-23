using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GenerateInformation
{
    public class GenerateInformation
    {
        public static General Generation()
        {
            General information = new General();

            //GENERAL INFORMATION

            information.GenDate = DateTime.Now;
            information.MachineName = Environment.MachineName;

            information.IsVirtualMachine = IsVirtualMachine();
            information.UserName = getuser();
            information.LoginAsAdministrator = loginasadmin();
            information.WinVersion = GetWinVer();

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem"), information);

            //NETWORK INFORMATION

            try
            {
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in adapters)
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    foreach (UnicastIPAddressInformation ip in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            string MAC = "";
                            try { MAC = adapter.GetPhysicalAddress().ToString(); } catch { }
                            information.NetworkAdapters.Add(new NetworkAdapter(adapter.Name, ip.Address.ToString(), adapter.Description,
                                properties.DnsSuffix, adapter.Speed / 1000 / 1000, MAC));
                        }
                    }
                }
            }
            catch { }

            //MOTHERBOARD INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard"), information.Motherboard);

            //VIDEO INFORMATION

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"))
            {
                string[] subkeys = rk.GetSubKeyNames();
                subkeys = subkeys.Where(t => Regex.IsMatch(t, @"\d*")).ToArray();

                for (int i = 0; i < subkeys.Length; i++)
                {
                    using (RegistryKey rk2 = rk.OpenSubKey(subkeys[i]))
                    {
                        if (rk2.GetValue("DriverDesc") != null)
                        {
                            VideoAdapter videoAdapter = new VideoAdapter();

                            videoAdapter.Name = rk2.GetValue("DriverDesc").ToString();

                            if (rk2.GetValue("ProviderName") != null)
                            {
                                videoAdapter.AdapterCompatibility = CheckValue(rk2.GetValue("ProviderName"));
                            }
                            else { videoAdapter.AdapterCompatibility = "No provider name"; }

                            if (rk2.GetValue("HardwareInformation.qwMemorySize") != null)
                            {
                                videoAdapter.AdapterRAM = long.Parse(CheckValue(rk2.GetValue("HardwareInformation.qwMemorySize")));
                                videoAdapter.AdapterRAM = videoAdapter.AdapterRAM / 1024 / 1024;
                            }
                            else { videoAdapter.AdapterRAM = 0; }

                            information.VideoAdapter = videoAdapter;

                            break;
                        }
                    }
                }
            }

            if (information.VideoAdapter.AdapterRAM == 0)
            {
                InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController"), information.VideoAdapter);
                information.VideoAdapter.AdapterRAM = information.VideoAdapter.AdapterRAM / 1024 / 1024;
            }

            //CPU INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"), information.CPUs);

            //RAM INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory"), information.RAMArray.RAM);

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM win32_PhysicalMemoryArray"), information.RAMArray);

            //MONITOR INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\wmi", "SELECT * FROM WmiMonitorID"), information.Monitors);

            //DRIVE INFORMATION

            InitializeInformation(new ManagementObjectSearcher(@"root\Microsoft\Windows\Storage", "SELECT * FROM MSFT_PhysicalDisk"), information.Drives);

            if (information.Drives.Count == 0) { InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "Select * from Win32_DiskDrive"), information.Drives); }

            try
            {
                //Primary disk detect

                try
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2",
                                "SELECT * FROM Win32_DiskPartition WHERE BootPartition=True");

                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        information.Drives.Find(t => t.DeviceId == queryObj["DiskIndex"].ToString()).IsPrimary = true;
                    }
                }
                catch { }

                //Get instances

                try
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2",
                                "Select * from Win32_DiskDrive");

                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        foreach (var property in queryObj.Properties)
                        {
                            if (property.Name == "SerialNumber" && property.Value != null)
                            {
                                string sn = property.Value.ToString().ToLower();

                                Drive drive = information.Drives.Find(t => t.SerialNumber != null && sn.Contains(t.SerialNumber.ToLower()));
                                if (drive != null) { drive.InstanceName = queryObj["PNPDeviceID"].ToString(); }
                            }
                        }
                    }
                }
                catch { }
            }
            catch { }

            //MARKS INFORMATION

            information.Marks = new Marks();

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_WinSAT"), information.Marks);

            information.Marks.TotalmemMark = information.RAMArray.RAMCapacity;
            information.Marks.TotalDrivesSize = (ulong)information.Drives.Sum(v => (long)v.Size) / 1024 / 1024 / 1024;

            //SMART INFORMATION

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\wmi",
                                "Select * from MSStorageDriver_FailurePredictData");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string instance = queryObj["InstanceName"].ToString();

                    Drive drive = information.Drives.Find(v => v.InstanceName != null && instance.ToLower().Contains(v.InstanceName.ToLower()));

                    if (drive != null)
                    {
                        Byte[] bytes = (Byte[])queryObj.Properties["VendorSpecific"].Value;
                        for (int i = 0; i < 30; ++i)
                        {
                            try
                            {
                                int id = bytes[i * 12 + 2];

                                int flags = bytes[i * 12 + 4];
                                
                                bool failureImminent = (flags & 0x1) == 0x1;

                                int value = bytes[i * 12 + 5];
                                int worst = bytes[i * 12 + 6];
                                int vendordata = BitConverter.ToInt32(bytes, i * 12 + 7);
                                if (id == 0) continue;

                                var attr = drive.SmartAttributes[id];
                                attr.Current = value;
                                attr.Worst = worst;
                                attr.Data = vendordata;
                                attr.IsOK = failureImminent == false;
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }

            //PRINTERS INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Printer"), information.Printers);

            //DEVICES INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "Select * from Win32_PnPentity"), information.Devices);

            //APPLICATIONS INFORMATION

            try
            {
                string SoftwareKey86 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                string SoftwareKey64 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

                information.Applications = GetApps(SoftwareKey86);
                information.Applications.AddRange(GetApps(SoftwareKey64));

                information.Applications = information.Applications.OrderBy(p => p.DisplayName).ToList();
            }
            catch { }

            //STARTUP REGISTRY INFORMATION

            RegistryKey StUpKey;
            try
            {
                StUpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                foreach (string valueName in StUpKey.GetValueNames())
                {
                    information.startUpKeys.Add(new StartUpKey(valueName, StUpKey.GetValue(valueName).ToString()));
                }
            }
            catch { }

            try
            {
                StUpKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                foreach (string valueName in StUpKey.GetValueNames())
                {
                    information.startUpKeys.Add(new StartUpKey(valueName, StUpKey.GetValue(valueName).ToString()));
                }
            }
            catch { }

            //SERVICES INFORMATION

            try
            {
                ServiceController[] scServices;
                scServices = ServiceController.GetServices();


                for (int i = 0; i < scServices.Length; i++)
                {
                    information.Services.Add(new Service(scServices[i].DisplayName,
                        scServices[i].ServiceName,
                        scServices[i].Status.ToString()));
                }
            }
            catch { }

            return information;
        }

        static void InitializeInformation<T>(ManagementObjectSearcher searcher, T objectinfo)
        {
            try
            {
                var properties = objectinfo.GetType().GetProperties();

                using (var results = searcher.Get())
                using (var manobj = results.Cast<ManagementObject>().First())
                {
                    foreach (var property in manobj.Properties)
                    {
                        if (properties.Any(n => n.Name == property.Name))
                        {
                            if (property.Value != null)
                            {
                                var val = property.Value;
                                if (property.Type == CimType.DateTime) { val = ManagementDateTimeConverter.ToDateTime(val.ToString()); }

                                objectinfo.GetType().GetProperty(property.Name).SetValue(objectinfo, val, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        static void InitializeInformation<T>(ManagementObjectSearcher searcher, List<T> objectinfo) where T : new()
        {
            try
            {
                var properties = objectinfo.GetType().GetGenericArguments().Single().GetProperties();

                using (var results = searcher.Get())
                {
                    foreach (var manobj in results)
                    {
                        T newobj = new T();

                        foreach (var property in manobj.Properties)
                        {
                            if (properties.Any(n => n.Name == property.Name))
                            {
                                if (property.Value != null)
                                {
                                    var val = property.Value;

                                    if (property.Value.GetType() == typeof(ushort[])) { val = ShortToString(val); }

                                    //if (newobj.GetType().GetProperty(property.Name).GetType() == property.Value.GetType())
                                    { newobj.GetType().GetProperty(property.Name).SetValue(newobj, val, null); }
                                }
                            }
                        }

                        objectinfo.Add(newobj);
                    }
                }
            }
            catch (Exception ex) { }
        }

        static string ShortToString(object ushortsasobject)
        {
            if (ushortsasobject == null) {  return ""; }

            var ushorts = (UInt16[])ushortsasobject;
            ushorts = ushorts.Where(v => v != 0).ToArray();
            return string.Join("", Array.ConvertAll(ushorts, v => Convert.ToChar(v)));
        }

        public static bool IsVirtualMachine()
        {
            string biosInfo = GetBiosInfo().ToLower();
            bool isVirtualMachine = false;
            string[] MachineInfo = GetMachineInfo();

            string model = MachineInfo[0].ToLower();
            string manufacturer = MachineInfo[1].ToLower();

            isVirtualMachine |= biosInfo.Contains("hyper-v");
            isVirtualMachine |= biosInfo.Contains("virtualbox");
            isVirtualMachine |= biosInfo.Contains("vmware");
            isVirtualMachine |= manufacturer.Contains("microsoft corporation") && !model.Contains("surface");
            isVirtualMachine |= manufacturer.Contains("parallels software");
            isVirtualMachine |= manufacturer.Contains("qemu");
            isVirtualMachine |= manufacturer.Contains("vmware");
            isVirtualMachine |= model.Contains("virtualbox");

            return isVirtualMachine;
        }

        static string GetBiosInfo()
        {
            var manufacturer = "";
            var name = "";

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                using (var results = searcher.Get())
                using (var bios = results.Cast<ManagementObject>().First())
                {
                    foreach (var property in bios.Properties)
                    {
                        if (property.Name == "Manufacturer")
                        {
                            manufacturer = property.Value.ToString();
                        }
                        else if (property.Name == "Name")
                        {
                            name = property.Value.ToString();
                        }
                    }
                }

                return manufacturer + " " + name;
            }
            catch
            {
                return "";
            }
        }

        static string[] GetMachineInfo()
        {
            string SysFamalyModel = "";
            string Manufacturer = "";

            string model = "";
            string systemFamily = "";

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                using (var results = searcher.Get())
                using (var system = results.Cast<ManagementObject>().First())
                {
                    foreach (var property in system.Properties)
                    {
                        if (property.Name.Equals("Manufacturer"))
                        {
                            Manufacturer = Convert.ToString(property.Value);
                        }
                        else if (property.Name.Equals("Model"))
                        {
                            model = Convert.ToString(property.Value);
                        }
                        else if (property.Name.Equals("SystemFamily"))
                        {
                            systemFamily = Convert.ToString(property.Value);
                        }
                    }
                }

                SysFamalyModel = $"{systemFamily} {model}";
            }
            catch (Exception)
            {
                SysFamalyModel = "";
                Manufacturer = "";
            }

            return new string[] { SysFamalyModel, Manufacturer };
        }

        public static List<Application> GetApps(string RegKey)
        {
            List<Application> apps = new List<Application>();

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(RegKey))
            {
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        try
                        {
                            if (sk.GetValue("DisplayName") != null)
                            {
                                Application app = new Application();

                                app.DisplayName = sk.GetValue("DisplayName").ToString();


                                if (sk.GetValue("DisplayVersion") != null)
                                {
                                    app.DisplayVersion = CheckValue(sk.GetValue("DisplayVersion"));
                                }
                                else { app.DisplayVersion = "No version"; }

                                if (sk.GetValue("Publisher") != null)
                                {
                                    app.Publisher = CheckValue(sk.GetValue("Publisher"));
                                }
                                else { app.Publisher = "No publisher"; }

                                apps.Add(app);
                            }
                        }
                        catch { }
                    }
                }
            }

            return apps;
        }

        static string CheckValue(object input)
        {
            if (input != null)
                return input.ToString();
            else
                return string.Empty;
        }

        public static string GetWinVer()
        {
            try
            {
                var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

                string productName = (string)reg.GetValue("ProductName");
                return productName;
            }
            catch
            {
                try
                {
                    string win = "";

                    ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                        "SELECT * FROM Win32_OperatingSystem");

                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        win = queryObj["Caption"].ToString();
                    }
                    return win;
                }
                catch { return Environment.OSVersion.ToString(); }
            }
        }

        static string getuser()
        {
            try
            {
                string _username = "";
                
                foreach (var p in Process.GetProcessesByName("explorer"))
                {
                    _username = GetProcessOwner(p.Id);
                }

                // remove the domain part from the username
                var usernameParts = _username.Split('\\');

                if (usernameParts.Length > 0)
                {
                    _username = usernameParts[usernameParts.Length - 1];
                }
                if (_username == "") { throw new Exception(); }

                return _username;
            }
            catch
            {
                try
                {
                    EventLog[] eventLogs = EventLog.GetEventLogs();

                    var evlog = eventLogs.Where(t => t.Log == "Security").ToArray();

                    List<EventLogEntry> evle = new List<EventLogEntry> { };

                    foreach (EventLogEntry evlogEvent in evlog[0].Entries)
                    {
                        if (evlogEvent.InstanceId == 4624 && (evlogEvent.ReplacementStrings[8] == "11" || evlogEvent.ReplacementStrings[8] == "2"))
                        {
                            evle.Add(evlogEvent);
                        }
                    }

                    while (evle.Last().ReplacementStrings[5].Contains("DWM") || evle.Last().ReplacementStrings[5].Contains("UMFD"))
                    {
                        evle.RemoveAt(evle.Count - 1);
                    }

                    return evle.Last().ReplacementStrings[5];
                }
                catch { return "Нет сведений"; }
            }
        }

        public static string GetProcessOwner(int processId)
        {
            var query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectCollection processList;

            using (var searcher = new ManagementObjectSearcher(query))
            {
                processList = searcher.Get();
            }

            foreach (var mo in processList.OfType<ManagementObject>())
            {
                object[] argList = { string.Empty, string.Empty };
                var returnVal = Convert.ToInt32(mo.InvokeMethod("GetOwner", argList));

                if (returnVal == 0)
                {
                    // return DOMAIN\user
                    return argList[1] + "\\" + argList[0];
                }
            }

            return "NO OWNER";
        }

        static bool loginasadmin()
        {
            try
            {
                bool istrue = false;
                DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName);
                DirectoryEntry admGroup = localMachine.Children.Find("Administrators", "group");

                foreach (DirectoryEntry item in localMachine.Children)
                {
                    if (item.Name == "Administrators" || item.Name == "Администраторы") { admGroup = item; break; }
                }

                string user = getuser();

                object members = admGroup.Invoke("members", null);

                foreach (object groupMember in (IEnumerable)members)
                {
                    DirectoryEntry member = new DirectoryEntry(groupMember);
                    if (user == member.Name) { istrue = true; break; }
                }

                return istrue;
            }
            catch
            {
                return false;
            }
        }
    }
}