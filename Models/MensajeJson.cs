namespace GasoStation.Models
{
    public class ComandoBomba
    {
        public string cmd { get; set; } = "";
        public int bomba { get; set; }
        public string modo { get; set; } = "";
        public int q { get; set; }
        public int ml { get; set; }
    }

    public class RespuestaBomba
    {
        public int bomba { get; set; }
        public string estado { get; set; } = "";
        public int q { get; set; }
        public int qt { get; set; }
        public float gal { get; set; }
        public float galt { get; set; }
        public string? error { get; set; }
        public int[]? niveles { get; set; }
    }
}