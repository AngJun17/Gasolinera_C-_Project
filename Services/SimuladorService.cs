using System;
using System.Threading;
using System.Threading.Tasks;
using GasoStation.Models;

namespace GasoStation.Services
{
    public class SimuladorService
    {
        public event Action<RespuestaBomba>? MensajeRecibido;

        private readonly CancellationTokenSource[] _tokens =
            new CancellationTokenSource[5];

        public void IniciarBomba(
            int bombaId,
            string modo,
            decimal litrosMax)
        {
            _tokens[bombaId]?.Cancel();

            _tokens[bombaId] =
                new CancellationTokenSource();

            var token =
                _tokens[bombaId].Token;

            Task.Run(async () =>
            {
                decimal progreso = 0;
                decimal total =
                    litrosMax > 0
                    ? litrosMax
                    : 5;

                while (
                    progreso < total &&
                    !token.IsCancellationRequested)
                {
                    await Task.Delay(500);

                    progreso += total / 20m;

                    if (progreso > total)
                        progreso = total;

                    bool finalizado =
                        progreso >= total;

                    MensajeRecibido?.Invoke(
                        new RespuestaBomba
                        {
                            bomba = bombaId,
                            estado =
                                finalizado
                                ? "finalizada"
                                : "llenando",

                            gal = (float)progreso,
                            galt = (float)total,

                            q = (int)(progreso * 30),
                            qt = (int)(total * 30)
                        });

                    if (finalizado)
                        break;
                }
            }, token);
        }

        public void DetenerBomba(int bombaId)
        {
            _tokens[bombaId]?.Cancel();

            MensajeRecibido?.Invoke(
                new RespuestaBomba
                {
                    bomba = bombaId,
                    estado = "finalizada",
                    gal = 0,
                    galt = 0,
                    q = 0,
                    qt = 0
                });
        }
    }
}