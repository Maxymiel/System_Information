using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInformation
{
    public class VideoAdapter
    {
        public string Name { get; set; }
        public string AdapterCompatibility { get; set; }
        public uint CurrentHorizontalResolution { get; set; }
        public uint CurrentVerticalResolution { get; set;}
        public long AdapterRAM { get; set; }

        public VideoAdapter (string name, string adapterCompatibility, uint currentHorizontalResolution, uint currentVerticalResolution, long adapterRAM)
        {
            Name = name;
            AdapterCompatibility = adapterCompatibility;
            CurrentHorizontalResolution = currentHorizontalResolution;
            CurrentVerticalResolution = currentVerticalResolution;
            AdapterRAM = adapterRAM;
        }

        public VideoAdapter()
        {
            
        }
    }
}
