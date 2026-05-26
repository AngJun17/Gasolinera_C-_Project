namespace GasoStation.Models
{
    public enum EstadoBomba { Libre, Activa, EnEspera }
    public enum TipoServicio { Prepago, TanqueLleno }

    public class Bomba
    {
        public int Id { get; set; }
        public EstadoBomba Estado { get; set; } = EstadoBomba.Libre;
        public Abastecimiento? ServicioActual { get; set; }

        public Bomba(int id) => Id = id;
    }
}
