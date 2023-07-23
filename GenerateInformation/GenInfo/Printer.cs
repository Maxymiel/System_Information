﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateInformation
{
    public class Printer
    {
        public string Name { get; set; }
        public string DeviceID { get; set; }
        public string DriverName { get; set; }
        public string PortName { get; set; }
        public bool Shared { get; set; }
        public bool Network { get; set; }
        public string ErrorInformation { get; set; }

        public Printer() { }
    }
}