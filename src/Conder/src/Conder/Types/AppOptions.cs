namespace Conder.Types
{
    public class AppOptions
    {
        public string Name { get; set; }
        public string Service { get; set; }
        public string Instance { get; set; }
        public string Version { get; set; }
        public double ShutdownTimeout { get; set; } = 5;
        public bool DisplayBanner { get; set; } = true;
        public bool DisplayVersion { get; set; } = true;
    }
}