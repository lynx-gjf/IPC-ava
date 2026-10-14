using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace IPC.Views;

public partial class MainWindow : Window
{
    private readonly SerialPortManager _serialPortManager = new();

    public MainWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            var tabControl = this.FindControl<TabControl>("MainTabControl")
                ?? throw new InvalidOperationException("MainTabControl was not found.");
            tabControl.SelectedIndex = 0;
            return;
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _serialPortManager.Encoding = Encoding.UTF8;
        _serialPortManager.DataReceived += OnDataReceived;
        RefreshPorts();
        Closed += OnClosed;
    }

    private void RefreshPorts()
    {
        var ports = _serialPortManager.GetPortNames();
        PortComboBox.ItemsSource = ports;
        PortComboBox.SelectedIndex = ports.Length > 0 ? 0 : -1;
    }

    private void OnDataReceived(string data)
    {
        Dispatcher.UIThread.Post(() =>
        {
            AppendText(DebugReceiveTextBox, data);
            AppendText(DataReceiveTextBox, data);
        });
    }

    private static void AppendText(TextBox textBox, string data)
    {
        textBox.Text += $"[{DateTime.Now:HH:mm:ss.fff}] {data}";
        if (!textBox.Text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            textBox.Text += Environment.NewLine;
        textBox.CaretIndex = textBox.Text.Length;
    }

    private async void OpenCloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_serialPortManager.IsOpen)
        {
            _serialPortManager.Close();
            OpenCloseButton.Content = "打开串口";
            PortComboBox.IsEnabled = true;
            return;
        }

        if (PortComboBox.SelectedItem is not string portName || string.IsNullOrWhiteSpace(portName))
        {
            await ShowMessage("提示", "请选择串口");
            return;
        }

        try
        {
            _serialPortManager.Open(portName, 115200);
            OpenCloseButton.Content = "关闭串口";
            PortComboBox.IsEnabled = false;
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
        {
            await ShowMessage("错误", $"打开串口失败: {ex.Message}");
        }
    }

    private void RefreshPortsButton_OnClick(object? sender, RoutedEventArgs e) => RefreshPorts();

    private async void PumpOpenCloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string tag)
            return;

        var pumpNumber = PumpNumber(tag);
        if (pumpNumber is null)
            return;

        var opening = string.Equals(button.Content?.ToString(), "开启注射泵", StringComparison.Ordinal);
        try
        {
            _serialPortManager.SendString($"R{pumpNumber},P1,G1,{(opening ? "1" : "0")}");
            button.Content = opening ? "关闭注射泵" : "开启注射泵";
        }
        catch (InvalidOperationException ex)
        {
            await ShowMessage("错误", $"发送失败: {ex.Message}");
        }
    }

    private async void RateSetButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string tag)
            return;

        var pumpNumber = PumpNumber(tag);
        var source = tag switch
        {
            "R1" => FlowRate1,
            "R2" => FlowRate2,
            "R3" => FlowRate3,
            _ => null
        };
        var display = tag switch
        {
            "R1" => RateShow1,
            "R2" => RateShow2,
            "R3" => RateShow3,
            _ => null
        };

        if (pumpNumber is null || source is null || display is null)
            return;

        var input = (source.Text ?? string.Empty).Trim().Replace(',', '.');
        if (!decimal.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var rate) ||
            rate < 0m || rate > 9.9999m)
        {
            await ShowMessage("提示", "流速必须在 0 到 9.9999 ml/min 之间");
            return;
        }

        display.Text = source.Text;
        var raw = (int)Math.Round(rate * 10000m, MidpointRounding.AwayFromZero);
        try
        {
            _serialPortManager.SendString($"R{pumpNumber},P1,S3,{raw:D5}");
        }
        catch (InvalidOperationException ex)
        {
            await ShowMessage("错误", $"发送失败: {ex.Message}");
        }
    }

    private async void DebugSendButton_OnClick(object? sender, RoutedEventArgs e) =>
        await SendFrom(DebugSendTextBox, clearAfterSend: true);

    private async void DataSendButton_OnClick(object? sender, RoutedEventArgs e) =>
        await SendFrom(DataSendTextBox, clearAfterSend: false);

    private async Task SendFrom(TextBox source, bool clearAfterSend)
    {
        if (string.IsNullOrWhiteSpace(source.Text))
            return;

        try
        {
            _serialPortManager.SendString(source.Text);
            if (clearAfterSend)
                source.Clear();
        }
        catch (InvalidOperationException ex)
        {
            await ShowMessage("错误", $"发送失败: {ex.Message}");
        }
    }

    private void Numeric_OnLostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        var input = (textBox.Text ?? string.Empty).Replace(',', '.');
        textBox.Text = decimal.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value.ToString("0.000", CultureInfo.InvariantCulture)
            : "0.000";
    }

    private static string? PumpNumber(string tag) => tag switch
    {
        "R1" => "1",
        "R2" => "2",
        "R3" => "3",
        _ => null
    };

    private async Task ShowMessage(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 360,
            Height = 150,
            Content = new StackPanel
            {
                Margin = new Thickness(16),
                Spacing = 16,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    new Button { Content = "确定", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right }
                }
            }
        };
        if (dialog.Content is Panel panel && panel.Children[1] is Button button)
            button.Click += (_, _) => dialog.Close();
        await dialog.ShowDialog(this);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _serialPortManager.Dispose();
        Closed -= OnClosed;
    }
}
