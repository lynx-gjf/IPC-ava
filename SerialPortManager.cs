using System;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace IPC;

public sealed class SerialPortManager : IDisposable
{
    private readonly SerialPort _serialPort = new();
    private readonly object _writeLock = new();

    public event Action<string>? DataReceived;

    public Encoding Encoding
    {
        get => _serialPort.Encoding;
        set => _serialPort.Encoding = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool IsOpen => _serialPort.IsOpen;

    public string[] GetPortNames() => SerialPort.GetPortNames();

    public void Open(string portName, int baudRate)
    {
        ArgumentException.ThrowIfNullOrEmpty(portName);

        if (_serialPort.IsOpen)
            return;

        _serialPort.PortName = portName;
        _serialPort.BaudRate = baudRate;
        _serialPort.DataBits = 8;
        _serialPort.StopBits = StopBits.One;
        _serialPort.Parity = Parity.None;
        _serialPort.Handshake = Handshake.None;
        _serialPort.DataReceived += InternalDataReceived;
        _serialPort.Open();
    }

    public void Close()
    {
        if (!_serialPort.IsOpen)
            return;

        _serialPort.DataReceived -= InternalDataReceived;
        _serialPort.Close();
    }

    public void ClearBuffer()
    {
        if (_serialPort.IsOpen && _serialPort.BytesToRead > 0)
            _serialPort.DiscardInBuffer();
    }

    public void SendString(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!_serialPort.IsOpen)
            throw new InvalidOperationException("串口未打开");

        lock (_writeLock)
        {
            _serialPort.Write(text);
        }
    }

    private void InternalDataReceived(object? sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (_serialPort.BytesToRead <= 0)
                return;

            var receivedData = _serialPort.ReadExisting();
            if (!string.IsNullOrEmpty(receivedData))
                DataReceived?.Invoke(receivedData);
        }
        catch (Exception ex) when (ex is InvalidOperationException or
                                   UnauthorizedAccessException or
                                   IOException)
        {
            // The port can close while the driver raises its receive event.
        }
    }

    public void Dispose()
    {
        Close();
        _serialPort.Dispose();
    }
}
