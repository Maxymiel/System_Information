using System;
using System.Collections.Generic;

namespace GenerateInformation.GenInfo
{
    public class General
    {
        public List<Application> Applications = new List<Application>();
        public List<CPU> CPUs = new List<CPU>();
        public List<Device> Devices = new List<Device>();
        public List<Drive> Drives = new List<Drive>();
        public List<Monitor> Monitors = new List<Monitor>();
        public Motherboard Motherboard = new Motherboard();


        public List<NetworkAdapter> NetworkAdapters = new List<NetworkAdapter>();
        public List<Printer> Printers = new List<Printer>();
        public RAMArray RAMArray = new RAMArray();
        public List<Service> Services = new List<Service>();
        public List<StartUpKey> startUpKeys = new List<StartUpKey>();
        public DateTime GenDate { get; set; }
        public string MachineName { get; set; }
        public string WinVersion { get; set; }
        public string SerialNumber { get; set; }
        public string OSArchitecture { get; set; }
        public string Description { get; set; }
        public bool IsVirtualMachine { get; set; }
        public DateTime InstallDate { get; set; }
        public DateTime LastBootUpTime { get; set; }
        public string SystemDrive { get; set; }
        public string UserName { get; set; }
        public bool LoginAsAdministrator { get; set; }
        public VideoAdapter VideoAdapter { get; set; }
        public Marks Marks { get; set; }

        public Dictionary<int, Smart> GetArrtibPrimaryDriveOrEmpty()
        {
            var primarydrive = Drives.Find(t => t.IsPrimary);

            if (primarydrive != null)
                return primarydrive.SmartAttributes;
            return new Dictionary<int, Smart>();
        }

        public bool IsCorrect()
        {
            var Correct = true;

            if (MachineName == null || MachineName == "") Correct = false;
            if (OSArchitecture == null || OSArchitecture == "") Correct = false;
            if (Motherboard == null) Correct = false;
            if (CPUs.Count == 0) Correct = false;
            if (Motherboard == null) Correct = false;
            if (Motherboard.Product == null) Correct = false;
            if (RAMArray.RAM.Count == 0) Correct = false;

            return Correct;
        }
    }
}