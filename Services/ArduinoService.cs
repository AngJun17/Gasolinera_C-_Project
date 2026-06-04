using System;
using System.IO.Ports;
using System.Text.Json;
using System.Threading.Tasks;
using GasoStation.Models;
using System.Text.Json.Serialization;

namespace GasoStation.Services
{
    public class ArduinoService
    {
        private SerialPort _port = new SerialPort();
        public bool Conectado => _port.IsOpen;

        public event Action<RespuestaBomba>? MensajeRecibido;
        public event Action<string>? ErrorRecibido;

        public bool Conectar(string portName, int baudRate = 9600)
        {
            try
            {
                _port = new SerialPort(portName, baudRate)
                {
                    ReadTimeout  = 2000,
                    WriteTimeout = 2000
                };
                _port.DataReceived += OnDataReceived;
                _port.Open();
                return true;
            }
            catch (Exception ex)
            {
                ErrorRecibido?.Invoke($"No se pudo conectar: {ex.Message}");
                return false;
            }
        }

        public void Desconectar()
        {
            if (_port.IsOpen)
            {
                _port.DataReceived -= OnDataReceived;
                _port.Close();
            }
        }

        public async Task EnviarComandoAsync(ComandoBomba comando)
        {
            if (!_port.IsOpen)
            {
                ErrorRecibido?.Invoke("Puerto serial no conectado.");
                return;
            }
            string json = JsonSerializer.Serialize(comando);
            await Task.Run(() => _port.WriteLine(json));
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string linea = _port.ReadLine().Trim();
                var respuesta = JsonSerializer.Deserialize<RespuestaBomba>(
                    linea,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (respuesta != null)
                    MensajeRecibido?.Invoke(respuesta);
            }
            catch (Exception ex)
            {
                ErrorRecibido?.Invoke($"Error al leer datos: {ex.Message}");
            }
        }

        public static string[] ObtenerPuertosDisponibles() => SerialPort.GetPortNames();
    }
}
