using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInformation
{
    public class Marks
    {
        public float CPUScore { get; set; }
        public float DiskScore { get; set; }
        public float GraphicsScore { get; set; }
        public float MemoryScore { get; set; }
        public ulong TotalmemMark { get; set; }
        public ulong TotalDrivesSize { get; set; }

        public double TotalMark { get; set; }

        public Marks() { }
    }
}
