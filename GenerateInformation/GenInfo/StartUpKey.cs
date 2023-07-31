namespace GenerateInformation.GenInfo
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
