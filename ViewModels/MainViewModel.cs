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
        private readonly ArduinoService _arduino = new();
        private readonly SimuladorService _simulador = new();

        // simulación 
        private bool _modoSimulacion = true;
        public bool ModoSimulacion
        {
            get => _modoSimulacion;
            set { SetProperty(ref _modoSimulacion, value); OnPropertyChanged(nameof(TextoModo)); }
        }
        public string TextoModo => _modoSimulacion ? " Modo simulación" : " Arduino real";

        // Navegación 
        private object? _vistaActual;
        public object? VistaActual { get => _vistaActual; set => SetProperty(ref _vistaActual, value); }

        // Bombas
        public ObservableCollection<BombaViewModel> Bombas { get; } = new();
        public ObservableCollection<BombaViewModel> BombasLibres { get; } = new();

        //Formulario 
        private string _clienteNuevo = "";
        public string ClienteNuevo { get => _clienteNuevo; set => SetProperty(ref _clienteNuevo, value); }

        private BombaViewModel? _bombaSeleccionada;
        public BombaViewModel? BombaSeleccionada { get => _bombaSeleccionada; set => SetProperty(ref _bombaSeleccionada, value); }

        private int _tipoSeleccionado = 0;
        public int TipoSeleccionado
        {
            get => _tipoSeleccionado;
            set { SetProperty(ref _tipoSeleccionado, value); OnPropertyChanged(nameof(EsPrepago)); }
        }
        public bool EsPrepago => TipoSeleccionado == 0;

        private decimal _montoPrepago;
        public decimal MontoPrepago { get => _montoPrepago; set => SetProperty(ref _montoPrepago, value); }

        // Stats panel principal
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

        // Precio y conexión
        private decimal _precioLitro = 10.50m;
        public decimal PrecioLitro { get => _precioLitro; set => SetProperty(ref _precioLitro, value); }

        /// <summary>
        /// aca cambiamos el puerto dependiendo de donde se conecte el arduino
        /// </summary>
        private string _puertoSeleccionado = "COM9";
        public string PuertoSeleccionado { get => _puertoSeleccionado; set => SetProperty(ref _puertoSeleccionado, value); }

        private bool _conectado = false;
        public bool Conectado { get => _conectado; set { SetProperty(ref _conectado, value); OnPropertyChanged(nameof(TextoConexion)); } }
        public string TextoConexion => Conectado ? "Conectado" : "Desconectado";

        // Historial 
        public ObservableCollection<Abastecimiento> Historial { get; } = new();

        // Cierre de caja
        private DateTime _fechaCierre = DateTime.Today;
        public DateTime FechaCierre { get => _fechaCierre; set => SetProperty(ref _fechaCierre, value); }

        private decimal _cierreTotalMonto;
        public decimal CierreTotalMonto { get => _cierreTotalMonto; set => SetProperty(ref _cierreTotalMonto, value); }

        private int _cierreTotalServicios;
        public int CierreTotalServicios { get => _cierreTotalServicios; set => SetProperty(ref _cierreTotalServicios, value); }

        private decimal _cierreTotalLitros;
        public decimal CierreTotalLitros { get => _cierreTotalLitros; set => SetProperty(ref _cierreTotalLitros, value); }

        public ObservableCollection<Abastecimiento> CierreDetalle { get; } = new();

        //Informes prepago/tanquelleno
        private string _informeTitulo = "";
        public string InformeTitulo { get => _informeTitulo; set => SetProperty(ref _informeTitulo, value); }

        private int _informeTotalServicios;
        public int InformeTotalServicios { get => _informeTotalServicios; set => SetProperty(ref _informeTotalServicios, value); }

        private decimal _informeTotalMonto;
        public decimal InformeTotalMonto { get => _informeTotalMonto; set => SetProperty(ref _informeTotalMonto, value); }

        public ObservableCollection<Abastecimiento> InformeDetalle { get; } = new();

        // Uso por bomba
        public ObservableCollection<UsoBombaItem> UsoBombas { get; } = new();

        //Comandos 
        public RelayCommand IniciarServicioCommand { get; }
        public RelayCommand CancelarCommand { get; }
        public RelayCommand DetenerBombaCommand { get; }
        public RelayCommand ToggleSimulacionCommand { get; }
        public RelayCommand NavPanelCommand { get; }
        public RelayCommand NavHistorialCommand { get; }
        public RelayCommand NavCierreCajaCommand { get; }
        public RelayCommand NavInformePrepagoCommand { get; }
        public RelayCommand NavInformeTanqueLlenoCommand { get; }
        public RelayCommand NavUsoBombasCommand { get; }
        public RelayCommand GenerarCierreCommand { get; }

       
        public MainViewModel()
        {
            for (int i = 1; i <= 4; i++)
                Bombas.Add(new BombaViewModel(i));

            ActualizarBombasLibres();
            ActualizarEstadisticas();
            CargarHistorial();

            _arduino.MensajeRecibido += OnMensajeArduino;
            _arduino.ErrorRecibido += msg => MessageBox.Show(msg, "Error Serial");
            _simulador.MensajeRecibido += OnMensajeArduino;

            IniciarServicioCommand = new RelayCommand(_ => IniciarServicio(), _ => PuedeIniciar());
            CancelarCommand = new RelayCommand(_ => LimpiarFormulario());
            DetenerBombaCommand = new RelayCommand(b => DetenerBomba(b as BombaViewModel));

            ToggleSimulacionCommand = new RelayCommand(_ =>
            {
                if (ModoSimulacion)
                {
                    bool ok = _arduino.Conectar(PuertoSeleccionado);
                    if (ok) { ModoSimulacion = false; Conectado = true; }
                    else MessageBox.Show($"No se pudo conectar en {PuertoSeleccionado}.\nSiguiendo en modo simulación.", "Conexión");
                }
                else { _arduino.Desconectar(); Conectado = false; ModoSimulacion = true; }
            });

            // Navegación — cada comando carga su vista y sus datos
            NavPanelCommand = new RelayCommand(_ => VistaActual = null); // null = panel principal
            NavHistorialCommand = new RelayCommand(_ => { CargarHistorial(); VistaActual = "Historial"; });
            NavCierreCajaCommand = new RelayCommand(_ => { GenerarCierre(); VistaActual = "CierreCaja"; });
            NavInformePrepagoCommand = new RelayCommand(_ => { CargarInforme(TipoServicio.Prepago); VistaActual = "Informe"; });
            NavInformeTanqueLlenoCommand = new RelayCommand(_ => { CargarInforme(TipoServicio.TanqueLleno); VistaActual = "Informe"; });
            NavUsoBombasCommand = new RelayCommand(_ => { CargarUsoBombas(); VistaActual = "UsoBombas"; });
            GenerarCierreCommand = new RelayCommand(_ => GenerarCierre());
        }

        // Lógica de servicio 

        private bool PuedeIniciar() =>
            !string.IsNullOrWhiteSpace(ClienteNuevo) &&
            BombaSeleccionada != null && BombaSeleccionada.EstaLibre;

        private async void IniciarServicio()
        {
            int montoQ = (int)MontoPrepago;

            int ml = (int)((MontoPrepago / 30m) * 300m);
            if (BombaSeleccionada == null) return;
            var tipo = EsPrepago ? TipoServicio.Prepago : TipoServicio.TanqueLleno;
            var litros = EsPrepago && PrecioLitro > 0 ? MontoPrepago / PrecioLitro : 0;

            var abast = new Abastecimiento
            {
                BombaId = BombaSeleccionada.Id,
                Cliente = ClienteNuevo,
                Tipo = tipo,
                MontoPagado = EsPrepago ? MontoPrepago : 0,
                LitrosSolicitados = litros,
                FechaHora = DateTime.Now
            };
            _historial.Agregar(abast);
            BombaSeleccionada.IniciarServicio(ClienteNuevo, tipo, abast.MontoPagado, litros);

            if (ModoSimulacion)
                _simulador.IniciarBomba(BombaSeleccionada.Id, EsPrepago ? "prepago" : "tanque_lleno", litros);
            else
                await _arduino.EnviarComandoAsync(
                    new ComandoBomba
                    {
                        cmd = "iniciar",
                        bomba = BombaSeleccionada.Id,
                        modo = EsPrepago
                            ? "prepago"
                            : "lleno",
                        q = montoQ,
                        ml = ml
                    });
            LimpiarFormulario();
            ActualizarBombasLibres();
        }

        private async void DetenerBomba(BombaViewModel? bomba)
        {
            if (bomba == null || !bomba.EstaActiva) return;
            if (ModoSimulacion) _simulador.DetenerBomba(bomba.Id);
            else await _arduino.EnviarComandoAsync(
                new ComandoBomba
                {
                    cmd = "stop"
                });
        }

        private void OnMensajeArduino(RespuestaBomba resp)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var bomba =
                    Bombas.FirstOrDefault(
                        b => b.Id == resp.bomba);

                if (bomba == null)
                    return;

                if (resp.estado == "llenando")
                {
                    bomba.ActualizarProgreso(
                        (decimal)resp.gal);
                }

                if (resp.estado == "finalizada")
                {
                    var registros =
                        _historial.ObtenerTodos();

                    var ultimo =
                        registros.LastOrDefault(
                            a =>
                                a.BombaId == resp.bomba &&
                                !a.Completado);

                    if (ultimo != null)
                    {
                        ultimo.LitrosServidos =
                            (decimal)resp.gal;

                        ultimo.MontoFinal =
                            resp.q;

                        ultimo.Completado = true;

                        _historial.Actualizar(
                            ultimo);
                    }

                    bomba.FinalizarServicio(
                        bomba.ClienteActual);

                    ActualizarBombasLibres();

                    ActualizarEstadisticas();
                }
            });
        }

        private void LimpiarFormulario() { ClienteNuevo = ""; MontoPrepago = 0; TipoSeleccionado = 0; }

        private void ActualizarBombasLibres()
        {
            BombasLibres.Clear();
            foreach (var b in Bombas.Where(b => b.EstaLibre)) BombasLibres.Add(b);
            BombaSeleccionada = BombasLibres.FirstOrDefault();
        }

        private void ActualizarEstadisticas()
        {
            var hoy = _historial.CierreDiario(DateTime.Today);
            TotalHoy = hoy.Sum(a => a.MontoFinal);
            ServiciosHoy = hoy.Count;
            ServiciosPrepago = hoy.Count(a => a.Tipo == TipoServicio.Prepago);
            ServiciosTanqueLleno = hoy.Count(a => a.Tipo == TipoServicio.TanqueLleno);

            var lider = _historial.ObtenerTodos()
                .GroupBy(a => a.BombaId)
                .Select(g => new { Id = g.Key, Total = g.Count() })
                .OrderByDescending(g => g.Total).FirstOrDefault();

            if (lider != null) { BombaLider = $"B{lider.Id}"; BombaLiderServicios = lider.Total; }
        }

        //Datos para vistas secundarias

        private void CargarHistorial()
        {
            Historial.Clear();
            foreach (var a in _historial.ObtenerTodos().OrderByDescending(a => a.FechaHora))
                Historial.Add(a);
        }

        private void GenerarCierre()
        {
            var lista = _historial.CierreDiario(FechaCierre);
            CierreTotalServicios = lista.Count;
            CierreTotalMonto = lista.Sum(a => a.MontoFinal);
            CierreTotalLitros = lista.Sum(a => a.LitrosServidos);
            CierreDetalle.Clear();
            foreach (var a in lista.OrderBy(a => a.FechaHora)) CierreDetalle.Add(a);
        }

        private void CargarInforme(TipoServicio tipo)
        {
            var lista = tipo == TipoServicio.Prepago
                ? _historial.InformePrepago()
                : _historial.InformeTanqueLleno();

            InformeTitulo = tipo == TipoServicio.Prepago ? "INFORME PREPAGO" : "INFORME TANQUE LLENO";
            InformeTotalServicios = lista.Count;
            InformeTotalMonto = lista.Sum(a => a.MontoFinal);
            InformeDetalle.Clear();
            foreach (var a in lista.OrderByDescending(a => a.FechaHora)) InformeDetalle.Add(a);
        }

        private void CargarUsoBombas()
        {
            var todos = _historial.ObtenerTodos();
            int maxServicios = todos.Any()
                ? todos.GroupBy(a => a.BombaId).Max(g => g.Count())
                : 1;

            UsoBombas.Clear();
            for (int i = 1; i <= 4; i++)
            {
                var grupo = todos.Where(a => a.BombaId == i).ToList();
                int total = grupo.Count;
                bool esLider = total == maxServicios && total > 0;
                UsoBombas.Add(new UsoBombaItem
                {
                    Nombre = $"Bomba {i}",
                    TotalServicios = total,
                    TotalMonto = grupo.Sum(a => a.MontoFinal),
                    AnchoBarraUso = maxServicios > 0 ? (double)total / maxServicios * 300 : 0,
                    EsLider = esLider ? " Más usada" : (total == todos.GroupBy(a => a.BombaId).Min(g => g.Count()) && total < maxServicios ? "Menos usada" : "")
                });
            }
        }
    }
}