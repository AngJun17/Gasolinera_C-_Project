using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasoStation.ViewModels
{
    public class UsoBombaItem
    {
        public string Nombre { get; set; } = "";
        public int TotalServicios { get; set; }
        public decimal TotalMonto { get; set; }
        public double AnchoBarraUso { get; set; } // px relativo al máximo
        public string EsLider { get; set; } = "";
    }
}
