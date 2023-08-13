namespace GenerateInformation.GenInfo
{
    public class CPU
    {
        public CPU() { }
        public string Name { get; set; }
        public uint MaxClockSpeed { get; set; }
        public uint NumberOfCores { get; set; }
        public string ProcessorId { get; set; }
        public ushort UpgradeMethod { get; set; }
    }
}