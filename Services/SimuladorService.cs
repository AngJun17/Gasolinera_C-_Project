using System;
using System.Threading;
using System.Threading.Tasks;
using GasoStation.Models;

namespace GasoStation.Services
{
    /// <summary>
    /// Simula el Arduino cuando no está conectado.
    /// Envía mensajes de progreso cada 500ms hasta completar el servicio.
    /// </summary>
    public class SimuladorService
    {
        public event Action<RespuestaBomba>? MensajeRecibido;

        private readonly CancellationTokenSource[] _tokens = new CancellationTokenSource[5]; // índice 1-4

        public void IniciarBomba(int bombaId, string modo, decimal litrosMax)
        {
            // Cancelar si ya había algo corriendo en esa bomba
            _tokens[bombaId]?.Cancel();
            _tokens[bombaId] = new CancellationTokenSource();
            var token = _tokens[bombaId].Token;

            Task.Run(async () =>
            {
                decimal litrosServidos = 0;
                decimal incremento = modo == "prepago" ? litrosMax / 20m : 0.25m; // 20 pasos para prepago
                decimal limite = modo == "prepago" ? litrosMax : 50m; // tanque lleno: máx 50L simulado

                while (litrosServidos < limite && !token.IsCancellationRequested)
                {
                    await Task.Delay(500, token).ContinueWith(_ => { }); // no lanzar si se cancela
                    if (token.IsCancellationRequested) break;

                    litrosServidos = Math.Min(litrosServidos + incremento, limite);
                    bool completado = litrosServidos >= limite;

                    MensajeRecibido?.Invoke(new RespuestaBomba
                    {
                        evento          = completado ? "completado" : "progreso",
                        bomba_id        = bombaId,
                        litros_servidos = litrosServidos,
                        completado      = completado
                    });

                    if (completado) break;
                }
            }, token);
        }

        public void DetenerBomba(int bombaId)
        {
            _tokens[bombaId]?.Cancel();

            // Notificar que se detuvo con los litros actuales (el VM ya los tiene)
            MensajeRecibido?.Invoke(new RespuestaBomba
            {
                evento          = "completado",
                bomba_id        = bombaId,
                litros_servidos = 0, // el VM usa el último valor que ya tenía
                completado      = true,
                mensaje         = "detenido_manual"
            });
        }
    }
}
