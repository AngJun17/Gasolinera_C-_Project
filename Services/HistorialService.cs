using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GasoStation.Models;

namespace GasoStation.Services
{
    public class HistorialService
    {
        private readonly string _ruta;
        private List<Abastecimiento> _historial = new();
        private int _nextId = 1;

        public HistorialService(string ruta = "historial.json")
        {
            _ruta = ruta;
            Cargar();
        }

        //CRUD 

        public void Agregar(Abastecimiento a)
        {
            a.Id = _nextId++;
            _historial.Add(a);
            Guardar();
        }

        public void Actualizar(Abastecimiento a)
        {
            var idx = _historial.FindIndex(x => x.Id == a.Id);
            if (idx >= 0) { _historial[idx] = a; Guardar(); }
        }

        public List<Abastecimiento> ObtenerTodos() => _historial.ToList();

        //INFORMES 

        public List<Abastecimiento> CierreDiario(DateTime fecha)
            => _historial.Where(a => a.FechaHora.Date == fecha.Date).ToList();

        public List<Abastecimiento> InformePrepago()
            => _historial.Where(a => a.Tipo == TipoServicio.Prepago).ToList();

        public List<Abastecimiento> InformeTanqueLleno()
            => _historial.Where(a => a.Tipo == TipoServicio.TanqueLleno).ToList();

        public (int bombaMaxId, int bombaMinId) BombasMasYMenosUsadas()
        {
            var grupos = _historial
                .GroupBy(a => a.BombaId)
                .Select(g => new { BombaId = g.Key, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToList();

            return grupos.Count == 0
                ? (0, 0)
                : (grupos.First().BombaId, grupos.Last().BombaId);
        }

        public decimal TotalRecaudadoHoy()
            => CierreDiario(DateTime.Today).Sum(a => a.MontoFinal);

        public int ServiciosHoy()
            => CierreDiario(DateTime.Today).Count;

        //PERSISTENCIA

        private void Guardar()
        {
            var json = JsonSerializer.Serialize(_historial, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_ruta, json);
        }

        private void Cargar()
        {
            if (!File.Exists(_ruta)) return;
            try
            {
                var json = File.ReadAllText(_ruta);
                _historial = JsonSerializer.Deserialize<List<Abastecimiento>>(json) ?? new();
                _nextId = _historial.Count > 0 ? _historial.Max(a => a.Id) + 1 : 1;
            }
            catch { _historial = new(); }
        }
    }
}
