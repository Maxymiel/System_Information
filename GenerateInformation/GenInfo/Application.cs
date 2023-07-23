using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateInformation
{
    public class Application
    {
        public string DisplayName { get; set; }
        public string DisplayVersion { get; set; }
        public string Publisher { get; set; }

        public Application()
        {

        }

        public Application(string displayName, string displayVersion, string publisher)
        {
            DisplayName = displayName;
            DisplayVersion = displayVersion;
            Publisher = publisher;
        }
    }
}