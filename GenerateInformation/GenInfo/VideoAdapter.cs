namespace GenerateInformation.GenInfo
{
    public class VideoAdapter
    {
        public VideoAdapter() { }

        public string Name { get; set; }
        public string AdapterCompatibility { get; set; }
        public uint CurrentHorizontalResolution { get; set; }
        public uint CurrentVerticalResolution { get; set; }
        public long AdapterRAM { get; set; }
    }
}