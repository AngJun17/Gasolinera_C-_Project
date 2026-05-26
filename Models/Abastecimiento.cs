using System;

namespace GasoStation.Models
{
    public class Abastecimiento
    {
        public int Id { get; set; }
        public int BombaId { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public TipoServicio Tipo { get; set; }
        public decimal MontoPagado { get; set; }       // Solo prepago
        public decimal LitrosSolicitados { get; set; } // Solo prepago
        public decimal LitrosServidos { get; set; }
        public decimal MontoFinal { get; set; }
        public DateTime FechaHora { get; set; } = DateTime.Now;
        public bool Completado { get; set; } = false;

        // Calculado: cuánto falta devolver si prepago no terminó
        public decimal DiferenciaDevolucion => MontoPagado - MontoFinal;
    }
}
