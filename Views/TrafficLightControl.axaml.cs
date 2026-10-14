using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace IPC.Views;

public enum TrafficLightState
{
    Off,
    Red,
    Yellow,
    Green
}

public partial class TrafficLightControl : UserControl
{
    public static readonly StyledProperty<TrafficLightState> StateProperty =
        AvaloniaProperty.Register<TrafficLightControl, TrafficLightState>(
            nameof(State), TrafficLightState.Off);

    public TrafficLightState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public TrafficLightControl()
    {
        InitializeComponent();
        UpdateVisuals(State);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StateProperty)
            UpdateVisuals(change.GetNewValue<TrafficLightState>());
    }

    private void UpdateVisuals(TrafficLightState state)
    {
        LightCircle.Fill = state switch
        {
            TrafficLightState.Red => Brushes.Red,
            TrafficLightState.Yellow => Brushes.Yellow,
            TrafficLightState.Green => Brushes.Green,
            _ => Brushes.Gray
        };
    }
}
