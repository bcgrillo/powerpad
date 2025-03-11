namespace PowerPad.Core.Models
{
    public class ModelInfo
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public ModelStatus Status { get; set; } = ModelStatus.Unkown;

        public ModelInfo(string name, string version)
        {
            Name = name;
            Version = version;
        }
    }
}