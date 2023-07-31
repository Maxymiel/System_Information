using System;
using System.Collections.Generic;

namespace GenerateInformation.GenInfo
{
    public class General
    {
        public DateTime GenDate { get; set; }
        public string MachineName { get; set; }
        public string WinVersion { get; set; }
        public string SerialNumber { get; set; }
        public string OSArchitecture { get; set; }
        public string CSName { get; set; }
        public string Description { get; set; }
        public bool IsVirtualMachine { get; set; }
        public DateTime InstallDate { get; set; }
        public DateTime LastBootUpTime { get; set; }
        public string SystemDrive { get; set; }
        public string UserName { get; set; }
        public bool LoginAsAdministrator { get; set; }
        public Motherboard Motherboard = new Motherboard();
        public VideoAdapter VideoAdapter { get; set; }
        public Marks Marks { get; set; }
        

        public List<NetworkAdapter> NetworkAdapters = new List<NetworkAdapter>();
        public List<CPU> CPUs = new List<CPU>();
        public RAMArray RAMArray = new RAMArray();
        public List<Drive> Drives = new List<Drive>();
        public List<Printer> Printers = new List<Printer>();
        public List<Device> Devices = new List<Device>();
        public List<Application> Applications = new List<Application>();
        public List<StartUpKey> startUpKeys = new List<StartUpKey>();
        public List<Service> Services = new List<Service>();
        public List<Monitor> Monitors = new List<Monitor>();

        public Dictionary<int, Smart> GetArrtibPrimaryDriveOrEmpty()
        {
            Drive primarydrive = this.Drives.Find(t => t.IsPrimary);

            if (primarydrive != null)
            {
                return primarydrive.SmartAttributes;
            }
            else { return new Dictionary<int, Smart>(); }
        }

        public bool IsCorrect()
        {
            bool Correct = true;

            if (MachineName == null || MachineName == "") { Correct = false; }
            if (OSArchitecture == null || OSArchitecture == "") { Correct = false; }
            if (Motherboard == null) { Correct = false; }
            if (CPUs.Count == 0) { Correct = false; }
            if (Motherboard == null) { Correct = false; }
            if (Motherboard.Product == null) { Correct = false; }
            if (RAMArray.RAM.Count == 0) { Correct = false; }

            return Correct;
        }
    }
}
