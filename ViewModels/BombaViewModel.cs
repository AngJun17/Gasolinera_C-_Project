using GasoStation.Helpers;
using GasoStation.Models;

namespace GasoStation.ViewModels
{
    public class BombaViewModel : ViewModelBase
    {
        public int Id { get; }
        public string NombreBomba => $"Bomba {Id}";

        // Estado 
        private EstadoBomba _estado = EstadoBomba.Libre;
        public EstadoBomba Estado
        {
            get => _estado;
            set
            {
                SetProperty(ref _estado, value);
                OnPropertyChanged(nameof(EstaLibre));
                OnPropertyChanged(nameof(EstaActiva));
            }
        }
        public bool EstaLibre  => Estado == EstadoBomba.Libre;
        public bool EstaActiva => Estado == EstadoBomba.Activa;

        // Servicio activo 
        private string _clienteActual = "";
        public string ClienteActual { get => _clienteActual; set => SetProperty(ref _clienteActual, value); }

        private TipoServicio _tipo;
        public TipoServicio Tipo { get => _tipo; set => SetProperty(ref _tipo, value); }

        private decimal _montoPagado;
        public decimal MontoPagado { get => _montoPagado; set => SetProperty(ref _montoPagado, value); }

        private decimal _litrosSolicitados;
        public decimal LitrosSolicitados { get => _litrosSolicitados; set => SetProperty(ref _litrosSolicitados, value); }

        private decimal _litrosServidos;
        public decimal LitrosServidos
        {
            get => _litrosServidos;
            set { SetProperty(ref _litrosServidos, value); OnPropertyChanged(nameof(AnchoBarraProgreso)); OnPropertyChanged(nameof(ProgresoTexto)); }
        }

        // Info cuando está libre 
        private int _totalServicios;
        public int TotalServicios { get => _totalServicios; set => SetProperty(ref _totalServicios, value); }

        private string _ultimoCliente = "—";
        public string UltimoCliente { get => _ultimoCliente; set => SetProperty(ref _ultimoCliente, value); }

        // Calculadas 
        public string EtiquetaTipo => Tipo == TipoServicio.Prepago
            ? $"Prepago · Q{MontoPagado:F2}"
            : "Tanque lleno";

        public string ProgresoTexto => Tipo == TipoServicio.TanqueLleno
            ? $"{LitrosServidos:F2} L (midiendo)"
            : $"{LitrosServidos:F2} / {LitrosSolicitados:F2} L";

        // Ancho en píxeles para la barra (máx ~180px dentro de la tarjeta)
        public double AnchoBarraProgreso
        {
            get
            {
                if (LitrosSolicitados <= 0 || EstaLibre) return 0;
                double pct = (double)(LitrosServidos / LitrosSolicitados);
                return pct * 180; // 180 = ancho visual aprox de la tarjeta
            }
        }

        // Constructor 
        public BombaViewModel(int id) => Id = id;

        // Acciones 
        public void IniciarServicio(string cliente, TipoServicio tipo, decimal monto, decimal litros)
        {
            ClienteActual     = cliente;
            Tipo              = tipo;
            MontoPagado       = monto;
            LitrosSolicitados = litros;
            LitrosServidos    = 0;
            Estado            = EstadoBomba.Activa;
        }

        public void ActualizarProgreso(decimal litros) => LitrosServidos = litros;

        public void FinalizarServicio(string ultimoCliente)
        {
            UltimoCliente = ultimoCliente;
            TotalServicios++;
            ClienteActual     = "";
            MontoPagado       = 0;
            LitrosSolicitados = 0;
            LitrosServidos    = 0;
            Estado            = EstadoBomba.Libre;
        }
    }
}
