namespace FixterJail.Client.Models
{
    public class Config
    {
        public Locations Locations { get; set; } = new();
    }

    public class Locations
    {
        public List<Position> PoliceDepartments { get; set; } = new();
        public Position Jail { get; set; } = new();
        public Position JailRelease { get; set; } = new();
    }

    public class Position
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

}
