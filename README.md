# Gasolinera_C-_Project
Proyecto Final de Programacion 3

# GasoStation — Panel Central WPF

## Estructura de archivos (MVVM)

```
GasoStation/
│
├── Models/
│   ├── Bomba.cs              # Entidad bomba + enums EstadoBomba / TipoServicio
│   ├── Abastecimiento.cs     # Registro de cada servicio (persiste en JSON)
│   └── MensajeJson.cs        # ComandoBomba (PC→Arduino) / RespuestaBomba (Arduino→PC)
│
├── ViewModels/
│   ├── MainViewModel.cs      # Lógica principal: bombas, formulario, estadísticas
│   └── BombaViewModel.cs     # Estado reactivo de cada bomba individual
│
├── Views/
│   ├── MainWindow.xaml       # UI principal (tema oscuro, diseño minimalista)
│   └── MainWindow.xaml.cs    # Code-behind mínimo (drag window, close)
│
├── Services/
│   ├── ArduinoService.cs     # Serial port: enviar/recibir JSON con Arduino
│   └── HistorialService.cs   # CRUD + informes sobre historial.json
│
├── Helpers/
│   ├── ViewModelBase.cs      # INotifyPropertyChanged base
│   ├── RelayCommand.cs       # ICommand genérico
│   └── Converters.cs         # Value converters para el XAML
│
├── Resources/
│   ├── Colors.xaml           # Paleta completa de colores como recursos
│   └── Styles.xaml           # Estilos globales (botones, inputs, textos)
│
├── App.xaml                  # Entry point, ResourceDictionary global
├── App.xaml.cs
└── GasoStation.csproj        # .NET 8 / WPF
```

## JSON entre Arduino y PC

**PC → Arduino (iniciar servicio):**
```json
{
  "tipo": "iniciar",
  "bomba_id": 2,
  "modo": "prepago",
  "litros_max": 4.76
}
```

**Arduino → PC (progreso/completado):**
```json
{
  "evento": "completado",
  "bomba_id": 2,
  "litros_servidos": 4.76,
  "completado": true
}
```

## Historial (historial.json)
Se genera automáticamente en el directorio del ejecutable.
Cada abastecimiento guarda: id, bomba, cliente, tipo, monto, litros, fecha/hora.

## Requisitos
- .NET 8 SDK
- Visual Studio 2022 o Rider
- Arduino con firmware que lea/escriba JSON por Serial (9600 baud)

