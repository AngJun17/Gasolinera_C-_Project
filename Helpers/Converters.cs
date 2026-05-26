using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using GasoStation.Models;

namespace GasoStation.Helpers
{
    //bool → Visibility 
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            bool b = value is bool bv && bv;
            if (Invert) b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    //EstadoBomba → color del borde de la tarjeta 
    public class EstadoToBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value is EstadoBomba estado)
                return estado switch
                {
                    EstadoBomba.Activa   => new SolidColorBrush(Color.FromRgb(0xE9, 0x45, 0x60)), // rojo
                    EstadoBomba.EnEspera => new SolidColorBrush(Color.FromRgb(0xF5, 0xC8, 0x42)), // amarillo
                    _                    => new SolidColorBrush(Color.FromRgb(0x27, 0xC9, 0x3F)), // verde libre
                };
            return Brushes.Transparent;
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    //EstadoBomba → color del número e icono 
    public class EstadoToNumColorConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value is EstadoBomba estado)
                return estado switch
                {
                    EstadoBomba.Activa   => new SolidColorBrush(Color.FromRgb(0xE9, 0x45, 0x60)),
                    EstadoBomba.EnEspera => new SolidColorBrush(Color.FromRgb(0xF5, 0xC8, 0x42)),
                    _                    => new SolidColorBrush(Color.FromRgb(0x27, 0xC9, 0x3F)),
                };
            return Brushes.Gray;
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    // TipoServicio → string legible 
    public class TipoServicioConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is TipoServicio tp ? (tp == TipoServicio.Prepago ? "Prepago" : "Tanque lleno") : "";
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    // decimal → "Q 1,234.56" 
    public class QuetzalConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is decimal d ? $"Q {d:N2}" : "Q 0.00";
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }
}
