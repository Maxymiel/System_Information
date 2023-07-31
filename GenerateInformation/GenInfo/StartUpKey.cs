namespace GenerateInformation.GenInfo
{
    public class StartUpKey
    {
        public StartUpKey(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}