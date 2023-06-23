using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FaultIndicator_MainIPConfig.SerialPMessages;
using FaultIndicator_MainIPConfig.SerialPortDevice;
//using test_app2.Config;
using FaultIndicator_MainIPConfig.UI;
//using test_app2.FaultIndicators;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FaultIndicator_MainIPConfig.ViewModels
{
    internal class MainViewModel
    { 
        //19 00 00 04 08 00 00 00 00 00 00 F4 8B
        //тест для последовательного порта через КДТН
        public SerialPortMessagesViewModel Messages { get; set; }

        //public FaultIndicatorViewModel FaultIndicator { get; set; }

        //public IndicatorDataViewModel IndicatorData { get; set; }

        public SerialPortMessagesReceive Receiver { get; set; }

        public SerialPortMessagesSend Sender { get; set; }

        public SerialPortViewModel SerialPort { get; set; }

        //public ConfigViewModel Config { get; set; }

        //public ObservableCollection<FaultIndicatorViewModel> Indicators { get; set; }

        public MainViewModel()
        {
            SerialPort = new SerialPortViewModel();
            Receiver = new SerialPortMessagesReceive();
            Sender = new SerialPortMessagesSend();
            Messages = new SerialPortMessagesViewModel();
            //IndicatorData = new IndicatorDataViewModel();
            //FaultIndicator = new FaultIndicatorViewModel();
            /*Indicators = new ObservableCollection<FaultIndicatorViewModel>
            {
                new FaultIndicatorViewModel() { CallAdress = 25, _callFrequency = 30 }
            };*/
            //Indicators.CollectionChanged += Indicators_CollectionChanged;

            // hmm
            Messages.Sender = Sender;
            Receiver.Messages = Messages;
            Sender.Messages = Messages;

            SerialPort.Receiver = Receiver;
            SerialPort.Sender = Sender;
            SerialPort.Messages = Messages;

            Receiver.Port = SerialPort.Port;
            Sender.Port = SerialPort.Port;

            //Config = new ConfigViewModel(SerialPort, IndicatorData);

            //IndicatorData.Messages = Messages;
            //IndicatorData.Sender = Sender;
            //IndicatorData.SerialPort = SerialPort;
            //IndicatorData.Receiver = Receiver;
            //Receiver.IndicatorData = IndicatorData;
            //IndicatorData.Indicators = Messages.Indicators;
            //IndicatorData.IndicatorConfirm = Receiver.IndicatorConfirm;
            //Config.SerialPort = SerialPort;
            //Config.Port = SerialPort.Port;
            //Config.Messages = SerialPort.Messages;
            //Config.Sender = Sender;
            //Config.Receiver = Receiver;

            //Config.Config.DataContext = this;
        }
    }
}
