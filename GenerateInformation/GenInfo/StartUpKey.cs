using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateInformation
{
    public class StartUpKey
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public StartUpKey(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
