using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;
using TheRFramework.Utilities;

namespace FaultIndicator_MainIPConfig.SerialPortDevice
{ 
    public class PortSettingsViewModel : BaseViewModel
    {
        private string _selectedComPort;
        private string _selectedBaudRate;
        private string _selectedParityBits;
        private string _selectedDataBits;
        private SettingsItemViewModel _selectedStopBits;
        private SettingsItemViewModel _handShake;
        private bool _canEditControls = true;

        public string SelectedCOMPort 
        {
            get => _selectedComPort; 
            set => RaisePropertyChanged(ref _selectedComPort, value); 
        }
        public string SelectedBaudRate
        {
            get => _selectedBaudRate;
            set => RaisePropertyChanged(ref _selectedBaudRate, value);
        }
        public SettingsItemViewModel SelectedStopBits
        {
            get => _selectedStopBits;
            set => RaisePropertyChanged(ref _selectedStopBits, value);
        }
        public string SelectedParityBits
        {
            get => _selectedParityBits;
            set => RaisePropertyChanged(ref _selectedParityBits, value);
        }
        public string SelectedDataBits
        {
            get => _selectedDataBits;
            set => RaisePropertyChanged(ref _selectedDataBits, value);
        }
        public SettingsItemViewModel SelectedHandshake
        {
            get => _handShake;
            set => RaisePropertyChanged(ref _handShake, value);
        }
        public bool CanEditControls
        {
            get => _canEditControls;
            set => RaisePropertyChanged(ref _canEditControls, value);
        }

        public ObservableCollection<string> AvaliablePorts { get; set; }

        public ObservableCollection<int> BaudRates { get; set; }

        public ObservableCollection<int> DataBits { get; set; }

        public Command RefreshPortsCommand { get; }

        public PortSettingsViewModel() 
        {
            AvaliablePorts = new ObservableCollection<string>();
            RefreshPortsCommand = new Command(RefreshPorts);
            RefreshPorts();
        }

        private void RefreshPorts()
        {
            //string LocalSave = SelectedCOMPort;
            AvaliablePorts.Clear();
            foreach (string comPort in SerialPort.GetPortNames())
            {
                AvaliablePorts.Add(comPort);
            }
            //if (AvaliablePorts.Count > 0)
            //{
                //SelectedCOMPort = AvaliablePorts[0];
            //}
            //else
            //{
                // default to COM1 just in case
                // нет, он знаят по дефолту
            SelectedCOMPort = "";
            //}
        }

        public StopBits GetStopBits()
        {
            if (SelectedStopBits != null)
            {
                if (Enum.TryParse(SelectedStopBits.RealName, out StopBits stopBits))
                {
                    return stopBits;
                }
            }
            return StopBits.One;
        }

        public Parity GetParity()
        {
            if (Enum.TryParse(SelectedParityBits ?? "None", out Parity parity))
            {
                return parity;
            }
            return Parity.None;
        }

        public Handshake GetHandshake()
        {
            if (SelectedHandshake != null)
            {
                if (Enum.TryParse(SelectedHandshake.RealName, out Handshake handshake))
                {
                    return handshake;
                }
            }
            return Handshake.None;
        }

        public string GetCOMPort()
        {
            // default to COM1 just in case
            return SelectedCOMPort ?? "COM1";
        }

        public int GetBaudRate()
        {
            if (int.TryParse(SelectedBaudRate ?? "0", out int baudRate))
            {
                return baudRate;
            }
            return 0;
        }

        public int GetDataBits()
        {
            if (int.TryParse(SelectedDataBits ?? "8", out int dataBits))
            {
                return dataBits;
            }
            return 0;
        }
    }
}
