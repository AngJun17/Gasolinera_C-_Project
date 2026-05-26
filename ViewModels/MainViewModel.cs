using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GasoStation.Helpers;
using GasoStation.Models;
using GasoStation.Services;

namespace GasoStation.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly HistorialService _historial = new();
        private readonly ArduinoService  _arduino    = new();

        // Bombas 
        public ObservableCollection<BombaViewModel> Bombas { get; } = new();
        public ObservableCollection<BombaViewModel> BombasLibres { get; } = new();

        //  Formulario 
        private string _clienteNuevo = "";
        public string ClienteNuevo { get => _clienteNuevo; set => SetProperty(ref _clienteNuevo, value); }

        private BombaViewModel? _bombaSeleccionada;
        public BombaViewModel? BombaSeleccionada { get => _bombaSeleccionada; set => SetProperty(ref _bombaSeleccionada, value); }

        // 0 = Prepago, 1 = TanqueLleno
        private int _tipoSeleccionado = 0;
        public int TipoSeleccionado
        {
            get => _tipoSeleccionado;
            set { SetProperty(ref _tipoSeleccionado, value); OnPropertyChanged(nameof(EsPrepago)); }
        }
        public bool EsPrepago => TipoSeleccionado == 0;

        private decimal _montoPrepago;
        public decimal MontoPrepago { get => _montoPrepago; set => SetProperty(ref _montoPrepago, value); }

        // Estadísticas 
        private decimal _totalHoy;
        public decimal TotalHoy { get => _totalHoy; set => SetProperty(ref _totalHoy, value); }

        private int _serviciosHoy;
        public int ServiciosHoy { get => _serviciosHoy; set => SetProperty(ref _serviciosHoy, value); }

        private int _serviciosPrepago;
        public int ServiciosPrepago { get => _serviciosPrepago; set => SetProperty(ref _serviciosPrepago, value); }

        private int _serviciosTanqueLleno;
        public int ServiciosTanqueLleno { get => _serviciosTanqueLleno; set => SetProperty(ref _serviciosTanqueLleno, value); }

        private string _bombaLider = "—";
        public string BombaLider { get => _bombaLider; set => SetProperty(ref _bombaLider, value); }

        private int _bombaLiderServicios;
        public int BombaLiderServicios { get => _bombaLiderServicios; set => SetProperty(ref _bombaLiderServicios, value); }

        // Conexión Arduino 
        private string _puertoSeleccionado = "COM3";
        public string PuertoSeleccionado { get => _puertoSeleccionado; set => SetProperty(ref _puertoSeleccionado, value); }

        private bool _conectado = false;
        public bool Conectado { get => _conectado; set { SetProperty(ref _conectado, value); OnPropertyChanged(nameof(TextoConexion)); } }
        public string TextoConexion => Conectado ? "Conectado" : "Desconectado";

        // Precio 
        private decimal _precioLitro = 10.50m;
        public decimal PrecioLitro { get => _precioLitro; set => SetProperty(ref _precioLitro, value); }

        // Comandos 
        public RelayCommand IniciarServicioCommand { get; }
        public RelayCommand CancelarCommand        { get; }
        public RelayCommand DetenerBombaCommand    { get; }
        public RelayCommand NavPanelCommand        { get; }
        public RelayCommand NavHistorialCommand    { get; }
        public RelayCommand NavCierreCajaCommand   { get; }

      
        public MainViewModel()
        {
            for (int i = 1; i <= 4; i++)
                Bombas.Add(new BombaViewModel(i));

            ActualizarBombasLibres();
            ActualizarEstadisticas();

            _arduino.MensajeRecibido += OnMensajeArduino;
            _arduino.ErrorRecibido   += msg => MessageBox.Show(msg, "Error Serial");

            IniciarServicioCommand = new RelayCommand(_ => IniciarServicio(), _ => PuedeIniciar());
            CancelarCommand        = new RelayCommand(_ => LimpiarFormulario());
            DetenerBombaCommand    = new RelayCommand(b => DetenerBomba(b as BombaViewModel));
            NavPanelCommand        = new RelayCommand(_ => { /* cambiar vista */ });
            NavHistorialCommand    = new RelayCommand(_ => { /* cambiar vista */ });
            NavCierreCajaCommand   = new RelayCommand(_ => { /* cambiar vista */ });
        }

        // Lógica 

        private bool PuedeIniciar() =>
            !string.IsNullOrWhiteSpace(ClienteNuevo) &&
            BombaSeleccionada != null &&
            BombaSeleccionada.EstaLibre;

        private async void IniciarServicio()
        {
            if (BombaSeleccionada == null) return;

            var tipo   = EsPrepago ? TipoServicio.Prepago : TipoServicio.TanqueLleno;
            var litros = EsPrepago && PrecioLitro > 0 ? MontoPrepago / PrecioLitro : 0;

            // Guardar en historial
            var abast = new Abastecimiento
            {
                BombaId        = BombaSeleccionada.Id,
                Cliente        = ClienteNuevo,
                Tipo           = tipo,
                MontoPagado    = EsPrepago ? MontoPrepago : 0,
                LitrosSolicitados = litros,
                FechaHora      = DateTime.Now
            };
            _historial.Agregar(abast);

            // Actualizar UI bomba
            BombaSeleccionada.IniciarServicio(ClienteNuevo, tipo, abast.MontoPagado, litros);

            // Enviar JSON a Arduino
            await _arduino.EnviarComandoAsync(new ComandoBomba
            {
                tipo      = "iniciar",
                bomba_id  = BombaSeleccionada.Id,
                modo      = EsPrepago ? "prepago" : "tanque_lleno",
                litros_max = litros
            });

            LimpiarFormulario();
            ActualizarBombasLibres();
        }

        private async void DetenerBomba(BombaViewModel? bomba)
        {
            if (bomba == null || !bomba.EstaActiva) return;
            await _arduino.EnviarComandoAsync(new ComandoBomba { tipo = "detener", bomba_id = bomba.Id });
        }

        private void OnMensajeArduino(RespuestaBomba resp)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var bomba = Bombas.FirstOrDefault(b => b.Id == resp.bomba_id);
                if (bomba == null) return;

                bomba.ActualizarProgreso(resp.litros_servidos);

                if (resp.completado)
                {
                    // Actualizar historial
                    var registros = _historial.ObtenerTodos();
                    var ultimo = registros.LastOrDefault(a => a.BombaId == resp.bomba_id && !a.Completado);
                    if (ultimo != null)
                    {
                        ultimo.LitrosServidos = resp.litros_servidos;
                        ultimo.MontoFinal     = resp.litros_servidos * PrecioLitro;
                        ultimo.Completado     = true;
                        _historial.Actualizar(ultimo);
                    }

                    bomba.FinalizarServicio(bomba.ClienteActual);
                    ActualizarBombasLibres();
                    ActualizarEstadisticas();
                }
            });
        }

        private void LimpiarFormulario()
        {
            ClienteNuevo  = "";
            MontoPrepago  = 0;
            TipoSeleccionado = 0;
        }

        private void ActualizarBombasLibres()
        {
            BombasLibres.Clear();
            foreach (var b in Bombas.Where(b => b.EstaLibre))
                BombasLibres.Add(b);
            BombaSeleccionada = BombasLibres.FirstOrDefault();
        }

        private void ActualizarEstadisticas()
        {
            var hoy = _historial.CierreDiario(DateTime.Today);
            TotalHoy             = hoy.Sum(a => a.MontoFinal);
            ServiciosHoy         = hoy.Count;
            ServiciosPrepago     = hoy.Count(a => a.Tipo == TipoServicio.Prepago);
            ServiciosTanqueLleno = hoy.Count(a => a.Tipo == TipoServicio.TanqueLleno);

            var grupos = _historial.ObtenerTodos()
                .GroupBy(a => a.BombaId)
                .Select(g => new { Id = g.Key, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .FirstOrDefault();

            if (grupos != null)
            {
                BombaLider         = $"B{grupos.Id}";
                BombaLiderServicios = grupos.Total;
            }
        }
    }
}
