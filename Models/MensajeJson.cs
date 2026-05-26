namespace GasoStation.Models
{
    // Mensaje que el panel central ENVÍA al Arduino
    public class ComandoBomba
    {
        public string tipo { get; set; } = "";       // "iniciar" | "detener"
        public int bomba_id { get; set; }
        public string modo { get; set; } = "";        // "prepago" | "tanque_lleno"
        public decimal litros_max { get; set; }       // Solo para prepago
    }

    // Mensaje que el Arduino DEVUELVE al panel
    public class RespuestaBomba
    {
        public string evento { get; set; } = "";      // "progreso" | "completado" | "error"
        public int bomba_id { get; set; }
        public decimal litros_servidos { get; set; }
        public bool completado { get; set; }
        public string? mensaje { get; set; }
    }
}
