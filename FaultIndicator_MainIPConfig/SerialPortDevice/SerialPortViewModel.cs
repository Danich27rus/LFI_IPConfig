using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using TheRFramework.Utilities;
using FaultIndicator_MainIPConfig.SerialPMessages;
using FaultIndicator_MainIPConfig.SerialPortDevice;
using System.Threading;

namespace FaultIndicator_MainIPConfig.SerialPortDevice
{
    public class SerialPortViewModel : BaseViewModel
    {
        // will be used to bind to the currently connected port
        private string _connectedPort;

        private string[] _request;

        public string ConnectedPort
        {
            get => _connectedPort;
            set => RaisePropertyChanged(ref _connectedPort, value);
        }

        public SerialPort Port { get; set; }

        private bool _isConnected;

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                RaisePropertyChanged(ref _isConnected, value);
                Settings.CanEditControls = !value;
            }
        }

        public string[] Request
        {
            get => _request;
            set => RaisePropertyChanged(ref _request, value);
        }

        public void CloseAll()
        {
            Disconnect();
            Receiver.StopThreadLoop();
        }
        // Because a button is used to connect/disconnect, well....
        public Command AutoConnectDisconnectCommand { get; }
        // Not really that useful, but can be used to clear the serialport's receive/send buffers, but they probably wont fill up unless you send a giant message and noone responds... sort of
        public Command ClearBuffersCommand { get; }

        public PortSettingsViewModel Settings { get; set; }

        public SerialPortMessagesViewModel Messages { get; set; }

        public SerialPortMessagesReceive Receiver { get; set; }

        public SerialPortMessagesSend Sender { get; set; }

        public SerialPortViewModel()
        {
            ConnectedPort = "None";
            Port = new SerialPort
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
            Settings = new PortSettingsViewModel();

            AutoConnectDisconnectCommand = new Command(AutoConnectDisconnect);
            ClearBuffersCommand = new Command(ClearBuffers);
        }

        public void AutoConnectDisconnect()
        {
            IsConnected = Port.IsOpen; 
            if (IsConnected)
            {
                Disconnect();
            }
            else
            {
                Connect();
            }
        }

        public void Connect()
        {
            IsConnected = Port.IsOpen;
            //Port.PortName = null;
            //Port.BaudRate = 0;
            Port.StopBits = StopBits.One;
            Port.Parity = Parity.None;
            Port.DataBits = 8;
            Port.Handshake = Handshake.None;
            Sender.InitializationPackage = true;
            if (IsConnected)
            {
                Messages.AddMessage("Порт уже открыт!");
                return;
            }
            if (Settings.SelectedCOMPort == "COM1")
            {
                Messages.AddMessage("Нельзя использовать COM1");
                return;
            }
            if (string.IsNullOrEmpty(Settings.SelectedCOMPort))
            {
                Messages.AddMessage("Ошибка с конфигурацией COM порта. Проверьте, выбрали ли все пункты в настройках");
                return;
            }
            Port.PortName = Settings.GetCOMPort();
            Port.BaudRate = Settings.GetBaudRate();
            //Port.DataBits = Settings.GetDataBits();
            //Port.StopBits = Settings.GetStopBits();
            //Port.Parity = Settings.GetParity();
            //Port.Handshake = Settings.GetHandshake();
            try
            {
                Port.Open();
            }
            catch(Exception ex)
            {
                Messages.AddMessage($"Ошибка приложения: {ex.Message}");
                return;
            }
            ConnectedPort = Settings.SelectedCOMPort;
            Messages.AddMessage($"Подключен к порту {ConnectedPort}!");
            IsConnected = true;
            Receiver.CanReceive = true;
            /*if (Sender.InitializationPackage)
            {
                new Thread(() => {
                    Request = new string[]
                    {
                    "68",
                    "04",
                    "07",
                    "00",
                    "00",
                    "00"
                    };

                    Sender.SendHEXMessage(string.Join(" ", Request));
                    Messages.AddSentMessage(string.Join(" ", Request));

                    Thread.Sleep(500);

                }).Start();

                Sender.InitializationPackage = false;
            }*/
        }

        public void Disconnect()
        {
            IsConnected = Port.IsOpen;
            if (!IsConnected)
            {
                Messages.AddMessage("Порт уже закрыт!");
                return;
            }
            try
            {
                Port.Close();
            }
            catch(Exception ex)
            {
                Messages.AddMessage($"Ошибка закрытия порта: {ex.Message}!");
                return;
            }

            Messages.AddMessage($"Отключен от порта {ConnectedPort}");
            ConnectedPort = "None";
            IsConnected = Port.IsOpen;
            Receiver.CanReceive = false;
        }

        private void ClearBuffers()
        {
            if (!Port.IsOpen)
            {
                Messages.AddMessage("Необходимо подключение к порту, чтобы очистить буффер");
                return;
            }
            Port.DiscardInBuffer();
            Port.DiscardOutBuffer();
        }

    }
}
