using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInformation
{
    public class CPU
    {
        public string Name { get; set; }
        public uint MaxClockSpeed { get; set; }
        public uint NumberOfCores { get; set; }
        public string ProcessorId { get; set; }
        public ushort UpgradeMethod { get; set; }

        public CPU() { }
        public CPU(string name, uint maxClockSpeed, uint numberOfCores, string processorId, ushort upgradeMethod)
        {
            Name = name;
            MaxClockSpeed = maxClockSpeed;
            NumberOfCores = numberOfCores;
            ProcessorId = processorId;
            UpgradeMethod = upgradeMethod;
        }
    }
}
