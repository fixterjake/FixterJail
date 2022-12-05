namespace FixterJail.Shared.Models
{
    public class Config
    {
        [JsonProperty("locations")]
        public Locations Locations { get; set; } = new();
        
        [JsonProperty("jailTimeMax")]
        public int JailTimeMaximum { get; set; } = 600;
    }

    public class Locations
    {
        [JsonProperty("policeDepartments")]
        public List<Position> PoliceDepartments { get; set; } = new();
        
        [JsonProperty("jail")]
        public Position Jail { get; set; } = new();

        [JsonProperty("jailRelease")]
        public Position JailRelease { get; set; } = new();
    }

    public class Position
    {
        [JsonProperty("x")]
        public float x { get; set; }

        [JsonProperty("y")]
        public float y { get; set; }

        [JsonProperty("z")]
        public float z { get; set; }
    }

}
