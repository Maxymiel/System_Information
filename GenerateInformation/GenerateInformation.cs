using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using GenerateInformation.GenInfo;
using Microsoft.Win32;

namespace GenerateInformation
{
    public class GenerateInformation
    {
        public static General Generation()
        {
            var information = new General();

            //GENERAL INFORMATION

            information.GenDate = DateTime.Now;
            information.MachineName = Environment.MachineName;

            information.IsVirtualMachine = IsVirtualMachine();
            information.UserName = Getuser();
            information.LoginAsAdministrator = Loginasadmin();
            information.WinVersion = GetWinVer();

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem"), information)?.AddList(information.Errors);

            //NETWORK INFORMATION

            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var adapter in adapters)
                {
                    var properties = adapter.GetIPProperties();
                    foreach (var ip in adapter.GetIPProperties().UnicastAddresses)
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            var MAC = "";
                            try
                            {
                                MAC = adapter.GetPhysicalAddress().ToString();
                            }
                            catch { }

                            information.NetworkAdapters.Add(new NetworkAdapter(adapter.Name, ip.Address.ToString(), adapter.Description, properties.DnsSuffix, adapter.Speed / 1000 / 1000, MAC));
                        }
                }
            }
            catch (Exception ex) { ex.Source = "NETWORK INFORMATION"; information.Errors.Add(ex); }

            //MOTHERBOARD INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard"), information.Motherboard)?.AddList(information.Errors);

            //VIDEO INFORMATION

            using (var rk = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"))
            {
                var subkeys = rk.GetSubKeyNames();
                subkeys = subkeys.Where(t => Regex.IsMatch(t, @"\d*")).ToArray();

                for (var i = 0; i < subkeys.Length; i++)
                {
                    using (var rk2 = rk.OpenSubKey(subkeys[i]))
                    {
                        if (rk2.GetValue("DriverDesc") != null)
                        {
                            var videoAdapter = new VideoAdapter();

                            videoAdapter.Name = rk2.GetValue("DriverDesc").ToString();

                            if (rk2.GetValue("ProviderName") != null)
                                videoAdapter.AdapterCompatibility = CheckValue(rk2.GetValue("ProviderName"));
                            else
                                videoAdapter.AdapterCompatibility = "No provider name";

                            if (rk2.GetValue("HardwareInformation.qwMemorySize") != null)
                            {
                                videoAdapter.AdapterRAM = long.Parse(CheckValue(rk2.GetValue("HardwareInformation.qwMemorySize")));
                                videoAdapter.AdapterRAM = videoAdapter.AdapterRAM / 1024 / 1024;
                            }
                            else
                            {
                                videoAdapter.AdapterRAM = 0;
                            }

                            information.VideoAdapter = videoAdapter;

                            break;
                        }
                    }
                }
            }

            if (information.VideoAdapter.AdapterRAM == 0)
            {
                InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController"), information.VideoAdapter)?.AddList(information.Errors);
                information.VideoAdapter.AdapterRAM = information.VideoAdapter.AdapterRAM / 1024 / 1024;
            }

            //CPU INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"), information.CPUs)?.AddList(information.Errors);

            //RAM INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory"), information.RAMArray.RAM)?.AddList(information.Errors);

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM win32_PhysicalMemoryArray"), information.RAMArray)?.AddList(information.Errors);

            //MONITOR INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\wmi", "SELECT * FROM WmiMonitorID"), information.Monitors)?.AddList(information.Errors);

            //DRIVE INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "Select * from Win32_DiskDrive"), information.Drives)?.AddList(information.Errors);

            try
            {
                //Primary disk detect

                foreach (var drive in information.Drives)
                {
                    var partitionQuery = new ManagementObjectSearcher("associators of {Win32_DiskDrive.DeviceID=\"" + drive.DeviceID.Replace("\\", "\\\\") +
                                                                      "\"} where AssocClass = Win32_DiskDriveToDiskPartition");
                    foreach (ManagementObject partition in partitionQuery.Get().Cast<ManagementObject>())
                    {
                        var logicalDriveQuery = new ManagementObjectSearcher("associators of {" + partition.Path.RelativePath + "} where AssocClass = Win32_LogicalDiskToPartition");
                        foreach (ManagementObject ld in logicalDriveQuery.Get().Cast<ManagementObject>())
                        {
                            var driveId = Convert.ToString(ld.Properties["DeviceId"].Value);

                            if (driveId == information.SystemDrive) drive.IsPrimary = true;
                        }
                    }
                }

                //Get MediaType

                try
                {
                    var PhysicalDiskQuery = new ManagementObjectSearcher(@"root\Microsoft\Windows\Storage", "SELECT * FROM MSFT_PhysicalDisk");
                    foreach (ManagementObject PhysicalDisk in PhysicalDiskQuery.Get().Cast<ManagementObject>())
                    {
                        if (PhysicalDisk["SerialNumber"].ToString() != "")
                        {
                            var drive = information.Drives.Find(t => t.SerialNumber != null && t.SerialNumber.Contains(PhysicalDisk["SerialNumber"].ToString()));
                            if (drive != null) drive.MediaType = PhysicalDisk["MediaType"];
                        }
                    }
                }
                catch (Exception ex) { ex.Source = "MediaType"; information.Errors.Add(ex); }

                //Get instances

                var searcher = new ManagementObjectSearcher("root\\CIMV2", "Select * from Win32_DiskDrive");

                foreach (ManagementObject queryObj in searcher.Get().Cast<ManagementObject>())
                {
                    foreach (var property in queryObj.Properties)
                    {
                        if (property.Name == "SerialNumber" && property.Value != null)
                        {
                            var sn = property.Value.ToString().ToLower();

                            var drive = information.Drives.Find(t => t.SerialNumber != null && sn.Contains(t.SerialNumber.ToLower()));
                            if (drive != null) drive.InstanceName = queryObj["PNPDeviceID"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex) { ex.Source = "DiskDetect"; information.Errors.Add(ex); }

            //MARKS INFORMATION

            information.Marks = new Marks();

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_WinSAT"), information.Marks)?.AddList(information.Errors);

            information.Marks.TotalmemMark = information.RAMArray.RAMCapacity;
            information.Marks.TotalDrivesSize = (ulong)information.Drives.Sum(v => (long)v.Size) / 1024 / 1024 / 1024;

            //SMART INFORMATION

            try
            {
                var searcher = new ManagementObjectSearcher("root\\wmi", "Select * from MSStorageDriver_FailurePredictData");

                foreach (ManagementObject queryObj in searcher.Get().Cast<ManagementObject>())
                {
                    var instance = queryObj["InstanceName"].ToString();

                    var drive = information.Drives.Find(v => v.InstanceName != null && instance.ToLower().Contains(v.InstanceName.ToLower()));

                    if (drive != null)
                    {
                        var bytes = (byte[])queryObj.Properties["VendorSpecific"].Value;
                        for (var i = 0; i < 30; ++i)
                        {
                            try
                            {
                                int id = bytes[i * 12 + 2];

                                int flags = bytes[i * 12 + 4];

                                var failureImminent = (flags & 0x1) == 0x1;

                                int value = bytes[i * 12 + 5];
                                int worst = bytes[i * 12 + 6];
                                var vendordata = BitConverter.ToInt32(bytes, i * 12 + 7);
                                if (id == 0) continue;

                                var attr = drive.SmartAttributes[id];
                                attr.Current = value;
                                attr.Worst = worst;
                                attr.Data = vendordata;
                                attr.IsOK = failureImminent == false;
                            }
                            catch (Exception ex) { ex.Source = "SMART INFORMATION CONVERTER"; information.Errors.Add(ex); }
                        }
                    }
                }
            }
            catch (Exception ex) { ex.Source = "SMART INFORMATION"; information.Errors.Add(ex); }

            //PRINTERS INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Printer"), information.Printers)?.AddList(information.Errors);

            //DEVICES INFORMATION

            InitializeInformation(new ManagementObjectSearcher("root\\CIMV2", "Select * from Win32_PnPentity"), information.Devices)?.AddList(information.Errors);

            //APPLICATIONS INFORMATION

            try
            {
                var SoftwareKey86 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                var SoftwareKey64 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

                information.Applications = GetApps(SoftwareKey86);
                information.Applications.AddRange(GetApps(SoftwareKey64));

                information.Applications = information.Applications.OrderBy(p => p.DisplayName).ToList();
            }
            catch (Exception ex) { ex.Source = "APPLICATIONS INFORMATION"; information.Errors.Add(ex); }

            //STARTUP REGISTRY INFORMATION

            RegistryKey StUpKey;
            try
            {
                StUpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                foreach (var valueName in StUpKey.GetValueNames()) information.startUpKeys.Add(new StartUpKey(valueName, StUpKey.GetValue(valueName).ToString()));
            }
            catch (Exception ex) { ex.Source = "STARTUP REGISTRY INFORMATION - HKLM"; information.Errors.Add(ex); }

            try
            {
                StUpKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                foreach (var valueName in StUpKey.GetValueNames()) information.startUpKeys.Add(new StartUpKey(valueName, StUpKey.GetValue(valueName).ToString()));
            }
            catch (Exception ex) { ex.Source = "STARTUP REGISTRY INFORMATION - HKCU"; information.Errors.Add(ex); }

            //SERVICES INFORMATION

            try
            {
                ServiceController[] scServices;
                scServices = ServiceController.GetServices();


                for (var i = 0; i < scServices.Length; i++)
                    information.Services.Add(new Service(scServices[i].DisplayName, scServices[i].ServiceName, scServices[i].Status.ToString()));
            }
            catch (Exception ex) { ex.Source = "SERVICES INFORMATION"; information.Errors.Add(ex); }

            return information;
        }

        private static Exception InitializeInformation<T>(ManagementObjectSearcher searcher, T objectinfo)
        {
            try
            {
                var properties = objectinfo.GetType().GetProperties();

                using (var results = searcher.Get())
                using (var manobj = results.Cast<ManagementObject>().First())
                {
                    foreach (var property in manobj.Properties)
                        if (properties.Any(n => n.Name == property.Name))
                            if (property.Value != null)
                            {
                                var val = property.Value;
                                if (property.Type == CimType.DateTime) val = ManagementDateTimeConverter.ToDateTime(val.ToString());

                                objectinfo.GetType().GetProperty(property.Name).SetValue(objectinfo, val, null);
                            }
                }

                return null;
            }
            catch (Exception ex) { return ex; }
        }

        private static Exception InitializeInformation<T>(ManagementObjectSearcher searcher, List<T> objectinfo) where T : new()
        {
            try
            {
                var properties = objectinfo.GetType().GetGenericArguments().Single().GetProperties();

                using (var results = searcher.Get())
                {
                    foreach (var manobj in results)
                    {
                        var newobj = new T();

                        foreach (var property in manobj.Properties)
                            if (properties.Any(n => n.Name == property.Name))
                                if (property.Value != null)
                                {
                                    var val = property.Value;

                                    if (property.Value.GetType() == typeof(ushort[])) val = ShortToString(val);

                                    newobj.GetType().GetProperty(property.Name).SetValue(newobj, val, null);
                                }

                        objectinfo.Add(newobj);
                    }
                }

                return null;
            }
            catch (Exception ex) { return ex; }
        }

        private static string ShortToString(object ushortsasobject)
        {
            if (ushortsasobject == null) return "";

            var ushorts = (ushort[])ushortsasobject;
            ushorts = ushorts.Where(v => v != 0).ToArray();
            return string.Join("", Array.ConvertAll(ushorts, v => Convert.ToChar(v)));
        }

        public static bool IsVirtualMachine()
        {
            var biosInfo = GetBiosInfo().ToLower();
            var isVirtualMachine = false;
            var MachineInfo = GetMachineInfo();

            var model = MachineInfo[0].ToLower();
            var manufacturer = MachineInfo[1].ToLower();

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

        private static string GetBiosInfo()
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
                        if (property.Name == "Manufacturer")
                            manufacturer = property.Value.ToString();
                        else if (property.Name == "Name") name = property.Value.ToString();
                }

                return manufacturer + " " + name;
            }
            catch
            {
                return "";
            }
        }

        private static string[] GetMachineInfo()
        {
            var SysFamalyModel = "";
            var Manufacturer = "";

            var model = "";
            var systemFamily = "";

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                using (var results = searcher.Get())
                using (var system = results.Cast<ManagementObject>().First())
                {
                    foreach (var property in system.Properties)
                        if (property.Name.Equals("Manufacturer"))
                            Manufacturer = Convert.ToString(property.Value);
                        else if (property.Name.Equals("Model"))
                            model = Convert.ToString(property.Value);
                        else if (property.Name.Equals("SystemFamily")) systemFamily = Convert.ToString(property.Value);
                }

                SysFamalyModel = $"{systemFamily} {model}";
            }
            catch (Exception)
            {
                SysFamalyModel = "";
                Manufacturer = "";
            }

            return new[] { SysFamalyModel, Manufacturer };
        }

        public static List<Application> GetApps(string RegKey)
        {
            var apps = new List<Application>();

            using (var rk = Registry.LocalMachine.OpenSubKey(RegKey))
            {
                foreach (var skName in rk.GetSubKeyNames())
                    using (var sk = rk.OpenSubKey(skName))
                    {
                        try
                        {
                            if (sk.GetValue("DisplayName") != null)
                            {
                                var app = new Application();

                                app.DisplayName = sk.GetValue("DisplayName").ToString();


                                if (sk.GetValue("DisplayVersion") != null)
                                    app.DisplayVersion = CheckValue(sk.GetValue("DisplayVersion"));
                                else
                                    app.DisplayVersion = "No version";

                                if (sk.GetValue("Publisher") != null)
                                    app.Publisher = CheckValue(sk.GetValue("Publisher"));
                                else
                                    app.Publisher = "No publisher";

                                apps.Add(app);
                            }
                        }
                        catch { }
                    }
            }

            return apps;
        }

        private static string CheckValue(object input)
        {
            if (input != null)
                return input.ToString();
            return string.Empty;
        }

        public static string GetWinVer()
        {
            try
            {
                var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

                var productName = (string)reg.GetValue("ProductName");
                return productName;
            }
            catch
            {
                try
                {
                    var win = "";

                    var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");

                    foreach (ManagementObject queryObj in searcher.Get().Cast<ManagementObject>()) win = queryObj["Caption"].ToString();
                    return win;
                }
                catch
                {
                    return Environment.OSVersion.ToString();
                }
            }
        }

        private static string Getuser()
        {
            try
            {
                var _username = "";

                foreach (var p in Process.GetProcessesByName("explorer")) _username = GetProcessOwner(p.Id);

                // remove the domain part from the username
                var usernameParts = _username.Split('\\');

                if (usernameParts.Length > 0) _username = usernameParts[usernameParts.Length - 1];
                if (_username == "") throw new Exception();

                return _username;
            }
            catch
            {
                try
                {
                    var eventLogs = EventLog.GetEventLogs();

                    var evlog = eventLogs.Where(t => t.Log == "Security").ToArray();

                    var evle = new List<EventLogEntry>();

                    foreach (EventLogEntry evlogEvent in evlog[0].Entries)
                        if (evlogEvent.InstanceId == 4624 && (evlogEvent.ReplacementStrings[8] == "11" || evlogEvent.ReplacementStrings[8] == "2"))
                            evle.Add(evlogEvent);

                    while (evle.Last().ReplacementStrings[5].Contains("DWM") || evle.Last().ReplacementStrings[5].Contains("UMFD")) evle.RemoveAt(evle.Count - 1);

                    return evle.Last().ReplacementStrings[5];
                }
                catch
                {
                    return "Нет сведений";
                }
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
                    // return DOMAIN\user
                    return argList[1] + "\\" + argList[0];
            }

            return "NO OWNER";
        }

        private static bool Loginasadmin()
        {
            try
            {
                var istrue = false;
                var localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName);
                var admGroup = localMachine.Children.Find("Administrators", "group");

                foreach (DirectoryEntry item in localMachine.Children)
                    if (item.Name == "Administrators" || item.Name == "Администраторы")
                    {
                        admGroup = item;
                        break;
                    }

                var user = Getuser();

                var members = admGroup.Invoke("members", null);

                foreach (var groupMember in (IEnumerable)members)
                {
                    var member = new DirectoryEntry(groupMember);
                    if (user == member.Name)
                    {
                        istrue = true;
                        break;
                    }
                }

                return istrue;
            }
            catch
            {
                return false;
            }
        }
    }
    public static class ExtensionAdd
    {
        public static void AddList(this Exception error, List<Exception> list)
        {
            list.Add(error);
        }
    }
}